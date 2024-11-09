using TMPro;
using UnityEngine;
using UniRx;
using DG.Tweening;
using Cysharp.Threading.Tasks;

namespace MonkeyKart.GamePlaying.Checkpoint
{
    public class LapUI : MonoBehaviour
    {
        [SerializeField] GameAudioManager gameAudio;
        [SerializeField] TextMeshProUGUI lapText;
        [SerializeField] CanvasGroup oneMoreCG;
        [SerializeField] RectTransform oneMoreTxtTfm;

        public void Init(PlayerProgress myProgress)
        {
            oneMoreCG.alpha = 0f;
            myProgress.Laps.Subscribe(OnLapChanged).AddTo(this);
        }

        void OnLapChanged(int lap)
        {
            if(lap == 0 || lap >= 2) return;
            gameAudio.MakeSE(gameAudio.LapSE);
            if(lap == 1) ShowOneMore();
            lapText.text = $"{lap + 1}/2";
        }

        async void ShowOneMore()
        {
            gameAudio.SetBGMVolume(0f);
            gameAudio.MakeSE(gameAudio.LapSE);
            oneMoreTxtTfm.DOLocalMoveX(0, 0.7f);
            oneMoreCG.DOFade(1, 0.7f);
            await UniTask.Delay(1500);
            oneMoreCG.DOFade(0, 0.7f);
            oneMoreTxtTfm.DOLocalMoveX(-1500, 0.7f);
            await UniTask.Delay(700);
            gameAudio.SetBGM(gameAudio.RaceBGM, pitch: 1.2f);
            gameAudio.SetBGMVolume(1f, 1f).Forget();
        }
    }
}
