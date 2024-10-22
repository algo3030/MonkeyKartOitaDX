using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace MonkeyKart.Common.UI
{
    public class LoadingMonkey : MonoBehaviour
    {
        Vector2 initialPos;
        Quaternion initialRot;

        private void Awake()
        {
            initialPos = transform.position;
            initialRot = transform.rotation;
        }

        void OnEnable()
        {
            transform.position = initialPos;
            transform.rotation = initialRot;

            transform.DOShakePosition(1f, 30f, 1, 100, true, false).SetLoops(-1, LoopType.Yoyo);
            transform.DOLocalRotate(new Vector3(0, 0, 360f), 2f, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutCirc)
                .SetLoops(-1, LoopType.Restart);
        }

        private void OnDisable()
        {
            DOTween.Kill(transform);
        }
    }
}