using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace MonkeyKart.LobbyScene
{
    public class ClientTextChatUI : MonoBehaviour
    {
        ServerLobbyManager serverLobbyManager;
        [SerializeField] TextMeshProUGUI chatTxt;
        [SerializeField] TMP_InputField inputField;

        private void Start()
        {
            serverLobbyManager = ServerLobbyManager.I;
            chatTxt.text = string.Empty;
            foreach (var chat in serverLobbyManager.ChatTexts)
            {
                chatTxt.text += chat;
            }
            serverLobbyManager.ChatTexts.OnListChanged += OnTextChatChanged;

            inputField.onSubmit.AddListener(OnTextSubmit);
        }

        private void OnDestroy()
        {
            serverLobbyManager.ChatTexts.OnListChanged -= OnTextChatChanged;
        }

        void OnTextSubmit(string text)
        {
            if (text == string.Empty) return;
            inputField.text = string.Empty;
            serverLobbyManager.SendChatMessageServerRpc(text);
        }

        void OnTextChatChanged(NetworkListEvent<FixedString128Bytes> changeEvent)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<FixedString128Bytes>.EventType.Add:
                    chatTxt.text += changeEvent.Value;
                    break;
            }
        }
    }
}