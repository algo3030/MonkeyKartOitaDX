using System;
using UnityEngine;

namespace MonkeyKart.GamePlaying
{
    public class PlayerAnimator : MonoBehaviour
    {
        static readonly int RunSpeed = Animator.StringToHash("RunSpeed");
        Animator animator;
        [SerializeField] VelocityFromPosition velocity;

        void Start()
        {
            animator = GetComponent<Animator>();
        }

        void Update()
        {
            animator.SetFloat(RunSpeed, velocity.CurrentVelocity.magnitude / 30f);
        }
    }
}
