using System.Collections.Generic;
using Unity.Netcode;


namespace k323.Commons.NetworkActionSystem {
    /// <summary>
    /// This is a companion class to ClientCharacter that is specifically responsible for visualizing Actions. 
    /// Action visualizations have lifetimes and ongoing state, making this class closely analogous ServerActionPlayer class.
    /// </summary>
    public sealed class ClientActionPlayer {
        public NetworkBehaviour ClientCharacter { get; private set; }

        List<NetworkAction> playingActions = new List<NetworkAction>();

        public ClientActionPlayer(NetworkBehaviour clientCharacter) {
            ClientCharacter = clientCharacter;
        }

        public void OnUpdate () {
            // reverse-walk so we can safely remove inside the loop.
            for (int i = playingActions.Count - 1; i >= 0; --i) {
                var action = playingActions[i];

                // only call OnUpdate() on actions that are past anticipation
                bool keepGoing = action.AnticipatedClient || action.OnUpdateClient(ClientCharacter);

                // non-positive value is a sentinel indicating the duration is indefinite
                bool expirable = action.Config.DurationSeconds > 0f; 
                bool timeExpired = expirable && action.TimeRunning >= action.Config.DurationSeconds;
                bool timedOut = action.AnticipatedClient; //&& action.TimeRunning >= k_AnticipationTimeoutSeconds;
                
                if (!keepGoing || timeExpired || timedOut) {
                    // an anticipated action that timed out shouldn't get its End called. It is canceled instead.
                    if (timedOut) {
                        action.CancelClient(ClientCharacter); 
                    } 
                    else {
                        action.EndClient(ClientCharacter); 
                    }

                    playingActions.RemoveAt(i);
                    ActionFactory.ReturnAction(action);
                }
            }
        }

        public void OnStoppedChargingUp(float finalChargeUpPercentage) {
            foreach (var actionFX in playingActions) {
                actionFX.OnStoppedChargingUpClient(ClientCharacter, finalChargeUpPercentage);
            }
        }

        //helper wrapper for a FindIndex call on m_PlayingActions.
        private int FindAction(ActionID actionID, bool anticipatedOnly) {
            return playingActions.FindIndex(a => a.ActionID == actionID && (!anticipatedOnly || a.AnticipatedClient));
        }

        public void PlayAction(ref ActionPacket data) {
            var anticipatedActionIndex = FindAction(data.ActionID, true);
            var actionFX = anticipatedActionIndex >= 0 ? playingActions[anticipatedActionIndex] : ActionFactory.CreateActionFromData(ref data);
            
            if (actionFX.OnStartClient(ClientCharacter)) {
                if (anticipatedActionIndex < 0) {
                    playingActions.Add(actionFX);
                }
                //otherwise just let the action sit in it's existing slot
            }
            else if (anticipatedActionIndex >= 0) {
                var removedAction = playingActions[anticipatedActionIndex];
                playingActions.RemoveAt(anticipatedActionIndex);
                ActionFactory.ReturnAction(removedAction);
            }
        }

        public void AnticipateAction(ref ActionPacket data) {
            var actionFX = ActionFactory.CreateActionFromData(ref data);

            if (actionFX.Config.IsAnticipatable && NetworkAction.ShouldClientAnticipate(ClientCharacter, ref data)) {
                actionFX.AnticipateActionClient(ClientCharacter);
                playingActions.Add(actionFX);
            }
        }
    }
}
