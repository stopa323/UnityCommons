using System;
using UnityEngine;

namespace k323.Commons.NetworkActionSystem {
    [Serializable]
    public class ActionConfig {
        [Tooltip("Duration in seconds that this Action takes to play")]
        public float DurationSeconds;

        [Tooltip("Time when the Action should do its \"main thing\" (e.g. when a melee attack should apply damage")]
        public float ExecTimeSeconds;    

        [Tooltip("The radius of effect for this action. Default is 0 if not needed")]
        public float Radius;

        [Tooltip("Whether this action can be anticipated by client")]
        public bool IsAnticipatable = false;

        [Tooltip("Indicates how long this action blocks other actions from happening: during the execution stage, or for as long as it runs?")]
        public BlockingModeType BlockingMode;
        
    }
}
