using System;
using UnityEngine;

namespace MonkeyKart.GamePlaying.Checkpoint.Test
{
    public class RankingJudgementDI : MonoBehaviour
    {
        [SerializeField] CheckpointManager cpm;
        [SerializeField] PlayerProgress player;

        void Start()
        {
            player.Init(cpm);
        }
    }
}
