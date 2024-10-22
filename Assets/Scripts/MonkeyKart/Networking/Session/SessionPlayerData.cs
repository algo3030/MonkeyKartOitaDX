using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace MonkeyKart.Networking.Session
{
    public struct SessionPlayerData
    {
        public ulong ClientId;
        public string PlayerName;
        public bool IsConnected;

        public SessionPlayerData(ulong clientId, string name, bool isConnected)
        {
            ClientId = clientId;
            PlayerName = name;
            IsConnected = isConnected;
        }
    }

}