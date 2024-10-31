using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MonkeyKart.Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonkeyKart.GamePlaying.UI
{
    public class GameEndingUI : MonoBehaviour
    {
        [SerializeField] Image finishImg;
        [SerializeField] CanvasGroup resultPanel;
        
        [SerializeField] List<TextMeshProUGUI> playerNames; // 順位順
        [SerializeField] List<Image> holders;

        void OnDestroy()
        {
            DOTween.Kill(transform);
        }

        public async void ShowFinishUI()
        {
            finishImg.enabled = true;
            finishImg.transform.localScale = Vector3.one * 1.7f;
            finishImg.transform.DOScale(1.0f, 1.0f).SetEase(Ease.OutElastic);
            await UniTask.WaitForSeconds(1f);
            finishImg.DOFade(0f, 0.3f);
        }

        public void ShowResultUI(IReadOnlyList<string> names, ulong localPlayerIdx) // 順位順
        {
            for (int i = 0; i < names.Count; i++)
            {
                if (i == (int)localPlayerIdx) holders[i].color = Colors.Scarlet;
                playerNames[i].text = names[i];
            }
            
            resultPanel.DOFade(1, 0.3f);
        }
    }
}
