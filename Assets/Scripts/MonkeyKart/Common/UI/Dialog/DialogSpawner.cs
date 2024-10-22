using DG.Tweening;
using MonkeyKart.Common;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using Cysharp.Threading;
using Cysharp.Threading.Tasks;

namespace MonkeyKart.Common.UI
{
    /// <summary>
    /// ダイアログ生成の窓口となるクラス。
    /// ダイアログのバックグラウンド画像を表示する。
    /// </summary>
    public class DialogSpawner : MonoBehaviour
    {
        [Inject] IObjectResolver container;

        [SerializeField] GameObject alertDialogPfb;
        [SerializeField] Image dialogBgImg;
        [SerializeField] GameObject dialogCanvas;
        List<GameObject> showingDialogs = new();

        const float ANIM_DURATION = 0.4f;

        public void SpawnAlertDialog(DialogOptions options)
        {
            var ins = SpawnDialog(alertDialogPfb);
            var dialog = ins.GetComponent<AlertDialog>();
            dialog.Init(options);
        }

        public GameObject SpawnDialog(GameObject dialog)
        {
            var ins = container.Instantiate(dialog, dialogCanvas.transform);
            showingDialogs.Add(ins);

            // BG表示
            var canvasGroup = ins.GetComponent<CanvasGroup>();
            ins.transform.localScale = new Vector3(0.9f, 0.9f);
            ins.transform.DOScale(1.0f, ANIM_DURATION).SetEase(Ease.OutExpo);
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1.0f, ANIM_DURATION).SetEase(Ease.OutExpo);
            dialogBgImg.DOFade(0.7f, ANIM_DURATION).SetEase(Ease.OutExpo);
            dialogBgImg.gameObject.SetActive(true);
            return ins;
        }

        public async void CloseDialog(GameObject obj)
        {
            var canvasGroup = obj.GetComponent<CanvasGroup>();
            var scale = obj.transform.DOScale(0.9f, ANIM_DURATION).SetEase(Ease.OutExpo).AsyncWaitForCompletion();
            var fade = canvasGroup.DOFade(0f, ANIM_DURATION).SetEase(Ease.OutExpo).AsyncWaitForCompletion();
            await Task.WhenAll(scale, fade);
            showingDialogs.Remove(obj);
            Destroy(obj);

            if (showingDialogs.Count > 0) return;
            // ダイアログが全部閉じたなら、BGを消す
            await dialogBgImg.DOFade(0f, ANIM_DURATION).SetEase(Ease.OutExpo).AsyncWaitForCompletion();
            dialogBgImg.gameObject.SetActive(false);
        }
    }
}