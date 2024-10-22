using Cysharp.Threading.Tasks;
using MonkeyKart.Common;
using MonkeyKart.Networking.ConnectionManagement;
using MonkeyKart.UnityService;
using MonkeyKart.UnityService.Lobbies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Threading;
using UniRx;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace MonkeyKart.Networking
{
    public class ClientSideLobby : IDisposable
    {
        const string TAG = "ClientSideLobby";
        ConnectionManager connectionManager;

        ILobbyEvents lobbyEvents;

        public Lobby CurrentLobby { get; private set; }

        public ClientSideLobby(Lobby lobby, ConnectionManager connectionManager)
        {
            CurrentLobby = lobby;
            this.connectionManager = connectionManager;
            SubscribeToLobby();
        }

        public void Dispose()
        {
            Log.d(TAG, "Dispose");
            LeaveLobby().Forget();
            UnsubscribeToLobby();
        }

        private async UniTask LeaveLobby()
        {
            Log.d(TAG, "Attempting to leave lobby.");
            var uasId = AuthenticationService.Instance.PlayerId;

            var res = await LobbyAPI.RemovePlayerFromLobby(uasId, CurrentLobby.Id);
            res
                .OnSuccess(_ =>
                {
                    Log.d(TAG, "Leaved lobby successflly.");
                })
                .OnFailure(_ =>
                {
                    Log.d(TAG, "Leave lobby failed.");
                });
        }

        public async UniTask<Result<Unit, string>> SetPlayerData(string allocationId, string connectionInfo, string playerDisplayName)
        {
            return (await LobbyAPI.UpdatePlayer(
                lobbyId: CurrentLobby.Id,
                playerId: AuthenticationService.Instance.PlayerId,
                allocationId: allocationId,
                connectionInfo: connectionInfo,
                data: new Dictionary<string, PlayerDataObject>()
                {
                    {"DisplayName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerDisplayName) }
                }))
                .Map(lobby => 
                {
                    CurrentLobby = lobby;
                    return Unit.Default;
                });
        }

        async void SubscribeToLobby()
        {
            var lobbyEventCallbacks = new LobbyEventCallbacks();
            lobbyEventCallbacks.LobbyChanged += OnLobbyChanged;
            lobbyEventCallbacks.KickedFromLobby += OnKickedFromLobby;
            lobbyEvents = await LobbyAPI.SubscribeToLobby(CurrentLobby.Id, lobbyEventCallbacks);
        }

        async void UnsubscribeToLobby()
        {
            if(lobbyEvents != null) 
            {
#if UNITY_EDITOR
                try
                {
                    await lobbyEvents.UnsubscribeAsync();
                }
                catch(WebSocketException e)
                {
                    Log.w(TAG, e.Message);
                }
#else
                await lobbyEvents.UnsubscribeAsync();
#endif
            }

        }

        void OnLobbyDisconnected(DisconnectReason reason)
        {
            Dispose();
            connectionManager.ChangeState(new OfflineState(connectionManager, reason));
        }

        void OnKickedFromLobby()
        {
            Log.d(TAG, "Kicked from Lobby");
            OnLobbyDisconnected(DisconnectReason.Kicked);
        }

        void OnLobbyChanged(ILobbyChanges changes)
        {
            if (changes.LobbyDeleted)
            {
                Log.d(TAG, "Lobby deleted.");
                OnLobbyDisconnected(DisconnectReason.HostEndedSession);
            }
            else
            {
                Log.d(TAG, $"Lobby updated. PlayerJoined: {changes.PlayerJoined}, Data: {changes.Data}");
                changes.ApplyToLobby(CurrentLobby);

                foreach(var player in CurrentLobby.Players)
                {
                    // ホストの存在確認
                    if (player.Id == CurrentLobby.HostId) return;
                    OnLobbyDisconnected(DisconnectReason.HostEndedSession);
                }
            }
        }
    }
}