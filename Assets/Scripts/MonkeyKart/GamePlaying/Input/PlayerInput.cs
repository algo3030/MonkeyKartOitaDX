using System;
using MonkeyKart.Common.UI.Button;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MonkeyKart.GamePlaying.Input
{
    public class PlayerInput : MonoBehaviour, MonkeyKartInput.IGamePlayingActions, IPlayerInputNotifier
    {
        const string TAG = "PlayerInput";
        const float INPUT_MULTIPLIER = 10f;
        
        // 増幅した上で、-1~1の範囲に制限された入力値
        public Vector2 InputVector
        {
            get
            {
                var tmp = inputVector * INPUT_MULTIPLIER;
                tmp.x = Mathf.Clamp(tmp.x, -1.0f, 1.0f);
                tmp.y = Mathf.Clamp(tmp.y, -1.0f, 1.0f);
                return tmp;
            }
        }

        readonly ReactiveProperty<bool> isDrifting = new();
        public IReadOnlyReactiveProperty<bool> IsDrifting => isDrifting;
        [SerializeField] SimpleButton driftButton;
        Vector2 initialPressPos;
        Vector2 inputVector;
        MonkeyKartInput gameInput;

        void Start()
        {
            gameInput = new MonkeyKartInput();
            gameInput.GamePlaying.SetCallbacks(this);
            gameInput.GamePlaying.Enable();
            
            driftButton.OnClickDown.Subscribe(_ =>
            {
                isDrifting.Value = true;
            }).AddTo(this);
            
            driftButton.OnClickUp.Subscribe(_ =>
            {
                isDrifting.Value = false;
            }).AddTo(this);
        }

        void OnDestroy()
        {
            gameInput.GamePlaying.Disable();
        }

        public void OnDrift(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    isDrifting.Value = true;
                    break;
                case InputActionPhase.Canceled:
                    isDrifting.Value = false;
                    break;
            }
        }

        void Update()
        {
            var pointer = Pointer.current;
            if (pointer != null && pointer.press.ReadValue() > 0f)
            {
                if(pointer.press.wasPressedThisFrame) initialPressPos = pointer.position.ReadValue();
                inputVector = (pointer.position.ReadValue() - initialPressPos) / new Vector2(Screen.width, Screen.height);
            }
            else
            {
                initialPressPos = Vector2.zero;
                inputVector = Vector2.zero;
            }
        }
    }
}