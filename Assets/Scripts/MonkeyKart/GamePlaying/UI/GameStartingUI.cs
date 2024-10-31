using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MonkeyKart.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonkeyKart.GamePlaying.UI
{
    public class GameStartingUI : MonoBehaviour
    {
        [SerializeField] Sprite sprite3;
        [SerializeField] Sprite sprite2;
        [SerializeField] Sprite sprite1;

        [SerializeField] Image countDownImg;
        [SerializeField] Image goImg;
        [SerializeField] CanvasGroup panelGroup;

        void OnDestroy()
        {
            DOTween.Kill(transform);
        }

        public void ShowCourseUI()
        {
            panelGroup.alpha = 0f;
            panelGroup.DOFade(1f, 0.5f).SetEase(Ease.OutCubic);
        }

        public async UniTask HideCourseUI()
        {
            await panelGroup.DOFade(0f, 0.5f).SetEase(Ease.OutCubic).AsyncWaitForCompletion();
        }

        public async UniTask CountDown()
        {
            countDownImg.enabled = true;
            countDownImg.sprite = sprite3;
            countDownImg.transform.localScale = Vector3.one * 1.5f;
            await countDownImg.transform.DOScale(1.0f, 1.0f).SetEase(Ease.OutElastic).AsyncWaitForCompletion();
            
            countDownImg.sprite = sprite2;
            
            countDownImg.transform.localScale = Vector3.one * 1.5f;
            await countDownImg.transform.DOScale(1.0f, 1.0f).SetEase(Ease.OutElastic).AsyncWaitForCompletion();
            
            countDownImg.sprite = sprite1;
            
            countDownImg.transform.localScale = Vector3.one * 1.5f;
            await countDownImg.transform.DOScale(1.0f, 1.0f).SetEase(Ease.OutElastic).AsyncWaitForCompletion();
            ShowGoImg().Forget();
        }

        async UniTask ShowGoImg()
        {
            countDownImg.gameObject.SetActive(false);
            goImg.gameObject.SetActive(true);
            goImg.transform.localScale = Vector3.one * 1.7f;
            goImg.transform.DOScale(1.0f, 1.0f).SetEase(Ease.OutElastic);
            await UniTask.WaitForSeconds(1f);
            goImg.DOFade(0f, 0.3f);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
