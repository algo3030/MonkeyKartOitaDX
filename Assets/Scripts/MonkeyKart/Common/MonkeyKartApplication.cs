using Unity.Netcode;
using VContainer;
using VContainer.Unity;
using UnityEngine;
using MonkeyKart.SceneManagement;
using MonkeyKart.Networking.ConnectionManagement;
using Cysharp.Threading.Tasks;
using MonkeyKart.Home;
using UnityEngine.SceneManagement;

namespace MonkeyKart.Common
{
    class MonkeyKartApplication : LifetimeScope
    {
        [SerializeField] NetworkManager networkManager;
        [SerializeField] ConnectionManager connectionManager;
        [SerializeField] SceneLoader sceneLoader;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.Register<ProfileManager>(Lifetime.Singleton);
            builder.RegisterComponent(networkManager);
            builder.RegisterComponent(connectionManager);
            builder.RegisterComponent(sceneLoader);
        }

        async void Start()
        {
            DontDestroyOnLoad(gameObject);
            Application.wantsToQuit += OnWantsToQuit;
            Application.targetFrameRate = 60;
#if UNITY_EDITOR
            // デバッグ時はログインしていきなりホームへ
            (await connectionManager.Initialize())
                .OnFailure(err =>
                {
                    Log.e("DEBUG", $"Connection initialization failed. reason:{err}");
                    Application.Quit();
                });
#else
            SceneManager.LoadScene(MonkeyKartScenes.TITLE);
#endif
        }

        protected override void OnDestroy()
        {
            if (connectionManager.CurrentState.Value is OfflineState) return;
            connectionManager.ChangeState(new OfflineState(connectionManager, DisconnectReason.ShutdownByMe));
        }

        bool OnWantsToQuit()
        {
            Application.wantsToQuit -= OnWantsToQuit;
            var canQuit = connectionManager.CurrentState.Value is OfflineState;
            if(!canQuit) DisconnectBeforeQuit();
            return canQuit;
        }
        
        // 終了処理
        async void DisconnectBeforeQuit()
        {
            connectionManager.ChangeState(new OfflineState(connectionManager, DisconnectReason.ShutdownByMe));
            await UniTask.WaitUntil(() => connectionManager.CurrentState.Value is OfflineState);
            Application.Quit();
        }
    }
}