using MonkeyKart.Common;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using MonkeyKart.Common.UI;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

namespace MonkeyKart.SceneManagement
{
    /// <summary>
    /// シーンロード
    /// </summary>
    public class SceneLoader : NetworkBehaviour
    {
        const string TAG = "SceneLoader";

        [SerializeField] SceneLoadingCanvas sceneLoadingCanvas;

        static SceneLoader instance;

        bool IsNetworkSceneManagementEnabled => 
            NetworkManager != null && 
            NetworkManager.SceneManager != null && 
            NetworkManager.NetworkConfig.EnableSceneManagement;

        bool isInitialized = false;

        public virtual void Awake()
        {
            if (instance != null) throw new InvalidOperationException("SceneLoader instance is already exists.");

            instance = this;
            DontDestroyOnLoad(this);
        }

        public virtual void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            // Server side
            NetworkManager.OnServerStarted += OnNetworkingSessionStarted;
            NetworkManager.OnServerStopped += OnNetworkingSessionEnded;
            // Client side
            NetworkManager.OnClientStopped += OnNetworkingSessionEnded;
            NetworkManager.OnClientStarted += OnNetworkingSessionStarted;
        }
        public override void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (NetworkManager != null)
            {
                NetworkManager.OnServerStarted -= OnNetworkingSessionStarted;
                NetworkManager.OnClientStarted -= OnNetworkingSessionStarted;
                NetworkManager.OnServerStopped -= OnNetworkingSessionEnded;
                NetworkManager.OnClientStopped -= OnNetworkingSessionEnded;
            }
            base.OnDestroy();
        }

        // �I�t���C���ł̃��[�h
        void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (!IsSpawned || NetworkManager.ShutdownInProgress)
            {
                EndSceneLoad();
            }
        }

        void OnNetworkingSessionStarted()
        {
            if (!isInitialized)
            {
                if (IsNetworkSceneManagementEnabled)
                {
                    NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
                }

                isInitialized = true;
            }
        }

        void OnNetworkingSessionEnded(bool _)
        {
            if(isInitialized)
            {
                if (IsNetworkSceneManagementEnabled)
                {
                    NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
                }

                isInitialized = false;
            }
        }

        public void LoadScene(string sceneName, bool useNetworkSceneManager, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            if(useNetworkSceneManager && IsSpawned && IsNetworkSceneManagementEnabled && !NetworkManager.ShutdownInProgress) 
            {
                // �I�����C���Ń��[�h
                if (NetworkManager.IsServer)
                {
                    sceneLoadingCanvas.Show();
                    NetworkManager.SceneManager.LoadScene(sceneName, loadSceneMode);
                }
            }
            else
            {
                // �I�t���C���Ń��[�h
                if (loadSceneMode == LoadSceneMode.Single)
                {
                    sceneLoadingCanvas.Show();
                }
                SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            }
        }

        void EndSceneLoad()
        {
            UniTask.Create(async () =>
            {
                await UniTask.Delay(1000);
                sceneLoadingCanvas.Hide();
            }).Forget();
        }

        void OnSceneEvent(SceneEvent sceneEvent)
        {
            Log.d(TAG, $"SceneEvent recieved. ClientId:{sceneEvent.ClientId}, Type: {sceneEvent.SceneEventType}, " +
                $"Scene: {sceneEvent.SceneName}");

            switch (sceneEvent.SceneEventType)
            {
                case SceneEventType.Load: // NOTE: ���[�h�������ƌĂ΂�Ȃ��Ƃ�������
                    if (NetworkManager.IsClient)
                    {
                        sceneLoadingCanvas.Show();
                    }
                    break;
                case SceneEventType.LoadEventCompleted:
                    if (NetworkManager.IsClient)
                    {
                        EndSceneLoad();
                    }
                    break;
                case SceneEventType.Synchronize:
                    if(NetworkManager.IsClient && !NetworkManager.IsHost)
                    {
                        if(NetworkManager.SceneManager.ClientSynchronizationMode == LoadSceneMode.Single)
                        {
                            UnloadAdditiveScenes();
                        }
                    }
                    break;
                case SceneEventType.SynchronizeComplete:
                    if (NetworkManager.IsServer)
                    {
                        ClientStopLoadingScreenRpc(RpcTarget.Group(new[] { sceneEvent.ClientId }, RpcTargetUse.Temp));
                    }
                    break;
            }
        }

        void UnloadAdditiveScenes()
        {
            var activeScene = SceneManager.GetActiveScene();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && scene != activeScene)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void ClientStopLoadingScreenRpc(RpcParams rpcParams = default)
        {
            EndSceneLoad();
        }
    }
}