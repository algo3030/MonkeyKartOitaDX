using System;
using MonkeyKart.GamePlaying.Input;
using UnityEngine;

namespace MonkeyKart.GamePlaying
{
    public class MovementArrow : MonoBehaviour
    {
        [SerializeField] Vector3 offset;
        [SerializeField] PlayerInput input;
        
        const float MAX_ANGLE_X = 45f;
        const float MAX_ANGLE_Y = 75f;
        bool shouldApplyYInput;

        void LateUpdate()
        {
            var targetRotation = Quaternion.Euler(shouldApplyYInput ? input.InputVector.y * MAX_ANGLE_X : 0f, input.InputVector.x * MAX_ANGLE_Y, 0);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}
