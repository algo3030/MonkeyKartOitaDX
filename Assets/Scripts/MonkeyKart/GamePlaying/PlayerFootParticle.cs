using System;
using UnityEngine;

namespace MonkeyKart.GamePlaying
{
    public class PlayerFootParticle : MonoBehaviour
    {
        [SerializeField] VelocityFromPosition velocity;
        ParticleSystem footParticle;

        void Start()
        {
            footParticle = GetComponent<ParticleSystem>();
        }

        void Update()
        {
            var emission = footParticle.emission;
            emission.rateOverTime = velocity.CurrentVelocity.magnitude * 2f;
        }
    }
}
