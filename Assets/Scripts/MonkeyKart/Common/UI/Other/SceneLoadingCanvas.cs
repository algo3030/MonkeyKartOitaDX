using MonkeyKart.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace MonkeyKart.Common.UI
{
    public class SceneLoadingCanvas : MonoBehaviour
    {
        const string TAG = "SceneLoadingCanvas";

        [SerializeField] CanvasGroup canvasGroup;
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Show()
        {
            Log.d(TAG, "Showed.");
            gameObject.SetActive(true);
            canvasGroup.alpha = 1.0f;
        }

        public async void Hide()
        {
            Log.d(TAG, "Hided.");
            await canvasGroup.DOFade(0f, 0.7f).SetEase(Ease.OutExpo).AsyncWaitForCompletion();
            gameObject.SetActive(false);
        }
    }

}