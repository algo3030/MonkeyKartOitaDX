using System;
using Cysharp.Threading.Tasks;
using MonkeyKart.Common;
using MonkeyKart.GamePlaying.Input;
using UnityEngine;

namespace MonkeyKart.GamePlaying
{
    /// <summary>
    /// プレイヤー移動用スクリプト。
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        const string TAG = "PlayerMovement";
        
        const float MiniTurboThreshold = 1.5f;
        const float SuperTurboThreshold = 2.5f;
        const float BaseAcceleration = 23f;
        const float DriftHorizontalMultiplier = 1.3f;
        const float MaxSpeed = 25f;
        
        IPlayerInputNotifier playerInput;
        Rigidbody rb;
        const float RotationAcceleration = 750f;

        float maxSpeed = MaxSpeed;
        float acceleration = BaseAcceleration;
        public float Turbo { get; private set; }
        
        float yRotation;
        float rotationSpeed;

        float inputVector;

        public void Init(IPlayerInputNotifier input)
        {
            playerInput = input;
        }

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
           
        }

        void Start()
        {
            yRotation = transform.rotation.eulerAngles.y;
            // TODO:ドリフト
            
            /*
            input.OnDriftReleased.Subscribe(_ =>
            {
                Accelerate(2.0f, 1.0f, 0.5f).Forget();
            }).AddTo(this);
            */
        }

        float targetX;
        void FixedUpdate()
        {
            // ステアリング
            var handle = 0f;
            if (playerInput.InputVector.x != 0f) handle = playerInput.InputVector.x;
            if (handle == 0f) rotationSpeed *= 0.5f; // ハンドリングを中央に近づける
            rotationSpeed += handle * RotationAcceleration * Time.fixedDeltaTime;
            rotationSpeed = Mathf.Clamp(rotationSpeed, 70f * handle, 70f * handle);
            yRotation += rotationSpeed * Time.fixedDeltaTime;
            
            if (Physics.Raycast(transform.position, Vector3.down, out var hit, 1.0f))
            {
                targetX = Quaternion.FromToRotation(Vector3.down, hit.normal).eulerAngles.x;
            }
            else
            {
                targetX = Mathf.LerpAngle(targetX, 0f, Time.fixedDeltaTime);
            }

            var currentX = transform.rotation.eulerAngles.x;
            var moveX = Mathf.LerpAngle(currentX, targetX, 4f * Time.fixedDeltaTime);
            rb.MoveRotation(Quaternion.Euler(moveX, yRotation,0));
            
            // 加速
            rb.AddForce(acceleration * transform.forward);
            rb.AddForce(Vector3.down * 10f);
            ClampVelocity();
        }

        async UniTask Accelerate(float multiplier, float accelerateSeconds, float decelerateSeconds)
        {
            maxSpeed = MaxSpeed * multiplier;
            acceleration *= multiplier;
            rb.velocity *= 1.3f;
            await UniTask.WaitForSeconds(accelerateSeconds);
            acceleration = BaseAcceleration;
            
            var elapsed = 0f;
            while (elapsed < decelerateSeconds)
            {
                elapsed += Time.deltaTime;
                var t = elapsed / decelerateSeconds;

                maxSpeed = Mathf.Lerp(MaxSpeed * multiplier, MaxSpeed, t);
                await UniTask.Yield();
            }
            maxSpeed = MaxSpeed;
        }

        void ClampVelocity()
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        }
    }
}