using UnityEngine;

namespace MonkeyKart.GamePlaying
{
    public class PlayerAudio : MonoBehaviour
    {
        [SerializeField] AudioSource audioSource;
        [SerializeField] VelocityFromPosition velocity;
        
        void Update()
        {
            audioSource.pitch = velocity.CurrentVelocity.magnitude / 10f;
        }
    }
}
