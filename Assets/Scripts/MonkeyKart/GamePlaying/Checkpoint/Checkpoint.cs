using System;
using MonkeyKart.Common;
using UnityEngine;

namespace MonkeyKart.GamePlaying.Checkpoint
{
    public class Checkpoint : MonoBehaviour
    { 
        int index;

        void Awake()
        {
            var parent = transform.parent;
            for (var i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i) != transform) continue;
                index = i;
                break;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.TryGetComponent<PlayerProgress>(out var progress)) return;
            var nextCpPos = transform.parent.GetChild(index + 1).position;
            progress.PassCheckpoint(index, transform.position, nextCpPos);
            Log.d($"{gameObject.name}","Checkpoint passed");
        }
    }
}
