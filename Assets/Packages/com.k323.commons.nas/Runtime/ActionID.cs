using System;
using Unity.Netcode;

namespace k323.Commons.NetworkActionSystem {
    /// <summary>
    /// Wraps an integer. Used to refer a specific action in runtime.
    /// </summary>
    public struct ActionID : INetworkSerializeByMemcpy, IEquatable<ActionID> {
        public int ID;

        public bool Equals(ActionID other) => ID == other.ID;

        public override bool Equals(object obj) => obj is ActionID other && Equals(other);

        public override int GetHashCode() => ID;

        public static bool operator ==(ActionID x, ActionID y) => x.Equals(y);

        public static bool operator !=(ActionID x, ActionID y) => !(x == y);

        public override string ToString() => $"ActionID({ID})";
    }
}