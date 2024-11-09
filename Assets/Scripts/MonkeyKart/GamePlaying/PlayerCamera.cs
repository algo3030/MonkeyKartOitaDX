using Unity.Mathematics;
using UnityEngine;
namespace MonkeyKart.GamePlaying
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] Transform pivotTfm;
        Rigidbody playerRb;

        public void Init(Rigidbody targetRb)
        {
            playerRb = targetRb;
            pivotTfm.position = playerRb.position;
            pivotTfm.rotation = playerRb.transform.rotation * Quaternion.Euler(0, 180, 0);
        }
        
        void LateUpdate()
        {
            pivotTfm.position = playerRb.position;

            if (playerRb.velocity.magnitude <= 0.01f) return;
            var lookRot = Quaternion.LookRotation(-playerRb.velocity);
            var targetEulerAngles = lookRot.eulerAngles;
            targetEulerAngles.x = 0f;
            targetEulerAngles.z = 0f;
            var targetRotation = Quaternion.Euler(targetEulerAngles);
            pivotTfm.rotation = Quaternion.Slerp(pivotTfm.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}
