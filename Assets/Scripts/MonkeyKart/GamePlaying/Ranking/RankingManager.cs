using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using System.Linq;

namespace MonkeyKart.GamePlaying.Ranking
{
    public class RankingManager : MonoBehaviour
    {
        ReactiveProperty<int> currentRank = new(-1);
        public IReadOnlyReactiveProperty<int> CurrentRank => currentRank;
        PlayerProgress localPlayerProgress;

        List<PlayerProgress> progresses = new();

        public void AddPlayer(PlayerProgress progress, bool isLocalPlayer)
        {
            if (isLocalPlayer)
            {
                localPlayerProgress = progress;
                return;
            }
            progresses.Add(progress);
        }

        void FixedUpdate()
        {
            var rank = 1;
            progresses.ForEach(p =>
            {
                if (p.Progress >= localPlayerProgress.Progress) rank++;
            });
            if (currentRank.Value != rank) currentRank.Value = rank;
        }
    }
}
