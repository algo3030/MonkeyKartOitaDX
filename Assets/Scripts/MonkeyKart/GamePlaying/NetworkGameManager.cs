using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MonkeyKart.Common;
using MonkeyKart.GamePlaying.Checkpoint;
using MonkeyKart.GamePlaying.Input;
using MonkeyKart.GamePlaying.Ranking;
using MonkeyKart.GamePlaying.UI;
using MonkeyKart.Networking.ConnectionManagement;
using MonkeyKart.Networking.Session;
using VContainer;
using UniRx;

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

        [Inject] ConnectionManager connectionManager;
        SessionManager sessionManager;

        [SerializeField] GameObject playerPfb;

        [SerializeField] CanvasGroup gameCanvasGroup;
        [SerializeField] GameStartingUI gameStartingUI;
        [SerializeField] GameEndingUI gameEndingUI;
        [SerializeField] PlayerInput playerInput;
        [SerializeField] PlayerCamera playerCamera;
        [SerializeField] GameObject stageCamera;
        [SerializeField] List<GameObject> spawnPoints;
        
        [SerializeField] CheckpointManager cpManager;
        [SerializeField] RankingManager rankingManager;

        readonly Dictionary<ulong, bool> playersReady = new();
        List<ulong> goaledPlayers = new();
        public NetworkVariable<GamePhase> currentPhase;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            currentPhase.Value = GamePhase.Initializing;
            currentPhase.OnValueChanged += OnGamePhaseChanged;
            sessionManager = connectionManager.GetSessionManager();
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
            var player =
                NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId);
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
                ServerSpawnPlayer(clients[i].ClientId, spawnPoints[i].transform.position,
                    spawnPoints[i].transform.rotation);
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
            var player =
                NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId);
            playerCamera.Init(player.GetComponent<Rigidbody>()); // カメラにRigidbodyを注入
            player.GetComponent<PlayerMovement>().Init(playerInput); // PlayerMovementに入力を注入
            
            // プログレス
            var localProgress = player.GetComponent<PlayerProgress>();
            localProgress.Init(cpManager);
            localProgress.Laps.Subscribe(lap =>
            {
                if (lap == 2)
                {
                    gameEndingUI.ShowFinishUI();
                    SendGoalServerRpc();
                }
            }).AddTo(this);
            rankingManager.AddPlayer(localProgress, true);
            foreach (var pid in NetworkManager.Singleton.ConnectedClientsIds)
            {
                if(pid == NetworkManager.Singleton.LocalClientId) continue;
                var prog = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(pid)
                    .GetComponent<PlayerProgress>();
                prog.Init(cpManager);
                rankingManager.AddPlayer(prog,false);
            }
            
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

        // 開始処理
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

        // 終了処理
        [ServerRpc(RequireOwnership = false)]
        void SendGoalServerRpc(ServerRpcParams serverRpcParams = default)
        {
            goaledPlayers.Add(serverRpcParams.Receive.SenderClientId);
            if (goaledPlayers.Count == NetworkManager.ConnectedClientsIds.Count) ServerEndGame();
        }

        async void ServerEndGame()
        {
            currentPhase.Value = GamePhase.Ended;
            await UniTask.Delay(1000);
            ShowResultUIClientRpc();
        }

        [ClientRpc]
        void ShowResultUIClientRpc(ClientRpcParams rpcParams = default)
        {
            List<string> goaledPlayerNames = new();
            foreach (var id in goaledPlayers)
            {
                sessionManager.GetPlayerData(id).OnSuccess(data => goaledPlayerNames.Add(data.PlayerName));
            }
            gameEndingUI.ShowResultUI(goaledPlayerNames, NetworkManager.Singleton.LocalClientId);
        }
    }
}