using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using System.Linq;

namespace MonkeyKart.GamePlaying.Ranking
{
    public class RankingManager : MonoBehaviour
    {
        Subject<int> onRankingUpdated = new();
        public IObservable<int> OnRankingUpdated => onRankingUpdated;
        
        List<PlayerProgress> progresses = new(); // index:0に自身のプログレス

        public void Init(IReadOnlyList<PlayerProgress> progresses)
        {
            this.progresses.AddRange(progresses);
            
            
        }

        void FixedUpdate()
        {
            progresses.Sort();
        }
    }
}
