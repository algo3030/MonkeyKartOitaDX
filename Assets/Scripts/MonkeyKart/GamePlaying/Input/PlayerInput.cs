using System;
using MonkeyKart.Common;
using MonkeyKart.Common.UI.Button;
using TMPro;
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
                var tmp = inputVector;
                tmp.x = Mathf.Clamp(tmp.x, -1.0f, 1.0f);
                tmp.y = Mathf.Clamp(tmp.y, -1.0f, 1.0f);
                return tmp;
            }
        }

        [SerializeField] TextMeshProUGUI text;
        readonly ReactiveProperty<bool> isDrifting = new();
        public IReadOnlyReactiveProperty<bool> IsDrifting => isDrifting;
        Vector2 initialPressPos;
        Vector2 inputVector;
        MonkeyKartInput gameInput;

        void Start()
        {
            if (UnityEngine.InputSystem.Gyroscope.current != null)
            {
                InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
            }
            if (AttitudeSensor.current != null)
            {
                InputSystem.EnableDevice(AttitudeSensor.current);
            }
            
            gameInput = new MonkeyKartInput();
            gameInput.GamePlaying.SetCallbacks(this);
            gameInput.GamePlaying.Enable();
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
            inputVector = Vector2.zero;
            if (pointer != null && pointer.press.ReadValue() > 0f)
            {
                if(pointer.press.wasPressedThisFrame) initialPressPos = pointer.position.ReadValue();
                inputVector = (pointer.position.ReadValue() - initialPressPos) / new Vector2(Screen.width, Screen.height) * INPUT_MULTIPLIER;
            }
            else initialPressPos = Vector2.zero;

            return;
            if (Accelerometer.current != null)
            {
                var rot = AttitudeSensor.current.attitude.ReadValue();
                inputVector += (Vector2)(rot * Vector2.up) * 2f - Vector2.right * 0.3f;
            }
        }
    }
}