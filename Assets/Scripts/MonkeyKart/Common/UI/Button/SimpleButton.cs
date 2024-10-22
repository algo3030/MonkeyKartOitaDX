using System;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MonkeyKart.Common.UI.Button
{
    public class SimpleButton : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        Subject<Unit> onClick = new();
        public IObservable<Unit> OnClick => onClick;
        Subject<Unit> onClickDown = new();
        public IObservable<Unit> OnClickDown => onClickDown;
        Subject<Unit> onClickUp = new();
        public IObservable<Unit> OnClickUp => onClickUp;

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick.OnNext(Unit.Default);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
           onClickDown.OnNext(Unit.Default);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            onClickUp.OnNext(Unit.Default);
        }
    }
}