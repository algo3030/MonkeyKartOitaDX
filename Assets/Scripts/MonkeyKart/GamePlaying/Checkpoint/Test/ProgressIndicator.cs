using System.Collections;
using System.Collections.Generic;
using MonkeyKart.GamePlaying;
using TMPro;
using UnityEngine;

public class ProgressIndicator : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] PlayerProgress progress;

    void Update()
    {
        text.text = $"Progress:{progress.Progress}\nCurrentSection:{progress.SectionIdx}\nLaps:{progress.Laps}";
    }
}
