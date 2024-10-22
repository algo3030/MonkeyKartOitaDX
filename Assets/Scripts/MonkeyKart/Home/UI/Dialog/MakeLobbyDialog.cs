using MonkeyKart.Common.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UniRx;
using MonkeyKart.Common;
using MonkeyKart.Common.UI.Button;
using MonkeyKart.Networking.ConnectionManagement;
using VContainer;

namespace MonkeyKart.Home.UI
{
    public class MakeLobbyDialog : MonoBehaviour
    {
        [Inject] DialogSpawner owner;
        [Inject] ConnectionManager connectionManager;
        [Inject] ProfileManager profileManager;
        [Inject] LoadingCanvas loadingCanvas;

        [SerializeField] SimpleButton cancelBtn;
        [SerializeField] SimpleButton makeBtn;
        [SerializeField] TMP_InputField inputField;

        void Start()
        {
            cancelBtn.OnClick.Subscribe(_ =>
            {
                owner.CloseDialog(gameObject);
            }).AddTo(this);

            makeBtn.OnClick.Subscribe(_ =>
            {
                loadingCanvas.Show();
                connectionManager.StartHost(
                    playerName: profileManager.PlayerName.Value,
                    lobbyName: inputField.text
                    );
            }).AddTo(this);

            connectionManager.CurrentState.Subscribe(state =>
            {
                if(state is OfflineState) loadingCanvas.Hide();
            }).AddTo(this);
        }
    }
}