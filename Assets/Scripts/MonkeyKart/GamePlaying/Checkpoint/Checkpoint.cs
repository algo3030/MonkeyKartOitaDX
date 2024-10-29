using System;
using MonkeyKart.Common;
using UnityEngine;

namespace MonkeyKart.GamePlaying.Checkpoint
{
    public class Checkpoint : MonoBehaviour
    { 
        int index;

        public void Init(int index){
            this.index = index;
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent<PlayerProgress>(out var progress)) return;
            progress.PassCheckpoint(index);
        }
    }
}
