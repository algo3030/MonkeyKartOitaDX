using Cysharp.Threading.Tasks;
using MonkeyKart.Common;
using MonkeyKart.SceneManagement;
using UnityEngine.SceneManagement;
using VContainer;

namespace MonkeyKart.Networking.ConnectionManagement
{
    /// <summary>
    /// �I�t���C�����̃X�e�[�g�B
    /// ���ꎞ��NetworkManager���V���b�g�_�E������B
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
            // �z�[���ȊO�ɋ���΃z�[���ɑJ��
            if (SceneManager.GetActiveScene().name != MonkeyKartScenes.HOME)
            {
                owner.sceneLoader.LoadScene(MonkeyKartScenes.HOME, useNetworkSceneManager: false);
            }
        }

        public override void Exit() { }
    }
}