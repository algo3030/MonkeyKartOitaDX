using MonkeyKart.Networking;
using System;
using Unity.Netcode;

namespace MonkeyKart.LobbyScene
{
    public struct LobbyPlayerState : INetworkSerializable, IEquatable<LobbyPlayerState>
    {
        public ulong ClientId;
        FixedPlayerName playerName;
        public FixedPlayerName PlayerName => playerName;

        public LobbyPlayerState(ulong clientId, string name)
        {
            ClientId = clientId;
            playerName = name;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref playerName);
        }
        public bool Equals(LobbyPlayerState other)
        {
            return ClientId == other.ClientId && playerName.Equals(other.playerName);
        }
    }
}