using System;
using Unity.Netcode;
using UnityEngine;


namespace k323.Commons.NetworkActionSystem {
    /// <summary>
    /// The abstract parent class that all Actions derive from.
    /// </summary>
    public abstract class Action : ScriptableObject {
        /// <summary>
        /// Unique ID of action - set at runtime by ActionPrototypeStore class.
        /// If action is not itself a prototype - will contain the action id of the prototype reference. This field
        /// is used to identify actions in a way that can be sent over the network.
        /// </summary>
        [NonSerialized] public ActionID ActionID;

        /// <summary>
        /// Time when this Action was started (from Time.time) in seconds. Set by the action Player or action Visualization.
        /// </summary>
        public float TimeStarted { get; set; }

        /// <summary>
        /// How long the Action has been running (since its Start was called) in seconds, measured via Time.time.
        /// </summary>
        public float TimeRunning { get { return Time.time - TimeStarted; } }

        /// <summary>
        /// Data Description for this action.
        /// </summary>
        public ActionConfig Config;

        /// <summary>
        /// True if this actionFX began running immediately, prior to getting a confirmation from the server.
        /// </summary>
        public bool AnticipatedClient { get; protected set; }

        /// <summary>
        /// Data we were instantiated with. Value should be treated as readonly.
        /// </summary>
        public ref ActionPacket Data => ref packetData;
        protected ActionPacket packetData;

        /// <summary>
        /// Constructor. The "data" parameter should not be retained after passing in to this method, because we take ownership of its internal memory.
        /// Needs to be called by the ActionFactory.
        /// </summary>
        public void Initialize(ref ActionPacket packet) {
            packetData = packet;
            ActionID = packet.ActionID;
        }

        /// <summary>
        /// This function resets the action before returning it to the pool
        /// </summary>
        public virtual void Reset() {
            packetData = default;
            ActionID = default;
            TimeStarted = 0;
        }

        /// <summary>
        /// Should this ActionFX be created anticipatively on the owning client?
        /// </summary>
        /// <param name="clientCharacter">The ActionVisualization that would be playing this ActionFX.</param>
        /// <param name="data">The request being sent to the server</param>
        /// <returns>If true action Visualization should pre-emptively create the ActionFX on the owning client, before hearing back from the server.</returns>
        public static bool ShouldClientAnticipate(NetworkBehaviour clientCharacter, ref ActionPacket data) {
            // if (!clientCharacter.CanPerformActions) { return false; }

            return true;
        }

        #region Server Side

        /// <summary>
        /// Called when the Action starts actually playing (which may be after it is created, because of queueing).
        /// </summary>
        /// <returns>false if the action decided it doesn't want to run after all, true otherwise. </returns>
        public abstract bool OnStart(NetworkBehaviour serverCharacter);

        /// <summary>
        /// Called each frame while the action is running.
        /// </summary>
        /// <returns>true to keep running, false to stop. The Action will stop by default when its duration expires, if it has a duration set. </returns>
        public abstract bool OnUpdate(NetworkBehaviour serverCharacter);

        /// <summary>
        /// This will get called when the Action gets canceled. The Action should clean up any ongoing effects at this point.
        /// (e.g. an Action that involves moving should cancel the current active move).
        /// </summary>
        public virtual void Cancel(NetworkBehaviour serverCharacter) { }

        /// <summary>
        /// Called when the Action ends naturally. By default just calls Cancel()
        /// </summary>
        public virtual void End(NetworkBehaviour serverCharacter) {
            Cancel(serverCharacter);
        }
        #endregion

        #region Client Side

        /// <summary>
        /// Starts the ActionFX. Derived classes may return false if they wish to end immediately without their Update being called.
        /// </summary>
        /// <remarks>
        /// Derived class should be sure to call base.OnStart() in their implementation, but note that this resets "Anticipated" to false.
        /// </remarks>
        /// <returns>true to play, false to be immediately cleaned up.</returns>
        public virtual bool OnStartClient(NetworkBehaviour clientCharacter) {
            // once you start for real you are no longer an anticipated action.
            AnticipatedClient = false;
            TimeStarted = UnityEngine.Time.time;
            return true;
        }

        public virtual bool OnUpdateClient(NetworkBehaviour clientCharacter) {
            return ActionConclusion.Continue;
        }

        /// <summary>
        /// End is always called when the ActionFX finishes playing. This is a good place for derived classes to put
        /// wrap-up logic (perhaps playing the "puff of smoke" that rises when a persistent fire AOE goes away). Derived
        /// classes should aren't required to call base.End(); by default, the method just calls 'Cancel', to handle the
        /// common case where Cancel and End do the same thing.
        /// </summary>
        public virtual void EndClient(NetworkBehaviour clientCharacter) {
            CancelClient(clientCharacter);
        }

        /// <summary>
        /// Cancel is called when an ActionFX is interrupted prematurely. It is kept logically distinct from End to allow
        /// for the possibility that an Action might want to play something different if it is interrupted, rather than
        /// completing. For example, a "ChargeShot" action might want to emit a projectile object in its End method, but
        /// instead play a "Stagger" animation in its Cancel method.
        /// </summary>
        public virtual void CancelClient(NetworkBehaviour clientCharacter) { }

        /// <summary>
        /// Called when the action is being "anticipated" on the client. For example, if you are the owner of a weapon
        /// and you shoot, you get this call immediately on the client, before the server round-trip.
        /// Overriders should always call the base class in their implementation!
        /// </summary>
        public virtual void AnticipateActionClient(NetworkBehaviour clientCharacter) {
            AnticipatedClient = true;
            TimeStarted = UnityEngine.Time.time;
        }
        #endregion
    }
}
