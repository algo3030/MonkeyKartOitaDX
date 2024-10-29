using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MonkeyKart.GamePlaying.Checkpoint
{
    public class CheckpointManager : MonoBehaviour
    {
        List<Vector3> cpPoses = new();
        public List<Vector3> CpPoses => cpPoses;
        List<float> distances = new();
        public List<float> Distances => distances;

        void Awake()
        {
            for(int i = 0;i < transform.childCount;i++)
            {
                var obj = transform.GetChild(i);
                obj.GetComponent<Checkpoint>().Init(i);
                cpPoses.Add(obj.position);
            }

            for(int i = 0;i < cpPoses.Count; i++)
            {
                distances.Add(Vector3.Distance(cpPoses[i],cpPoses[i == cpPoses.Count - 1 ? 0 : i + 1]));
            }
        }
    }
}
