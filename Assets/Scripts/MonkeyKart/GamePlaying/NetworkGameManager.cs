using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MonkeyKart.Common;
using MonkeyKart.GamePlaying.Input;
using MonkeyKart.GamePlaying.UI;
using Unity.Netcode.Components;
using Unity.VisualScripting;

namespace MonkeyKart.GamePlaying
{
    public enum GamePhase
    {
        Initializing,
        Playing,
        Ended
    }
    
    public class NetworkGameManager : NetworkBehaviour
    {
        const string TAG = "NetworkGameInitializer";
        [SerializeField] GameObject playerPfb;

        [SerializeField] CanvasGroup gameCanvasGroup;
        [SerializeField] GameStartingUI gameStartingUI;
        [SerializeField] PlayerInput playerInput;
        [SerializeField] PlayerCamera playerCamera;
        [SerializeField] GameObject stageCamera;
        [SerializeField] List<GameObject> spawnPoints;

        readonly Dictionary<ulong, bool> playersReady = new();
        public NetworkVariable<GamePhase> currentPhase;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            currentPhase.Value = GamePhase.Initializing;
            currentPhase.OnValueChanged += OnGamePhaseChanged;
            if (IsServer) ServerInitializeGame();
        }
        
        // クライアントがゲーム状態の変化を受け取るコールバック
        void OnGamePhaseChanged(GamePhase prev, GamePhase current)
        {
            Log.d(TAG, $"GamePhase changed: {prev} --> {current}");
            switch (current)
            {
                case GamePhase.Initializing:
                    break;
                case GamePhase.Playing:
                    ClientStartGame();
                    break;
                case GamePhase.Ended:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(current), current, null);
            }
        }

        void ClientStartGame()
        {
            var player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId);
            player.GetComponent<PlayerMovement>().enabled = true;
            playerCamera.enabled = true;
            gameCanvasGroup.DOFade(1, 0.5f);
        }
        
        public override void OnNetworkDespawn()
        {
            currentPhase.OnValueChanged -= OnGamePhaseChanged;
            playerCamera.enabled = false;
            DOTween.Kill(transform);
        }

        void ServerInitializeGame()
        {
            var clients = NetworkManager.Singleton.ConnectedClientsList;
            // まずサーバがクライアント分のプレイヤーをスポーンさせる
            for (int i = 0; i < clients.Count; i++)
            {
                ServerSpawnPlayer(clients[i].ClientId, spawnPoints[i].transform.position, spawnPoints[i].transform.rotation);
            }
            SetUpClientRpc();　// その後、スポーンした自機プレイヤーをクライアント側が特定し、クライアント側のカメラなどをセットアップ。
        }
        
        void ServerSpawnPlayer(ulong targetClientId, Vector3 position, Quaternion rotation)
        {
            playersReady[targetClientId] = false;
            var ins = Instantiate(playerPfb, position, rotation);
            ins.GetComponent<NetworkObject>().SpawnAsPlayerObject(targetClientId);
        }

        [ClientRpc]
        void SetUpClientRpc()
        {
            // 自機を取得
            var player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId);
            playerCamera.Init(player.GetComponent<Rigidbody>()); // カメラにRigidbodyを注入
            player.GetComponent<PlayerMovement>().Init(playerInput); // PlayerMovementに入力を注入
            ClientPrepareGame().Forget();
        }

        // カメラ移動などを済ませ、サーバに準備完了RPCを送信する。
        async UniTask ClientPrepareGame()
        {
            gameStartingUI.ShowCourseUI();
            await stageCamera.transform.DOMoveZ(30, 10f).AsyncWaitForCompletion();
            await gameStartingUI.HideCourseUI();
            playerCamera.gameObject.SetActive(true);
            stageCamera.gameObject.SetActive(false);
            SendReadyServerRpc();
        }

        // サーバはクライアント全員が準備完了した際に、ゲームをスタートさせる。
        [ServerRpc(RequireOwnership = false)]
        void SendReadyServerRpc(ServerRpcParams serverRpcParams = default)
        {
            playersReady[serverRpcParams.Receive.SenderClientId] = true;
            if (playersReady.Values.All(ready => ready)) ServerStartGame();
        }

        async void ServerStartGame()
        {
            ShowGameStartUIClientRpc();
            // ここから3秒後に強制的に始める。準備の同期を取るのはカウントダウン前にする（カウントダウン後にラグがあると不自然なので）
            await UniTask.Delay(3000);
            currentPhase.Value = GamePhase.Playing; // ゲームを開始する。
        }

        [ClientRpc]
        void ShowGameStartUIClientRpc()
        {
            gameStartingUI.CountDown().Forget();
        }
    }
}