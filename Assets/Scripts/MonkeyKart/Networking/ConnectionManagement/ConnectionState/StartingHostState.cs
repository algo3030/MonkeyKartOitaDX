using MonkeyKart.UnityService.Lobbies;
using Unity.Services.Authentication;
using MonkeyKart.Common;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using MonkeyKart.Home;
using MonkeyKart.Networking.Session;
using System.Collections.Generic;
using System;
using VContainer;
using Cysharp.Threading.Tasks;
using UniRx;

namespace MonkeyKart.Networking.ConnectionManagement
{
    class StartingHostState : ConnectionState
    {
        const string TAG = "StartingHostState";

        string playerName;
        string lobbyName;

        ServerSideLobby serverSideLobby;
        SessionManager sessionManager;

        public StartingHostState(ConnectionManager manager, string playerName, string lobbyName) : base(manager)
        {
            this.playerName = playerName;
            this.lobbyName = lobbyName;
        }
        public override bool CanTransitionTo(ConnectionState next)
        {
            return next is HostingState || next is OfflineState;
        }

        public override void Enter()
        {
            CreateLobbyAndStartHost();
        }

        // TODO: ���W�b�N�𓦂���
        private async void CreateLobbyAndStartHost()
        {
            var playerId = AuthenticationService.Instance.PlayerId;
            var lobbyRes = await LobbyAPI.CreateLobby(playerId, lobbyName);
            lobbyRes
                .OnFailure(
                    _ =>
                    {
                        Log.d(TAG, "Failed to create lobby.");
                        StartHostFailed(DisconnectReason.LobbyCreationFailed);
                    }
                ).OnSuccess(
                    async lobby =>
                    {
                        serverSideLobby = new ServerSideLobby(lobby, owner);

                        Log.d(TAG, "created lobby.");
                        Log.d(TAG, $"LobbyID: {serverSideLobby.CurrentLobby.Id}");
                        Log.d(TAG, $"LobbyCode: {serverSideLobby.CurrentLobby.LobbyCode}");
                        Log.d(TAG, $"LobbyName: {serverSideLobby.CurrentLobby.Name}");

                        // �y�C���[�h�̃Z�b�g�A�b�v
                        var payload = JsonUtility.ToJson(new ConnectionPayload()
                        {
                            playerId = playerId,
                            playerName = playerName, // �v���C���[�����擾
                            isDebug = Debug.isDebugBuild,
                        });
                        var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
                        owner.networkManager.NetworkConfig.ConnectionData = payloadBytes;

                        // Relay�T�[�o�[�̍쐬
                        Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(ConnectionManager.MAX_PLAYERS);
                        var relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);
                        Log.d(TAG, $"server: connection data: {hostAllocation.ConnectionData[0]} {hostAllocation.ConnectionData[1]}, " +
                    $"allocation ID:{hostAllocation.AllocationId}, region:{hostAllocation.Region}");


                        // Relay��Lobby��ڑ�
                        (await serverSideLobby.SetRelayJoinCodeAndUnlock(relayJoinCode))
                        .OnFailure(_ =>
                        {
                            Log.d(TAG, "Setting relay join code failed.");
                            StartHostFailed(DisconnectReason.LobbyCreationFailed);
                        });
                        
                        (await serverSideLobby.SetPlayerData(
                            allocationId: hostAllocation.AllocationIdBytes.ToString(),
                            connectionInfo: relayJoinCode,
                            playerDisplayName: playerName
                            ))
                            .OnFailure(_ =>
                            {
                                Log.d(TAG, "Setting player data failed.");
                                StartHostFailed(DisconnectReason.LobbyCreationFailed);
                            });



                        // Relay�̐ڑ���񂩂�UTP���Z�b�g�A�b�v
                        var utp = (UnityTransport)owner.networkManager.NetworkConfig.NetworkTransport;
                        utp.SetRelayServerData(new RelayServerData(hostAllocation, ConnectionManager.CONNECTION_TYPE));

                        // SessionManager���J�n
                        sessionManager = new SessionManager(lobbyCode: lobby.LobbyCode);

                        // �z�X�g�J�n
                        if (!owner.networkManager.StartHost())
                        {
                            StartHostFailed(DisconnectReason.Generic);
                            return;
                        }
                    }
                );
        }

        public override void OnServerStarted()
        {
            Log.d(TAG, "Server started.");
            owner.ChangeState(new HostingState(owner, serverSideLobby, sessionManager));
        }

        public override void OnServerStopped()
        {
            StartHostFailed(DisconnectReason.Generic);
        }

        void StartHostFailed(DisconnectReason reason)
        {
            Log.e(TAG, "StartHost failed.");
            serverSideLobby?.Dispose();
            owner.ChangeState(new OfflineState(owner, reason));
        }

        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            Log.d(TAG, "Approval check started.");

            var connectionData = request.Payload;
            var clientId = request.ClientNetworkId;

            if (clientId == owner.networkManager.LocalClientId)
            {
                var payload = System.Text.Encoding.UTF8.GetString(connectionData);
                var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

                // SessionManagerを初期化
                sessionManager.SetConnectingPlayerData(
                    clientId,
                    connectionPayload.playerId,
                    new SessionPlayerData(clientId: clientId, name: connectionPayload.playerName, isConnected: false)
                    );

                // 承認
                response.Approved = true;
                response.CreatePlayerObject = false;

                Log.d(TAG, $"New player approved! ClientId:{clientId}, PlayerName:{connectionPayload.playerName}");
            }
        }

        public override void Exit() {}
    }
}