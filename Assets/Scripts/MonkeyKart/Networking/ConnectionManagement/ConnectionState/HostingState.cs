using MonkeyKart.Common;
using MonkeyKart.SceneManagement;
using Unity.Services.Lobbies.Models;
using MonkeyKart.Networking.Session;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;

namespace MonkeyKart.Networking.ConnectionManagement
{
    public class HostingState : OnlineState
    {
        const string TAG = "HostingState";

        ServerSideLobby serverSideLobby;
        public SessionManager SessionManager { get; private set; }

        const int MAX_CONNECT_PAYLOAD = 1024;

        public override bool CanTransitionTo(ConnectionState next)
        {
            return next is not HostingState && next is not StartingHostState;
        }

        public HostingState(ConnectionManager manager, ServerSideLobby serverSideLobby, SessionManager sessionManager): base(manager)
        {
            this.serverSideLobby = serverSideLobby;
            SessionManager = sessionManager;
        }

        public override void Enter()
        {
            owner.sceneLoader.LoadScene(MonkeyKartScenes.LOBBY, useNetworkSceneManager: true);
        }

        public override void Exit()
        {
            serverSideLobby?.Dispose();
        }

        public override void OnUserRequestedShutDown()
        {
            var reason = JsonUtility.ToJson(DisconnectReason.HostEndedSession);
            // NOTE: �Ȃ�foreach�ŃG���[��
            for (var i = owner.networkManager.ConnectedClientsIds.Count - 1; i >= 0; i--)
            {
                var id = owner.networkManager.ConnectedClientsIds[i];
                if (id != owner.networkManager.LocalClientId)
                {
                    owner.networkManager.DisconnectClient(id, reason);
                }
            }
            owner.ChangeState(new OfflineState(owner, DisconnectReason.ShutdownByMe));
        }

        public override void OnClientConnected(ulong clientId)
        {
            var res = SessionManager.GetPlayerData(clientId);
            // ��G���[
            res
                .OnFailure(_ =>
                {
                    Log.e(TAG, $"No player data associated with client {clientId}");
                    var reason = JsonUtility.ToJson(DisconnectReason.Generic);
                    owner.networkManager.DisconnectClient(clientId, reason);
                });
        }

        public override void OnClientDisconnected(ulong clientId)
        {
            if (clientId == owner.networkManager.LocalClientId) return; // �z�X�g�Ȃ牽�����Ȃ�

            Log.d(TAG, $"Player disconnected. ClientId:{clientId}");
            SessionManager.DisconnectClient(clientId);
        }

        public override void OnServerStopped()
        {
            owner.ChangeState(new OfflineState(owner, DisconnectReason.HostEndedSession));
        }

        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            Log.d(TAG, $"Approval check started.");

            var connectionData = request.Payload;
            var clientId = request.ClientNetworkId;
            // ������Ƃ���DOS�΍�
            if (connectionData.Length > MAX_CONNECT_PAYLOAD)
            {
                response.Approved = false;
                return;
            }

            var payload = System.Text.Encoding.UTF8.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

            // ���r�[�����b�N����Ă���
            if (SessionManager.IsLocked)
            {
                response.Approved = false;
                response.Reason = JsonUtility.ToJson(DisconnectReason.LobbyIsLocked);
                Log.w(TAG, $"clientId:{clientId} approval failed because lobby is locked.");
                return;
            }

            // �T�[�o�[������
            if (owner.networkManager.ConnectedClientsIds.Count >= ConnectionManager.MAX_PLAYERS)
            {
                response.Approved = false;
                response.Reason = JsonUtility.ToJson(DisconnectReason.ServerFull);
                Log.w(TAG, $"clientId:{clientId} approval failed because room is full.");
                return;
            }

            /*
            // �r���h�^�C�v�Ɍ݊������Ȃ�
            if (connectionPayload.isDebug != Debug.isDebugBuild)
            {
                response.Approved = false;
                response.Reason = JsonUtility.ToJson(DisconnectReason.BuildIncompatible);
                Log.w(TAG, $"clientId:{clientId} approval failed because of build incompatibility.");
                return;
            }
            */


            SessionManager.SetConnectingPlayerData(
                clientId: clientId,
                playerId: connectionPayload.playerId,
                playerData: new SessionPlayerData(clientId: clientId, name: connectionPayload.playerName, isConnected: false)
                );

            response.Approved = true;
            response.CreatePlayerObject = false;
            Log.d(TAG, $"New player approved! ClientId:{clientId}, PlayerName:{connectionPayload.playerName}");
        }
    }
}