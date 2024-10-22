using MonkeyKart.Common;
using MonkeyKart.Networking.ConnectionManagement;
using MonkeyKart.Networking.Session;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using UniRx;
using System.Collections.Generic;
using System;
using MonkeyKart.SceneManagement;
using Unity.VisualScripting;
using TMPro;

namespace MonkeyKart.LobbyScene
{
    public class ServerLobbyManager : NetworkBehaviour
    {
        const string TAG = "ServerLobbyManager";

        [Inject] ConnectionManager connectionManager;
        [Inject] SceneLoader sceneLoader;
        SessionManager sessionManager;

        public NetworkVariable<FixedString32Bytes> LobbyCode { get; private set; } = new();
        public NetworkList<LobbyPlayerState> LobbyPlayers { get; private set;}

        public NetworkList<FixedString128Bytes> ChatTexts { get; private set; }

        private void Awake()
        {
            LobbyPlayers = new();
            ChatTexts = new();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
                return;
            }
            sessionManager = connectionManager.GetSessionManager();
            LobbyCode.Value = sessionManager.LobbyCode;
            
            connectionManager.networkManager.SceneManager.OnSceneEvent += OnSceneEvent;
            connectionManager.networkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }

        public void StartGame()
        {
            sceneLoader.LoadScene(MonkeyKartScenes.GAME, true);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            connectionManager.networkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
            connectionManager.networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        // XXX: �����T�C�Y
        [ServerRpc(RequireOwnership = false)]
        public void SendChatMessageServerRpc(string s, ServerRpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            // �K���q�b�g����
            string senderName = string.Empty;
            foreach(var player in LobbyPlayers)
            {
                if (clientId == player.ClientId) senderName = player.PlayerName;
            }
            ChatTexts.Add($"<color=yellow>{senderName}:</color> {s}\n");
        }

        void AddNewPlayer(ulong clientId)
        {
            sessionManager.GetPlayerData(clientId)
                .OnFailure(_ => throw new Exception()) // ���蓾�Ȃ�
                .OnSuccess(playerData =>
                {
                    LobbyPlayers.Add(new LobbyPlayerState(playerData.ClientId, playerData.PlayerName));
                    ChatTexts.Add($"<color=green>{playerData.PlayerName}が入室しました</color>\n");
                });
        }

        void OnSceneEvent(SceneEvent sceneEvent)
        {
            if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;
            AddNewPlayer(sceneEvent.ClientId);
        }

        void OnClientDisconnected(ulong clientId)
        {
            for (int i = 0; i < LobbyPlayers.Count; i++)
            {
                if (LobbyPlayers[i].ClientId == clientId)
                {
                    ChatTexts.Add($"<color=green>{LobbyPlayers[i].PlayerName}が退出しました</color>\n");
                    LobbyPlayers.RemoveAt(i);
                    break;
                }
            }
        }
    }
}