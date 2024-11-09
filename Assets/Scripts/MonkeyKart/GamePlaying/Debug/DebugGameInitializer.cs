using MonkeyKart.Common.UI.Button;
using MonkeyKart.GamePlaying.Checkpoint;
using MonkeyKart.GamePlaying.Input;
using UnityEngine;
using UniRx;
using Unity.Netcode;
using MonkeyKart.GamePlaying.Ranking;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine.Serialization;

namespace MonkeyKart.GamePlaying.Debug
{
    public class DebugGameInitializer : NetworkBehaviour
    {
        [SerializeField] PlayerCamera playerCamera;
        [SerializeField] PlayerInput playerInput;
        [SerializeField] CheckpointManager cpManager;
        [SerializeField] RankingManager rankingManager;
        [SerializeField] ProgressIndicator indicator;
        [SerializeField] ItemManager itemManager;
        [SerializeField] LapUI lapUI;

        [SerializeField] SimpleButton startHostBtn;
        [SerializeField] SimpleButton startClientBtn;
        
        void Awake()
        {
            startHostBtn.OnClick.Subscribe(_ =>
            {
                NetworkManager.Singleton.StartHost();
            }).AddTo(this);

            startClientBtn.OnClick.Subscribe(_ =>
            {
                NetworkManager.Singleton.StartClient();
            }).AddTo(this);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            Init().Forget();
        }
        
        async UniTask Init()
        {
            await UniTask.Delay(1000);
            
            var myPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            var playerMovement = myPlayer.GetComponent<PlayerMovement>();
            var playerRb = myPlayer.GetComponent<Rigidbody>();
            var playerProgress = myPlayer.GetComponent<PlayerProgress>();
            var arrow = myPlayer.GetComponentInChildren<MovementArrow>();
            playerMovement.Init(playerInput);
            playerMovement.enabled = true;
            playerCamera.Init(playerRb);
            playerCamera.enabled = true;
            playerProgress.Init(cpManager);
            lapUI.Init(playerProgress);
            arrow.Init(playerInput);
            arrow.enabled = true;
            indicator.Init(playerProgress);
            itemManager.Init(playerRb);
            playerCamera.enabled = true;
        }
    }
}
