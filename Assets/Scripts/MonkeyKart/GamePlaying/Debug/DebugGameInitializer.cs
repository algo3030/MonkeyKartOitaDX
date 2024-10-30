using MonkeyKart.Common.UI.Button;
using MonkeyKart.GamePlaying.Checkpoint;
using MonkeyKart.GamePlaying.Input;
using UnityEngine;
using UniRx;
using Unity.Netcode;

namespace MonkeyKart.GamePlaying.Debug
{
    public class DebugGameInitializer : MonoBehaviour
    {
        [SerializeField] Rigidbody playerRb;
        [SerializeField] PlayerMovement playerMovement;
        [SerializeField] PlayerProgress playerProgress;
        [SerializeField] PlayerCamera playerCamera;
        [SerializeField] PlayerInput playerInput;
        [SerializeField] CheckpointManager cpManager;

        [SerializeField] SimpleButton startButton;
        void Awake()
        {
            playerMovement.Init(playerInput);
            playerCamera.Init(playerRb);
            playerProgress.Init(cpManager);
            
            startButton.OnClick.Subscribe(_ =>
            {
                Initialize();
            }).AddTo(this);
        }

        void Initialize()
        {
            NetworkManager.Singleton.StartHost();
            playerMovement.enabled = true;
            playerCamera.enabled = true;
        }
    }
}
