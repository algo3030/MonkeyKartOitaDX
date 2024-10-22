using MonkeyKart.Common;
using MonkeyKart.Common.UI;
using MonkeyKart.Networking.ConnectionManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using UniRx;
using TMPro;
using Cysharp.Threading.Tasks;
using MonkeyKart.Common.UI.Button;
using Unity.Netcode;
using Unity.Collections;

namespace MonkeyKart.LobbyScene
{
    public class ClientLobbyUI : MonoBehaviour
    {
        const string TAG = "ClientLobbyUIMediator";

        [Inject] ConnectionManager connectionManager;
        [Inject] ServerLobbyManager serverLobbyManager;
        [SerializeField] SimpleButton exitLobbyBtn;
        [SerializeField] TextMeshProUGUI lobbyIdTxt;

        [SerializeField] List<PlayerBanner> clientPlayerDisplay;

        void Start()
        {
            exitLobbyBtn.OnClick.Subscribe(async _ =>
            {
                await UniTask.Delay(500);
                connectionManager.RequestShutdown();
            });

            OnLobbyCodeChanged(string.Empty, serverLobbyManager.LobbyCode.Value);
            ApplyLobbyPlayersState();
            serverLobbyManager.LobbyCode.OnValueChanged += OnLobbyCodeChanged;
            serverLobbyManager.LobbyPlayers.OnListChanged += OnLobbyPlayersChanged;
        }

        private void OnDestroy()
        {
            serverLobbyManager.LobbyCode.OnValueChanged -= OnLobbyCodeChanged;
            serverLobbyManager.LobbyPlayers.OnListChanged -= OnLobbyPlayersChanged;
        }

        void OnLobbyCodeChanged(FixedString32Bytes _, FixedString32Bytes code)
        {
            lobbyIdTxt.text = $"ロビーID: {code}";
        }

        void OnLobbyPlayersChanged(NetworkListEvent<LobbyPlayerState> changeEvent)
        {
            Log.d(TAG, "Lobby players changed.");
            ApplyLobbyPlayersState();
        }

        void ApplyLobbyPlayersState()
        {
            for (int i = 0; i < clientPlayerDisplay.Count; i++)
            {
                if (i < serverLobbyManager.LobbyPlayers.Count)
                {
                    var playerData = serverLobbyManager.LobbyPlayers[i];
                    clientPlayerDisplay[i].Enable(playerData.ClientId == NetworkManager.Singleton.LocalClientId, playerData);
                    continue;
                }
                clientPlayerDisplay[i].Disable();
            }
        }
    }
}