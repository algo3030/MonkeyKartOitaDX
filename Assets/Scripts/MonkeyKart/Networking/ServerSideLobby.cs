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
    public class ServerSideLobby : IDisposable
    {
        const string TAG = "ServerSideLobby";

        ConnectionManager connectionManager;

        ILobbyEvents lobbyEvents;
        LobbyEventConnectionState lobbyEventConnectionState = LobbyEventConnectionState.Unknown;

        const float HEARTBEAT_PERIOD = 8f;

        public Lobby CurrentLobby { get; private set; }

        CancellationTokenSource heartBeatCTS = new();

        

        public ServerSideLobby(Lobby lobby, ConnectionManager connectionManager)
        {
            CurrentLobby = lobby;
            this.connectionManager = connectionManager;
            SubscribeToLobby();
            StartHeartBeat(heartBeatCTS.Token).Forget();
        }

        public async void Dispose()
        {
            heartBeatCTS.Cancel();
            UnsubscribeToLobby();
            await TryDeleteLobby();
        }

        void OnLobbyDisconnected(DisconnectReason reason)
        {
            Log.d(TAG, "Lobby disconnected.");
            Dispose();
            connectionManager.ChangeState(new OfflineState(connectionManager, reason));
        }

        async UniTask StartHeartBeat(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            while (true)
            {
                await UniTask.Yield(cancellationToken);
                // Log.d(TAG, "Heartbeat sending.");
                try
                {
                    LobbyAPI.SendHeartbeatPing(CurrentLobby.Id);
                }
                catch(LobbyServiceException)
                {
                    OnLobbyDisconnected(DisconnectReason.Generic);
                }
                await UniTask.Delay((int)(HEARTBEAT_PERIOD * 1000));
            }
        }

        private async UniTask TryDeleteLobby()
        {
            Log.d(TAG, "Attempting to delete lobby.");
            var res = await LobbyAPI.DeleteLobby(CurrentLobby.Id);
            res
                .OnSuccess(_ =>
                {
                    Log.d(TAG, "Lobby deleted successflly.");
                })
                .OnFailure(err =>
                {
                    Log.d(TAG, $"Delete lobby failed. reason:{err}");
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

        public async UniTask<Result<Unit,string>> SetRelayJoinCodeAndUnlock(string relayJoinCode)
        {
            var dataCurr = CurrentLobby.Data;
            if (dataCurr == null)
            {
                dataCurr = new Dictionary<string, DataObject>();
            }

            var localData = 
                new Dictionary<string, DataObject>()
                {
                    { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode) }
                };
            foreach (var dataNew in localData)
            {
                if (dataCurr.ContainsKey(dataNew.Key))
                {
                    dataCurr[dataNew.Key] = dataNew.Value;
                }
                else
                {
                    dataCurr.Add(dataNew.Key, dataNew.Value);
                }
            }

            return (await LobbyAPI.UpdateLobby(
                lobbyId: CurrentLobby.Id,
                data: dataCurr,
                shouldLock: false
                ))
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
            lobbyEventCallbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;
            lobbyEvents = await LobbyAPI.SubscribeToLobby(CurrentLobby.Id, lobbyEventCallbacks);
        }

        async void UnsubscribeToLobby()
        {
            if(lobbyEvents != null && lobbyEventConnectionState != LobbyEventConnectionState.Unsubscribed) 
            {
#if UNITY_EDITOR
                try
                {
                    await lobbyEvents.UnsubscribeAsync();
                }
                catch(Exception e)
                {
                    Log.w(TAG, e.Message);
                }
#else
                await lobbyEvents.UnsubscribeAsync();
#endif
            }

        }

        void OnLobbyChanged(ILobbyChanges changes)
        {
            if (changes.LobbyDeleted)
            {
                Log.d(TAG, "Lobby deleted.");
                OnLobbyDisconnected(DisconnectReason.Generic);
            }
            else
            {
                Log.d(TAG, $"Lobby updated. PlayerJoined: {changes.PlayerJoined.Value}");
                changes.ApplyToLobby(CurrentLobby);
            }
        }

        void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState lobbyEventConnectionState)
        {
            this.lobbyEventConnectionState = lobbyEventConnectionState;
            Log.d(TAG, $"LobbyEventConnectionState changed to {lobbyEventConnectionState}");
        }
    }
}