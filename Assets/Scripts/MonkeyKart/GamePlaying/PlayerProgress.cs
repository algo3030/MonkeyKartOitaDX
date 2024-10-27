using System;
using MonkeyKart.Common;
using UnityEngine;

namespace MonkeyKart.GamePlaying
{
    public class PlayerProgress : MonoBehaviour
    {
        float progress;
        int lastPassedCp = 0;
        Vector3 prevCpPos;
        Vector3 nextCpPos;

        public void PassCheckpoint(int cpIdx, Vector3 prev, Vector3 next)
        {
            lastPassedCp = cpIdx;
            prevCpPos = prev;
            nextCpPos = next;
        }

        void Update()
        {
            progress = lastPassedCp;
            Log.d("PlayerProgress", $"progress: {progress}");
        }
    }
}
