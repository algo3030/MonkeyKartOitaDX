using System;
using Cysharp.Threading.Tasks;
using MonkeyKart.Common;
using UnityEngine;

namespace MonkeyKart.GamePlaying.Item.Items
{
    public class HotSpringItem : MonoBehaviour
    {
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip spawnClip;
        [SerializeField] AudioClip onEnterPlayerClip;

        void Start()
        {
            audioSource.PlayOneShot(spawnClip);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(MonkeyKartTags.Player))
            {
                audioSource.PlayOneShot(onEnterPlayerClip);
                var movement = other.GetComponent<PlayerMovement>();
                movement.Stun(3000).Forget();
                movement.transform.position =
                    new Vector3(transform.position.x, movement.transform.position.y, transform.position.z);
            }
        }
    }
}
