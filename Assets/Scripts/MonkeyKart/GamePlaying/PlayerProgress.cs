using System.Linq;
using MonkeyKart.GamePlaying.Checkpoint;
using UniRx;
using UnityEngine;

namespace MonkeyKart.GamePlaying
{
    public class PlayerProgress : MonoBehaviour
    {
        CheckpointManager cpManager;

        readonly ReactiveProperty<int> laps = new();
        public IReadOnlyReactiveProperty<int> Laps => laps;
        public float Progress {get; private set;}
        public int SectionIdx {get; private set;}
        Vector3 passedCpPos;
        Vector3 headingCpPos;

        Vector3 velocity;
        Vector3 prevPos;

        public void Init(CheckpointManager cpManager)
        {
            this.cpManager = cpManager;
            SectionIdx = 0;
            passedCpPos = cpManager.CpPoses[0];
            headingCpPos = cpManager.CpPoses[1];
        }
        
        int GetLoopedDistance(int index1, int index2, int arrayLength)
        {
            int directDistance = Mathf.Abs(index1 - index2);
            int loopingDistance = arrayLength - directDistance;
            return Mathf.Min(directDistance, loopingDistance);
        }

        public void PassCheckpoint(int passedCp)
        {
            if (GetLoopedDistance(passedCp,SectionIdx,cpManager.CpPoses.Count) >= 2) return;
            
            var prevCpPos = cpManager.CpPoses.GetWithLoopedIndex(passedCp - 1);
            var currentCpPos = cpManager.CpPoses[passedCp];
            var nextCpPos = cpManager.CpPoses.GetWithLoopedIndex(passedCp + 1);

            var currentToNext = nextCpPos - currentCpPos;
            var currentToPrev = prevCpPos - currentCpPos;
            var toNextAngle = Vector3.Angle(velocity, currentToNext);
            var toPrevAngle = Vector3.Angle(velocity, currentToPrev);
            var advancing = toNextAngle < toPrevAngle;
            
            switch (SectionIdx)
            {
                case >= 0:
                {
                    if (SectionIdx == cpManager.CpPoses.Count - 1 && advancing) laps.Value++;
                    SectionIdx = advancing ? passedCp : passedCp - 1;
                    break;
                }
                case -1:
                    SectionIdx = advancing ? 0 : -2;
                    break;
                default:
                {
                    if(SectionIdx == -cpManager.CpPoses.Count) // 逆周りで始点を抜ける
                    {
                        if(!advancing) laps.Value--;
                        SectionIdx = advancing ? -cpManager.CpPoses.Count : -1;
                    }
                    else
                    {
                        SectionIdx = advancing ? passedCp - cpManager.CpPoses.Count : passedCp - cpManager.CpPoses.Count - 1;
                    }

                    break;
                }
            }

            passedCpPos = cpManager.CpPoses.GetWithLoopedIndex(advancing ? passedCp : passedCp - 1);
            headingCpPos = cpManager.CpPoses.GetWithLoopedIndex(advancing ? passedCp + 1 : passedCp);
        }

        void FixedUpdate()
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
            Laps.Value * cpManager.Distances.Sum()
            + sectionDistance
            + Vector3.Dot(passedToHeading, passedToPlayer) / passedToHeading.magnitude; // セクション内の進捗
        }
    }
}
