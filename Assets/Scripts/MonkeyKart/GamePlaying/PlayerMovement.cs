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
        const float BaseAcceleration = 17f;
        const float DriftHorizontalMultiplier = 1.3f;
        const float MaxSpeed = 20f;
        
        IPlayerInputNotifier playerInput;
        Rigidbody rb;
        const float RotationAcceleration = 600f;

        float maxSpeed = MaxSpeed;
        float acceleration = BaseAcceleration;
        public float Turbo { get; private set; }
        
        float rotation;
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
            rotation = transform.rotation.eulerAngles.y;
            // TODO:ドリフト
            
            /*
            input.OnDriftReleased.Subscribe(_ =>
            {
                Accelerate(2.0f, 1.0f, 0.5f).Forget();
            }).AddTo(this);
            */
        }

        void FixedUpdate()
        {
            Log.d(TAG,"あ");
            // ステアリング
            var handle = 0f;
            if (playerInput.InputVector.x != 0f) handle = playerInput.InputVector.x;
            if (handle == 0f) rotationSpeed *= 0.5f; // ハンドリングを中央に近づける
            rotationSpeed += handle * RotationAcceleration * Time.fixedDeltaTime;
            rotationSpeed = Mathf.Clamp(rotationSpeed, 70f * handle, 70f * handle);
            rotation += rotationSpeed * Time.fixedDeltaTime;
            rb.MoveRotation(Quaternion.Euler(0,rotation,0));
            
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