using MonkeyKart.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Threading.Tasks;
using DG.Tweening;
using MonkeyKart.Common.UI.Button;
using UnityEngine.UIElements;
using VContainer;

namespace MonkeyKart.Common.UI
{
    /// <summary>
    /// シンプルなダイアログを、DialogOptionsから生成し、表示する。
    /// </summary>
    public class AlertDialog : MonoBehaviour
    {
        /*
         * NOTE: 
         * AddComponentは遅いのでPrefabに全て追加し、編集するという方針にした。
         * ボタンくらいは生成してもいいかもしれない。
        */

        [Inject] DialogSpawner owner;
        [SerializeField] Sprite warnSprite;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] GameObject titleHolder;
        [SerializeField] TextMeshProUGUI bodyTxt;
        [SerializeField] TextMeshProUGUI titleTxt;
        [SerializeField] UnityEngine.UI.Image titleImg;
        [SerializeField] SimpleButton okButton;

        public void Init(DialogOptions options)
        {
            var padding = options.Padding;
            // タイトルがあるなら、上に追加の余白を追加
            if (options.Title is not NoneTitle) padding += Vector2.up * 50;
            // 余白を適用
            var rect = GetComponent<RectTransform>();
            rect.offsetMin = new Vector2 (padding.x, padding.y);
            rect.offsetMax = new Vector2(-padding.x, -padding.y);

            ProvideActiveState(options);
            switch (options.Title)
            {
                case NoneTitle:
                    break;
                case MessageTitle message:
                    titleTxt.text = message.Message;
                    break;
                case IconTitle icon:
                    titleImg.sprite = icon.Icon switch
                    {
                        IconTitle.IconType.Warn => warnSprite,
                        _ => throw new InvalidOperationException()
                    };
                    break;
                default:
                    throw new InvalidOperationException("Title is invalid.");
            }

            switch (options.Body)
            {
                case MessageBody body:
                    bodyTxt.text = body.Message;
                    break;
                default:
                    throw new InvalidOperationException("Body is invalid.");
            }

            okButton.OnClick.Subscribe(_ =>
            {
                owner.CloseDialog(gameObject);
            });
        }

        private void ProvideActiveState(DialogOptions options)
        {
            switch (options.Title)
            {
                case NoneTitle:
                    titleHolder.gameObject.SetActive(false);
                    break;
                case MessageTitle message:
                    titleImg.gameObject.SetActive(false);
                    break;
                case IconTitle icon:
                    titleTxt.gameObject.SetActive(false);
                    break;
                default:
                    throw new InvalidOperationException("Title is invalid.");
            }
        }
    }
}