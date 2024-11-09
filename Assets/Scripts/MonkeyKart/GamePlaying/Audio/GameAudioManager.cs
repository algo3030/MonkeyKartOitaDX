using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameAudioManager : MonoBehaviour
{
    [SerializeField] AudioSource SEChannel;
    static float MaxBGMVolume;
    [SerializeField] AudioSource BGMChannel;

    [SerializeField] AudioClip countDownSE; public AudioClip CountDownSE => countDownSE;
    [SerializeField] AudioClip lapSE; public AudioClip LapSE => lapSE;
    [SerializeField] AudioClip goalSE; public AudioClip GoalSE => goalSE;
    [SerializeField] AudioClip breakItemSE; public AudioClip BreakItemSE => breakItemSE;
    [SerializeField] AudioClip getItemSE; public AudioClip GetItemSE => getItemSE;
    [SerializeField] AudioClip useItemSE; public AudioClip UseItemSE => useItemSE;

    [SerializeField] AudioClip courseIntroBGM; public AudioClip CourceIntroBGM => courseIntroBGM;
    [SerializeField] AudioClip raceBGM; public AudioClip RaceBGM => raceBGM;
    [SerializeField] AudioClip postGoalBGM; public AudioClip PostGoalBGM => postGoalBGM;


    void Awake()
    {
        MaxBGMVolume = BGMChannel.volume;
    }

    public async void MakeSE(AudioClip se, int delayMs = 0) 
    {
        await UniTask.Delay(delayMs);
        SEChannel.PlayOneShot(se);
    }

    public async void SetBGM(AudioClip bgm,float pitch = 1.0f, int delayMs = 0)
    {
        await UniTask.Delay(delayMs);
        BGMChannel.pitch = pitch;
        BGMChannel.volume = MaxBGMVolume;
        BGMChannel.clip = bgm;
        BGMChannel.Play();
    }

    public void PlayBGM(AudioClip bgm) => BGMChannel.PlayOneShot(bgm);

    public void SetBGMVolume(float volume)
    {
        BGMChannel.volume = MaxBGMVolume * volume;
    }

    public async UniTask SetBGMVolume(float volume, float duration)
    {
        float initialVolume = BGMChannel.volume;
        var targetVolume = MaxBGMVolume * volume;

        var elapsed = 0f;
        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float currentValue = Mathf.Lerp(initialVolume, targetVolume, t);
            BGMChannel.volume = currentValue;
            await UniTask.Yield();
        }
    }
}
