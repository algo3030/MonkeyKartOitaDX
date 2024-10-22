using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MonkeyKart.Common.UI.Button;
using MonkeyKart.GamePlaying.Input;
using UnityEngine;
using UniRx;

namespace MonkeyKart.GamePlaying.UI
{
    public class DriftButtonAnim : MonoBehaviour
    {
        [SerializeField] PlayerInput input;
        [SerializeField] CanvasGroup canvasGroup;
        
        [SerializeField][Range(0f, 1f)] float duration = 0.24f;
        [SerializeField][Range(0f, 1f)] float fade = 0.8f;
        [SerializeField][Range(0f,1f)] float scale = 0.95f;

        void Start()
        {
            input.IsDrifting.Subscribe(isDrifting =>
            {
                if (isDrifting)
                {
                    transform.DOScale(scale, duration).SetEase(Ease.OutCubic);
                    canvasGroup.DOFade(fade, duration).SetEase(Ease.OutCubic);
                }
                else
                {
                    transform.DOScale(1f, duration).SetEase(Ease.OutCubic);
                    canvasGroup.DOFade(1f, duration).SetEase(Ease.OutCubic);
                }
            }).AddTo(this);
        }
        
        void OnDestroy()
        {
            DOTween.Kill(transform);
        }
    }
}
