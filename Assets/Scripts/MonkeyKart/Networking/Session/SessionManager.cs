using MonkeyKart.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine.Events;

namespace MonkeyKart.Networking.Session
{
    public class SessionManager
    {
        const string TAG = "SessionManager";

        public string LobbyCode { get; private set; }

        // NOTE: playerIdは一意にクライアントを識別するが、clientIdはただの通し番号
        Dictionary<string, SessionPlayerData> clientsPlayerData = new(); // playerId to SessionPlayerData
        public Dictionary<string, SessionPlayerData> ClientsPlayerData => clientsPlayerData;
        Dictionary<ulong, string> clientIdToPlayerId = new();


        public bool IsLocked { get; private set; }

        public SessionManager(string lobbyCode) 
        {
            LobbyCode = lobbyCode;
        } 

        public Result<SessionPlayerData, Unit> GetPlayerData(ulong clientId)
        {
            return GetPlayerId(clientId).AndThen<SessionPlayerData>(playerId =>
            {
                if (clientsPlayerData.TryGetValue(playerId, out var playerData))
                {
                    return playerData;
                }
                return Unit.Default;
            });
        }

        public Result<string, Unit> GetPlayerId(ulong clientId)
        {
            if (clientIdToPlayerId.TryGetValue(clientId, out var playerId)) return playerId;
            return Unit.Default;
        }

        public void LockLobby()
        {
            Log.d(TAG, "Locked.");
            IsLocked = true;
        }

        public void UnlockLobby()
        {
            Log.d(TAG, "Locked.");
            IsLocked = false;
        }

        public void SetConnectingPlayerData(ulong clientId, string playerId, SessionPlayerData playerData)
        {
            var isReconnecting = false;
            
            if (clientsPlayerData.ContainsKey(playerId))
            {
                if (clientsPlayerData[playerId].IsConnected)
                {
                    Log.e(TAG, $"Duplicate connection. I can't connect the client again with playerId:{playerId}.");
                    return;
                }

                isReconnecting = true;
            }

            if (isReconnecting)
            {
                playerData = clientsPlayerData[playerId];
                playerData.ClientId = clientId;
                playerData.IsConnected = true;
            }

            clientIdToPlayerId[clientId] = playerId;
            clientsPlayerData[playerId] = playerData;
        }

        public void DisconnectClient(ulong clientId)
        {
            if (clientIdToPlayerId.TryGetValue(clientId, out var playerId))
            {
                if (IsLocked)
                {
                    var updatedPlayerData = clientsPlayerData[playerId];
                    updatedPlayerData.IsConnected = false;
                    clientsPlayerData[playerId] = updatedPlayerData;
                }
                else
                {
                    clientIdToPlayerId.Remove(clientId);
                    clientsPlayerData.Remove(playerId);
                }
            }
        }
    }
}