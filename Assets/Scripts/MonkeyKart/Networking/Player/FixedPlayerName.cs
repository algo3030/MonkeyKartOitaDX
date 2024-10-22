using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace MonkeyKart.Networking
{
    public struct FixedPlayerName : INetworkSerializable
    {
        FixedString32Bytes m_Name;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref m_Name);
        }

        public override string ToString()
        {
            return m_Name.Value.ToString();
        }

        public static implicit operator string(FixedPlayerName s) => s.ToString();
        public static implicit operator FixedPlayerName(string s) => new FixedPlayerName() { m_Name = new FixedString32Bytes(s) };
    }
}