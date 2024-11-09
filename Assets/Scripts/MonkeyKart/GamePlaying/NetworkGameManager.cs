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
using VContainer;
using UniRx;
using MonkeyKart.LobbyScene;
using MonkeyKart.SceneManagement;
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


        [Inject] SceneLoader sceneLoader;
        [SerializeField] GameObject playerPfb;
        
        [SerializeField] GameObject playerNameDisplayPfb;

        [SerializeField] CanvasGroup gameCanvasGroup;
        [SerializeField] GameStartingUI gameStartingUI;
        [SerializeField] GameEndingUI gameEndingUI;
        [SerializeField] PlayerInput playerInput;
        [SerializeField] PlayerCamera playerCamera;
        [SerializeField] GameObject stageCamera;
        [SerializeField] List<GameObject> spawnPoints;
        
        [SerializeField] CheckpointManager cpManager;
        [SerializeField] RankingManager rankingManager;
        [SerializeField] GameAudioManager audioManager;
        [SerializeField] LapUI lapDisplay;
        [SerializeField] ProgressIndicator indicator;
        [SerializeField] ItemManager itemManager;

        List<PlayerProgress> progresses = new();

        readonly Dictionary<ulong, bool> playersReady = new();
        NetworkList<ulong> goaledPlayers;
        public NetworkVariable<GamePhase> currentPhase;

        private PlayerMovement localPlayerMovement;

        void Awake()
        {
            goaledPlayers = new();
            gameCanvasGroup.alpha = 0f;
            rankingManager.enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            currentPhase.Value = GamePhase.Initializing;
            currentPhase.OnValueChanged += OnGamePhaseChanged;
            if (IsServer) ServerInitializeGame();
            SetUp();
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

        async void ClientStartGame()
        {
            progresses.ForEach(p => p.enabled = true); // プログレスの有効化
            var player =
                NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId);
            localPlayerMovement = player.GetComponent<PlayerMovement>();
            localPlayerMovement.enabled = true; // 移動の有効化
            playerCamera.enabled = true;

            audioManager.SetBGM(audioManager.RaceBGM, delayMs: 800);
            await UniTask.Delay(1000);
            rankingManager.enabled = true; // ランキング判定の有効化
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
        }

        void ServerSpawnPlayer(ulong targetClientId, Vector3 position, Quaternion rotation)
        {
            playersReady[targetClientId] = false;
            var ins = Instantiate(playerPfb, position, rotation);
            ins.GetComponent<NetworkObject>().SpawnAsPlayerObject(targetClientId);
        }

        void SetUp()
        {
            // 自機を取得
            var localPlayer =
                NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId);
            var localPlayerRb = localPlayer.GetComponent<Rigidbody>();
            playerCamera.Init(localPlayerRb); // カメラにRigidbodyを注入
            itemManager.Init(localPlayerRb);
            localPlayer.GetComponent<PlayerMovement>().Init(playerInput); // PlayerMovementに入力を注入
            var arrow = localPlayer.GetComponentInChildren<MovementArrow>();
            arrow.Init(playerInput);
            arrow.enabled = true;
            
            
            // プログレス
            var localProgress = localPlayer.GetComponent<PlayerProgress>();
            progresses.Add(localProgress);
            localProgress.Init(cpManager);
            lapDisplay.Init(localProgress);
            indicator.Init(localProgress);

            localProgress.Laps.Subscribe(lap =>
            {
                if (lap == 2) ClientEndGame();
            }).AddTo(this);
            
            rankingManager.AddPlayer(localProgress, true);

            var objs = GameObject.FindGameObjectsWithTag("Player");
            foreach (var p in objs)
            {
                /*
                var net = p.GetComponent<NetworkObject>();
                var pid = net.OwnerClientId;
                if (pid != NetworkManager.Singleton.LocalClientId)
                {
                    var playerName = ServerLobbyManager.I.GetPlayerState(pid).PlayerName;
                    // 名前表示の有効化
                    Instantiate(playerNameDisplayPfb,gameCanvasGroup.transform).GetComponent<PlayerNameDisplay>().Init(
                        net.transform,
                        pid,
                        playerName
                    );
                }
                */
                
                if(p == localPlayer.gameObject) continue;

                Log.d(TAG,"Setting new player.");
                var prog = p.GetComponent<PlayerProgress>();
                progresses.Add(prog);
                prog.Init(cpManager);
                prog.enabled = true;
                rankingManager.AddPlayer(prog,false);
            }
            
            ClientPrepareGame().Forget();
        }

        // カメラ移動などを済ませ、サーバに準備完了RPCを送信する。
        async UniTask ClientPrepareGame()
        {
            audioManager.PlayBGM(audioManager.CourceIntroBGM); // BGM
            gameStartingUI.ShowCourseUI();
            await stageCamera.transform.DOMoveZ(30, 11f).AsyncWaitForCompletion();
            await gameStartingUI.HideCourseUI();
            playerCamera.gameObject.SetActive(true);
            stageCamera.gameObject.SetActive(false);
            await UniTask.Delay(1000);
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
            CountDownClientRpc();
            // ここから3秒後に強制的に始める。準備の同期を取るのはカウントダウン前にする（カウントダウン後にラグがあると不自然なので）
            await UniTask.Delay(3000);
            currentPhase.Value = GamePhase.Playing; // ゲームを開始する。
        }

        [ClientRpc]
        void CountDownClientRpc()
        {
            ClientCountDown();
        }

        async void ClientCountDown()
        {
            audioManager.MakeSE(audioManager.CountDownSE); // SE
            await gameStartingUI.CountDown();
        }

        // 終了処理
        void ClientEndGame()
        {
            audioManager.MakeSE(audioManager.GoalSE);
            localPlayerMovement.enabled = false;
            rankingManager.enabled = false;
            gameEndingUI.ShowFinishUI();
            gameCanvasGroup.DOFade(0, 0.5f);
            SendGoalServerRpc();

            audioManager.SetBGMVolume(0f, 1f).Forget();
            audioManager.SetBGM(audioManager.PostGoalBGM, delayMs: 1500);
        }

        [ServerRpc(RequireOwnership = false)]
        void SendGoalServerRpc(ServerRpcParams serverRpcParams = default)
        {
            goaledPlayers.Add(serverRpcParams.Receive.SenderClientId);
            if (goaledPlayers.Count == NetworkManager.ConnectedClientsIds.Count) ServerEndGame();
        }

        async void ServerEndGame()
        {
            currentPhase.Value = GamePhase.Ended;
            await UniTask.Delay(3000);
            ShowResultUIClientRpc();
            await UniTask.Delay(10000);

            var clients = NetworkManager.Singleton.ConnectedClientsList;
            playerCamera.enabled = false;
            // プレイヤーはシーン間で引き継がれてしまうのでDespawn
            for (int i = 0; i < clients.Count; i++)
            {
                NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clients[i].ClientId).Despawn();
            }

            sceneLoader.LoadScene(MonkeyKartScenes.LOBBY, true);
        }

        [ClientRpc]
        void ShowResultUIClientRpc()
        {
            List<string> goaledPlayerNames = new();
            foreach (var id in goaledPlayers)
            {
                goaledPlayerNames.Add(ServerLobbyManager.I.GetPlayerState(id).PlayerName);
            }
            gameEndingUI.ShowResultUI(goaledPlayerNames, goaledPlayers.IndexOf(NetworkManager.Singleton.LocalClientId));
        }
    }
}