using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MonkeyKart.Common.UI.Button
{
    class NormalButtonAnim: MonoBehaviour
    {
        SimpleButton button;
        CanvasGroup canvasGroup;
        
        [SerializeField][Range(0f, 1f)] float duration = 0.24f;
        [SerializeField][Range(0f, 1f)] float fade = 0.8f;
        [SerializeField][Range(0f,1f)] float scale = 0.95f;

        void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            button = GetComponent<SimpleButton>();

            button.OnClickDown.Subscribe(_ =>
            {
                transform.DOScale(scale, duration).SetEase(Ease.OutCubic);
                canvasGroup.DOFade(fade, duration).SetEase(Ease.OutCubic);
            }).AddTo(this);

            button.OnClickUp.Subscribe(_ =>
            {
                transform.DOScale(1f, duration).SetEase(Ease.OutCubic);
                canvasGroup.DOFade(1f, duration).SetEase(Ease.OutCubic);
            }).AddTo(this);
        }

        void OnDestroy()
        {
            DOTween.Kill(transform);
        }
    }
}