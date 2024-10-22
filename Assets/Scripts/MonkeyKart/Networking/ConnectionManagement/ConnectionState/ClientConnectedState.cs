
using MonkeyKart.Common;
using UnityEngine;

namespace MonkeyKart.Networking.ConnectionManagement
{
    class ClientConnectedState : OnlineState
    {
        const string TAG = "ClientConnectedState";
        ClientSideLobby clientSideLobby;

        public ClientConnectedState(ConnectionManager connectionManager, ClientSideLobby clientSideLobby) : base(connectionManager)
        {
            this.clientSideLobby = clientSideLobby;
        }

        public override void Enter()
        {
            
        }

        public override void Exit()
        {
            clientSideLobby?.Dispose();
        }

        public override void OnUserRequestedShutDown()
        {
            owner.ChangeState(new OfflineState(owner, DisconnectReason.ShutdownByMe));
        }

        public override void OnClientDisconnected(ulong clientId)
        {
            var disconnectReason = owner.networkManager.DisconnectReason;
            DisconnectReason reason =
                string.IsNullOrEmpty(disconnectReason) ?
                    DisconnectReason.Generic : JsonUtility.FromJson<DisconnectReason>(disconnectReason);
            owner.ChangeState(new OfflineState(owner, reason));
        }
    }
}