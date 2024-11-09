using System;
using MonkeyKart.GamePlaying.Input;
using UnityEngine;

namespace MonkeyKart.GamePlaying
{
    public class MovementArrow : MonoBehaviour
    {
        [SerializeField] Vector3 offset;
        PlayerInput input;

        const float MAX_ANGLE_X = 45f;

        public void Init(PlayerInput input)
        {
            this.input = input;
        }

        void LateUpdate()
        {
            if (input == null)
            {
                gameObject.SetActive(false);
                return;
            }

            var targetRotation 
                = Quaternion.Euler(0f, Mathf.Acos(-input.InputVector.x) * Mathf.Rad2Deg + 270f, 0);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}