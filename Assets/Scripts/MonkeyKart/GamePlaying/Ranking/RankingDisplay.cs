using System.Collections.Generic;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MonkeyKart.GamePlaying.Ranking
{
    public class RankingDisplay : MonoBehaviour
    {
        [SerializeField] RankingManager rankingManager;
        [SerializeField] List<Sprite> rankingSprites;
        [SerializeField] Image rankingImage;
        [SerializeField] AudioSource rankAudio;
        [SerializeField] AudioClip updateRankingSE;

        void Start()
        {
            rankingManager.CurrentRank.Subscribe(rank =>
            {
                if (rank == -1) return;
                rankingImage.sprite = rankingSprites[rank - 1];
                transform.localScale = Vector3.zero;
                transform.localRotation = Quaternion.Euler(0,0,-90);
                transform.DOScale(1.0f, 0.2f).SetEase(Ease.OutExpo);
                transform.DORotate(new Vector3(0, 0, 0), 0.2f).SetEase(Ease.OutExpo);
                rankAudio.PlayOneShot(updateRankingSE);
            }).AddTo(this);
        }
    }
}
