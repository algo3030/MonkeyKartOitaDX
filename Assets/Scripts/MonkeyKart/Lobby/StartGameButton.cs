using MonkeyKart.Common.UI;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using UniRx;
using Coffee.UIExtensions;
using MonkeyKart.Common.UI.Button;

namespace MonkeyKart.LobbyScene
{
	class StartGameButton : NetworkBehaviour
	{
		ServerLobbyManager serverLobbyManager;
		[SerializeField] GameObject disableImg;
        [SerializeField] UIShiny shiny;
        [SerializeField] SimpleButton btn;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            serverLobbyManager = ServerLobbyManager.I;

            if (!NetworkManager.Singleton.IsServer)
            {
                shiny.enabled = false;
                disableImg.SetActive(true);
                enabled = false;
                return;
            }
			disableImg.SetActive(false);
			btn.OnClick.Subscribe(_ =>
            {
                serverLobbyManager.StartGame();
            }).AddTo(this);
        }
    }
}