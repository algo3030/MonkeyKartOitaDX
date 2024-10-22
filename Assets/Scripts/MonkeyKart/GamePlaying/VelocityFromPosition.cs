using UnityEngine;

namespace MonkeyKart.GamePlaying
{
    /// <summary>
    /// NetworkRigidbodyではRigidbodyの値が同期されないのでやむを得ず・・・
    /// </summary>
    public class VelocityFromPosition : MonoBehaviour
    {
        Vector3 previousPosition;
        public Vector3 CurrentVelocity { get; private set; }

        void Start()
        {
            previousPosition = transform.position;
        }

        void Update()
        {
            CurrentVelocity = (transform.position - previousPosition) / Time.deltaTime;
            previousPosition = transform.position;
        }
    }
}