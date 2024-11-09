using System.Collections;
using System.Collections.Generic;
using MonkeyKart.GamePlaying;
using TMPro;
using UnityEngine;

public class ProgressIndicator : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    PlayerProgress progress;

    public void Init(PlayerProgress progress)
    {
        this.progress = progress;
    }

    void Update()
    {
        if(!progress) return;
        text.text = $"Progress:{progress.Progress}\nCurrentSection:{progress.SectionIdx}\nLaps:{progress.Laps}";
    }
}
