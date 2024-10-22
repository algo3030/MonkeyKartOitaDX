using Cysharp.Threading.Tasks;
using Unity.Services.Lobbies.Models;
using MonkeyKart.Common;
using System;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using UniRx;
using Unity.Services.Authentication;
using System.Threading.Tasks;

namespace MonkeyKart.UnityService.Lobbies
{
    /// <summary>
    /// ÉçÉrÅ[Ç…ä÷Ç∑ÇÈèàóùÇÇ‹Ç∆ÇﬂÇΩAPIÅB
    /// </summary>
    public static class LobbyAPI
    {
        const int MAX_PLAYERS = 6;
        const string DEFAULT_LOBBY_NAME = "NO-NAME";

        static RateLimitCooldown rateLimitQuery = new(1f);
        static RateLimitCooldown rateLimitJoin = new(3f);
        static RateLimitCooldown rateLimitQuickJoin = new(10f);
        static RateLimitCooldown rateLimitHost = new(3f);

        public static async UniTask<Result<Lobby, string>> UpdateLobby(string lobbyId, Dictionary<string, DataObject> data, bool shouldLock)
        {
            if (!rateLimitQuery.CanCall) return "Update lobby hit the rate limit.";

            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions { Data = data, IsLocked = shouldLock };

            try
            {
                return await LobbyService.Instance.UpdateLobbyAsync(lobbyId, updateOptions);
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    rateLimitQuery.PutOnCooldown();
                    return "Create lobby hit the rate limit.";
                }
                return e.ToString();
            }
        }

        public static async Task<Result<Lobby, string>> UpdatePlayer(string lobbyId, string playerId, Dictionary<string, PlayerDataObject> data, string allocationId, string connectionInfo)
        {
            if (!rateLimitQuery.CanCall) return "Create lobby hit the rate limit.";

            var updateOptions = new UpdatePlayerOptions
            {
                Data = data,
                AllocationId = allocationId,
                ConnectionInfo = connectionInfo
            };
            try
            {
                return await LobbyService.Instance.UpdatePlayerAsync(lobbyId, playerId, updateOptions);
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    rateLimitQuery.PutOnCooldown();
                }
                return e.ToString();
            }
        }

        public static async UniTask<Result<Unit, string>> RemovePlayerFromLobby(string requesterUasId, string lobbyId)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(lobbyId, requesterUasId);
                return Unit.Default;
            }
            catch(LobbyServiceException e)
            {
                if (e is { Reason: LobbyExceptionReason.PlayerNotFound }) return Unit.Default;
                return e.ToString();
            }
        }

        public static async void SendHeartbeatPing(string lobbyId)
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
        }

        public static async UniTask<ILobbyEvents> SubscribeToLobby(string lobbyId, LobbyEventCallbacks eventCallbacks)
        {
            return await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyId, eventCallbacks);
        }

        public static async UniTask<Result<Lobby, string>> CreateLobby(string requesterUasId, string lobbyName)
        {
            if (requesterUasId == null) throw new ArgumentException();
            if (!rateLimitHost.CanCall) return "Create lobby hit the rate limit.";

            CreateLobbyOptions createOptions = new CreateLobbyOptions
            {
                IsPrivate = true,
                IsLocked = false, // locking the lobby at creation to prevent other players from joining before it is ready
                Player = new Player(id: requesterUasId),
            };

            try
            {
                var lobby = await LobbyService.Instance.CreateLobbyAsync(
                    lobbyName: string.IsNullOrWhiteSpace(lobbyName) ? DEFAULT_LOBBY_NAME : lobbyName,
                    maxPlayers: MAX_PLAYERS,
                    options: createOptions
                    );
                return lobby;
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    rateLimitHost.PutOnCooldown();
                }
                else
                {
                    return e.ToString();
                }
            }

            return "Create lobby failed.";
        }

        public static async UniTask<Result<Lobby, string>> TryJoinLobby(string lobbyCode, string playerDisplayName)
        {
            if (string.IsNullOrWhiteSpace(lobbyCode) || string.IsNullOrWhiteSpace(playerDisplayName)) return "Arguments is invalid.";
            if (!rateLimitJoin.CanCall) return "Join Lobby hit the rate limit.";

            try
            {
                var joinOptions = new JoinLobbyByCodeOptions
                {
                    Player = new Player(
                        id: AuthenticationService.Instance.PlayerId,
                        data: new() { { "DisplayName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerDisplayName) } }
                        )
                };
                return await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinOptions);
            }
            catch (LobbyServiceException e)
            {
                if (e.Reason == LobbyExceptionReason.RateLimited)
                {
                    rateLimitJoin.PutOnCooldown();
                }
                else
                {
                    return e.Reason.ToString();
                }
            }

            return "Join Lobby Failed.";
        }

        public static async UniTask<Result<Unit, string>> DeleteLobby(string lobbyId)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
                return Unit.Default;
            }
            catch (LobbyServiceException e)
            {
                return e.ToString();
            }
        }
    }
}