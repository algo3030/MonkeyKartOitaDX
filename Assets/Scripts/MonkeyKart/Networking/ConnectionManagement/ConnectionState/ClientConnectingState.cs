
using MonkeyKart.Common;
using MonkeyKart.UnityService.Lobbies;
using System.Collections.Generic;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace MonkeyKart.Networking.ConnectionManagement
{
    class ClientConnectingState : ConnectionState
    {
        const string TAG = "ClientConnectingState";

        string playerName;
        string lobbyCode;

        ClientSideLobby clientSideLobby;

        public ClientConnectingState(ConnectionManager connectionManager, string playerName, string lobbyCode) : base(connectionManager)
        {
            this.playerName = playerName;
            this.lobbyCode = lobbyCode;
        }

        public override void Enter()
        {
            JoinLobby();
        }

        public override void OnClientConnected(ulong _)
        {
            owner.ChangeState(new ClientConnectedState(owner, clientSideLobby));
        }

        public override void OnClientDisconnected(ulong _) 
        {
            // 必ず自身のidである
            var disconnectReason = owner.networkManager.DisconnectReason;
            DisconnectReason reason =
                string.IsNullOrEmpty(disconnectReason) ?
                    DisconnectReason.Generic : JsonUtility.FromJson<DisconnectReason>(disconnectReason);
            StartClientFailed(reason);
        }

        // TODO: ロジックを逃がす
        async void JoinLobby()
        {
            if (!AuthenticationService.Instance.IsAuthorized)
            {
                StartClientFailed(DisconnectReason.Generic);
                return;
            }

            var res = await LobbyAPI.TryJoinLobby(lobbyCode: lobbyCode, playerDisplayName: playerName);
            res
                .OnFailure(reason =>
                {
                    Log.w(TAG, $"Joining lobby failed. reason: {reason}");
                    StartClientFailed(DisconnectReason.LobbyNotFound);
                })
                .OnSuccess(async lobby =>
                {
                    clientSideLobby = new ClientSideLobby(lobby, owner);

                    // ペイロードのセットアップ
                    var playerId = AuthenticationService.Instance.PlayerId;
                    var payload = JsonUtility.ToJson(new ConnectionPayload()
                    {
                        playerId = playerId,
                        playerName = playerName,
                        isDebug = Debug.isDebugBuild,
                    });
                    var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
                    owner.networkManager.NetworkConfig.ConnectionData = payloadBytes;

                    var relayJoinCode = lobby.Data.ContainsKey("RelayJoinCode") ? lobby.Data["RelayJoinCode"].Value : null;
                    if(relayJoinCode == null)
                    {
                        Log.w(TAG, $"RelayJoinCode not found.");
                        StartClientFailed(DisconnectReason.Generic);
                        return;
                    }

                    var joinedAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
                    Log.d(TAG, $"client: {joinedAllocation.ConnectionData[0]} {joinedAllocation.ConnectionData[1]}, " +
                        $"host: {joinedAllocation.HostConnectionData[0]} {joinedAllocation.HostConnectionData[1]}, " +
                        $"client: {joinedAllocation.AllocationId}");


                    (await clientSideLobby.SetPlayerData(
                            allocationId: joinedAllocation.AllocationId.ToString(),
                            connectionInfo: relayJoinCode,
                            playerDisplayName: playerName))
                            .OnFailure(_ =>
                            {
                                Log.w(TAG, "Setting player data failed.");
                                StartClientFailed(DisconnectReason.Generic);
                            });

                    var utp = (UnityTransport)owner.networkManager.NetworkConfig.NetworkTransport;
                    utp.SetRelayServerData(new RelayServerData(joinedAllocation, ConnectionManager.CONNECTION_TYPE));

                    if (!owner.networkManager.StartClient())
                    {
                        StartClientFailed(DisconnectReason.Generic);
                        return;
                    }
                });
        }

        void StartClientFailed(DisconnectReason reason)
        {
            Log.w(TAG, "Connecting as client failed.");
            clientSideLobby?.Dispose();
            owner.ChangeState(new OfflineState(owner, reason));
        }

        public override void Exit(){}
    }
}