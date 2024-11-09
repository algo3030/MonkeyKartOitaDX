using UnityEngine;

namespace MonkeyKart.GamePlaying
{
    public class PlayerAudio : MonoBehaviour
    {
        [SerializeField] AudioSource footStepsChannel;
        [SerializeField] AudioSource seChannel;
        [SerializeField] AudioClip dash;
        [SerializeField] AudioClip cry;
        [SerializeField] AudioClip damaged;
        [SerializeField] VelocityFromPosition velocity;
        
        void Update()
        {
            footStepsChannel.pitch = velocity.CurrentVelocity.magnitude / 10f;
        }

        public void MakeDashSE()
        {
            seChannel.PlayOneShot(dash);
            seChannel.PlayOneShot(cry);
        }

        public void MakeDamageSE()
        {
            seChannel.PlayOneShot(damaged);
        }
    }
}
