using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


namespace k323.Commons.NetworkActionSystem {
    public class ServerActionPlayer {
        NetworkBehaviour serverCharacter;

        // list of enqueued action, executed one at the time
        List<Action> actionQueue;

        // list of actions runnig in background, executed in parallel
        List<Action> nonBlockingActions;

        public ServerActionPlayer(NetworkBehaviour serverCharacter) {
            this.serverCharacter = serverCharacter;

            actionQueue = new List<Action>();
            nonBlockingActions = new List<Action>();
        }

        /// <summary>
        /// Perform a sequence of actions.
        /// </summary>
        public void PlayAction(ref ActionPacket data) {
            var newAction = ActionFactory.CreateActionFromData(ref data);
            actionQueue.Add(newAction);
            if (actionQueue.Count == 1) { StartAction(); }
        }

        /// <summary>
        /// Starts the action at the head of the queue, if any.
        /// </summary>
        private void StartAction() {
            if (actionQueue.Count == 0) { return; }

            actionQueue[0].TimeStarted = Time.time;
            bool play = actionQueue[0].OnStart(serverCharacter);
            if (!play) {
                //actions that exited out in the "Start" method will not have their End method called, by design.
                AdvanceQueue(false); // note: this will call StartAction() recursively if there's more stuff in the queue ...
                return;              // ... so it's important not to try to do anything more here
            }

            if (actionQueue[0].Config.ExecTimeSeconds == 0 && actionQueue[0].Config.BlockingMode == BlockingModeType.OnlyDuringExecTime) {
                //this is a non-blocking action with no exec time. It should never be hanging out at the front of the queue (not even for a frame),
                //because it could get cleared if a new Action came in in that interval.
                nonBlockingActions.Add(actionQueue[0]);
                AdvanceQueue(false); // note: this will call StartAction() recursively if there's more stuff in the queue ...
                return;              // ... so it's important not to try to do anything more here
            }
        }

        public void OnUpdate() {
            // if there's a blocking action, update it
            if (actionQueue.Count > 0) {
                if (!UpdateAction(actionQueue[0])) {
                    AdvanceQueue(true);
                }
            }

            // if there's non-blocking actions, update them! We do this in reverse-order so we can easily remove expired actions.
            for (int i = nonBlockingActions.Count - 1; i >= 0; --i) {
                Action runningAction = nonBlockingActions[i];
                if (!UpdateAction(runningAction)) {
                    // it's dead!
                    runningAction.End(serverCharacter);
                    nonBlockingActions.RemoveAt(i);
                    TryReturnAction(runningAction);
                }
            }
        }

        /// <summary>
        /// Optionally end the currently playing action, and advance to the next Action that wants to play.
        /// </summary>
        /// <param name="endRemoved">if true we call End on the removed element.</param>
        private void AdvanceQueue(bool endRemoved) {
            if (actionQueue.Count > 0) {
                if (endRemoved) {
                    actionQueue[0].End(serverCharacter);
                }
                var action = actionQueue[0];
                actionQueue.RemoveAt(0);
                TryReturnAction(action);
            }

            StartAction();
        }

        /// <summary>
        /// Calls a given Action's Update() and decides if the action is still alive.
        /// </summary>
        /// <returns>true if the action is still active, false if it's dead</returns>
        private bool UpdateAction(Action action) {
            bool keepGoing = action.OnUpdate(serverCharacter);
            // non-positive value is a sentinel indicating the duration is indefinite.
            bool expirable = action.Config.DurationSeconds > 0f;
            bool timeExpired = expirable && action.TimeRunning >= action.Config.DurationSeconds;
            return keepGoing && !timeExpired;
        }

        // Try release action object to pool
        private void TryReturnAction(Action action) {
            if (actionQueue.Contains(action)) {
                return;
            }

            if (nonBlockingActions.Contains(action)) {
                return;
            }

            ActionFactory.ReturnAction(action);
        }
    }
}
