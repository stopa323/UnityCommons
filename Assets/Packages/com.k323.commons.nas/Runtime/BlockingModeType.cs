using System;


namespace k323.Commons.NetworkActionSystem {
    [Serializable]
    public enum BlockingModeType {
        // Action blocks queue for its entire durarion
        EntireDuration,

        // Fire and forget
        OnlyDuringExecTime,
    }
}
