using TMPro;
using UnityEngine;
using UniRx;
using MonkeyKart.Common;
using MonkeyKart.Common.UI;
using MonkeyKart.Common.UI.Button;
using VContainer;
using MonkeyKart.Networking.ConnectionManagement;

namespace MonkeyKart.Home.UI
{
    /// <summary>
    /// ���r�[�����_�C�A���O�B
    /// </summary>
    public class SearchLobbyDialog : MonoBehaviour
    {
        const string TAG = "SearchLobbyDialog";

        [Inject] DialogSpawner owner;
        [Inject] ProfileManager profileManager;
        [Inject] ConnectionManager connectionManager;
        [Inject] LoadingCanvas loadingCanvas;

        [SerializeField] SimpleButton cancelBtn;
        [SerializeField] SimpleButton joinBtn;
        [SerializeField] TextMeshProUGUI errTxt;
        [SerializeField] TMP_InputField lobbyIdField;

        void Start()
        {
            errTxt.text = string.Empty;

            cancelBtn.OnClick.Subscribe(_ =>
            {
                owner.CloseDialog(gameObject);
            }).AddTo(this);

            joinBtn.OnClick.Subscribe(_ =>
            {
                loadingCanvas.Show();
                connectionManager.JoinLobby(playerName: profileManager.PlayerName.Value, lobbyCode: lobbyIdField.text);
            }).AddTo(this);

            connectionManager.CurrentState.Subscribe(state =>
            {
                if (state is OfflineState) loadingCanvas.Hide();
            }).AddTo(this);
        }
    }
}