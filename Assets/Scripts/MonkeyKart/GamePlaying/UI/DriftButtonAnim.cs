using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MonkeyKart.Common.UI.Button;
using MonkeyKart.GamePlaying.Input;
using UnityEngine;
using UniRx;

namespace MonkeyKart.GamePlaying.UI
{
    public class DriftButtonAnim : MonoBehaviour
    {
        [SerializeField] CanvasGroup canvasGroup;
        
        [SerializeField][Range(0f, 1f)] float duration = 0.24f;
        [SerializeField][Range(0f, 1f)] float fade = 0.8f;
        [SerializeField][Range(0f,1f)] float scale = 0.95f;
        
        
        void OnDestroy()
        {
            DOTween.Kill(transform);
        }
    }
}
