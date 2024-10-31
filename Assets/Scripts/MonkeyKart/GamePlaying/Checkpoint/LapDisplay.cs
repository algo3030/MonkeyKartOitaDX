using TMPro;
using UnityEngine;
using UniRx;

namespace MonkeyKart.GamePlaying.Checkpoint
{
    public class LapDisplay : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI lapText;

        public void Init(PlayerProgress myProgress)
        {
            myProgress.Laps.Subscribe(lap =>
            {
                lapText.text = $"{lap + 1}/2";
            }).AddTo(this);
        }
    }
}
