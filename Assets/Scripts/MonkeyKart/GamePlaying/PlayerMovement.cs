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

        [SerializeField] PlayerAudio playerAudio;
        const float MiniTurboThreshold = 1.5f;
        const float SuperTurboThreshold = 2.5f;
        const float BaseAcceleration = 30f;
        const float DriftHorizontalMultiplier = 1.3f;
        const float MaxSpeed = 23f;
        
        IPlayerInputNotifier playerInput;
        Rigidbody rb;
        const float RotationAcceleration = 20 * 1000f;

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

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(MonkeyKartTags.DashBoard)) Dash();
            else if (other.CompareTag("Kabosu"))
            {
                
                Stun(3000).Forget();
            }
        }

        public async UniTask Stun(int ms)
        {
            playerAudio.MakeDamageSE(); 
            maxSpeed = 0f;
            await UniTask.Delay(ms);
            maxSpeed = MaxSpeed;
        }
        
        public void Dash()
        {
            Accelerate(2.0f, 1.0f, 1.0f).Forget();
            playerAudio.MakeDashSE();
        }

        void Start()
        {
            yRotation = transform.rotation.eulerAngles.y;
            rb.useGravity = true;
            // TODO:ドリフト
        }

        float targetX;
        void FixedUpdate()
        {
            //if (playerInput.InputVector.magnitude == 0) return;
            
            // ステアリング
            var handle = 0f;
            if (playerInput.InputVector.x != 0f) handle = playerInput.InputVector.x;
            if (handle == 0f) rotationSpeed *= 0.5f; // ハンドリングを中央に近づける
            rotationSpeed += handle * RotationAcceleration * Time.fixedDeltaTime;
            rotationSpeed = Mathf.Clamp(rotationSpeed, 70f * handle, 70f * handle);
            yRotation += rotationSpeed * Time.fixedDeltaTime;
            
            if (Physics.Raycast(transform.position, Vector3.down, out var hit, 1.5f))
            {
                var rot = Quaternion.FromToRotation(Vector3.up, hit.normal);
                var euler = (rot * transform.rotation).eulerAngles;
                euler.x = (euler.x > 180) ? euler.x - 360 : euler.x;
                targetX = euler.x / 2f;
            }
            else
            {
                targetX = Mathf.LerpAngle(targetX, 0f, Time.fixedDeltaTime);
            }

            var currentX = transform.rotation.eulerAngles.x;
            var moveX = Mathf.LerpAngle(currentX, targetX, 10f * Time.fixedDeltaTime);
            rb.MoveRotation(Quaternion.Euler(moveX, yRotation,0));
            
            // 加速
            rb.AddForce(acceleration * transform.forward);
            rb.AddForce(Vector3.down * 20f);
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