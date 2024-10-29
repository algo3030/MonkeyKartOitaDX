using System;
using System.Linq;
using MonkeyKart.Common;
using MonkeyKart.GamePlaying.Checkpoint;
using Unity.VisualScripting;
using UnityEngine;

namespace MonkeyKart.GamePlaying
{
    public class PlayerProgress : MonoBehaviour
    {
        [SerializeField] CheckpointManager cpManager;
        Rigidbody rb;

        public int Laps{get; private set;}
        public float Progress {get; private set;}
        public int SectionIdx {get; private set;}
        Vector3 passedCpPos;
        Vector3 headingCpPos;

        Vector3 velocity;
        Vector3 prevPos;
        
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            prevPos = transform.position;
            SectionIdx = 0;
            passedCpPos = cpManager.CpPoses[0];
            headingCpPos = cpManager.CpPoses[1];
        }

        public void PassCheckpoint(int passedCp)
        {
            var prevCpPos = cpManager.CpPoses.GetWithLoopedIndex(passedCp - 1);
            var currentCpPos = cpManager.CpPoses[passedCp];
            var nextCpPos = cpManager.CpPoses.GetWithLoopedIndex(passedCp + 1);

            Debug.Log($"prevCp:{prevCpPos}");
            Debug.Log($"nextCp:{nextCpPos}");

            var currentToNext = nextCpPos - currentCpPos;
            var currentToPrev = prevCpPos - currentCpPos;
            var toNextAngle = Vector3.Angle(velocity, currentToNext);
            var toPrevAngle = Vector3.Angle(velocity, currentToPrev);
            bool advancing = toNextAngle < toPrevAngle;
            
            if(SectionIdx >= 0)
            {
                if(SectionIdx == cpManager.CpPoses.Count - 1 && advancing) Laps++;
                SectionIdx = advancing ? passedCp : passedCp - 1;
            }
            else
            {
                if(SectionIdx == -1)
                {
                    SectionIdx = advancing ? 0 : -2;
                }
                else if(SectionIdx == -cpManager.CpPoses.Count) // 逆周りで始点を抜ける
                {
                    if(!advancing) Laps--;
                    SectionIdx = advancing ? -cpManager.CpPoses.Count : -1;
                }
                else
                {
                    SectionIdx = advancing ? passedCp - cpManager.CpPoses.Count : passedCp - cpManager.CpPoses.Count - 1;
                }
            }

            passedCpPos = cpManager.CpPoses.GetWithLoopedIndex(advancing ? passedCp : passedCp - 1);
            headingCpPos = cpManager.CpPoses.GetWithLoopedIndex(advancing ? passedCp + 1 : passedCp);
        }

        void Update()
        {
            velocity = transform.position - prevPos;
            prevPos = transform.position;

            var passedToHeading = headingCpPos - passedCpPos;
            var passedToPlayer = transform.position - passedCpPos;

            float sectionDistance = default;
            if(SectionIdx >= 0)
            {
                for(int i = 0;i < SectionIdx;i++)
                {
                    sectionDistance += cpManager.Distances[i];
                }
            }
            else
            {
                for(int i = -1; i >= SectionIdx; i--)
                {
                    sectionDistance -= cpManager.Distances.GetWithLoopedIndex(i);
                }
            }

            Progress =
            Laps * cpManager.Distances.Sum()
            + sectionDistance
            + Mathf.Clamp(
                Vector3.Dot(passedToHeading, passedToPlayer) / passedToHeading.magnitude,
                0,
                passedToHeading.magnitude
                ); // セクション内の進捗
        }
    }
}
