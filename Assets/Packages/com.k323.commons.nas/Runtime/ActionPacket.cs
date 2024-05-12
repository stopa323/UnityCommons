using Unity.Netcode;
using UnityEngine;


namespace k323.Commons.NetworkActionSystem {
    /// <summary>
    /// Comprehensive struct that contains information needed to play back any action on the server. 
    /// This is what gets sent client->server when the Action gets played, and also what gets sent 
    /// server->client to broadcast the action event. 
    /// </summary>
    public struct ActionPacket : INetworkSerializable {
        public ActionID ActionID;
        public Vector3 Position;
        public ulong[] TargetIds;          //NetworkObjectIds of targets, or null if untargeted.

        [System.Flags]
        private enum PackFlags {
            None = 0,
            HasPosition = 1,
            HasTargetIds = 1 << 1,
        }

        public static ActionPacket Create(Action action) => 
            new() { ActionID = action.ActionID };

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            PackFlags flags = PackFlags.None;
            if (!serializer.IsReader) {
                flags = GetPackFlags();
            }

            serializer.SerializeValue(ref ActionID);
            serializer.SerializeValue(ref flags);
            
            if ((flags & PackFlags.HasPosition) != 0) {
                serializer.SerializeValue(ref Position);
            }

            if ((flags & PackFlags.HasTargetIds) != 0) {
                serializer.SerializeValue(ref TargetIds);
            }
        }

        private PackFlags GetPackFlags() {
            PackFlags flags = PackFlags.None;
            if (Position != Vector3.zero) { flags |= PackFlags.HasPosition; }
            if (TargetIds != null) { flags |= PackFlags.HasTargetIds; }

            return flags;
        }
    }
}
