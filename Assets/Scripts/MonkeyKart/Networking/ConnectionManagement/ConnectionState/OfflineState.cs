using Cysharp.Threading.Tasks;
using MonkeyKart.Common;
using MonkeyKart.SceneManagement;
using UnityEngine.SceneManagement;
using VContainer;

namespace MonkeyKart.Networking.ConnectionManagement
{
    /// <summary>
    /// オフライン時のステート。
    /// 遷移時にNetworkManagerをシャットダウンする。
    /// </summary>
    public class OfflineState : ConnectionState
    {
        const string TAG = "OfflineState";

        public DisconnectReason Reason { get; private set; }

        public OfflineState(ConnectionManager connectionManager, DisconnectReason reason) : base(connectionManager)
        {
            Reason = reason;
        }

        public override bool CanTransitionTo(ConnectionState next)
        {
            return next is ClientConnectingState || next is StartingHostState;
        }

        public override void Enter()
        {
            Log.d(TAG, $"Went to offline. Reason: {Reason}");
            owner.networkManager.Shutdown();
            // ホーム以外にいればホームに遷移
            if (SceneManager.GetActiveScene().name != MonkeyKartScenes.HOME)
            {
                owner.sceneLoader.LoadScene(MonkeyKartScenes.HOME, useNetworkSceneManager: false);
            }
        }

        public override void Exit() { }
    }
}