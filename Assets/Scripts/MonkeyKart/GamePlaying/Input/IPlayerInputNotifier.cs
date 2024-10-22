using System;
using UniRx;
using UnityEngine;

namespace MonkeyKart.GamePlaying.Input
{
    /// <summary>
    /// プレイヤーの入力を抽象化する。これによって、入力を受ける側がローカル/ネットを気にせず利用できる。
    /// </summary>
    public interface IPlayerInputNotifier
    {
        public Vector2 InputVector { get; }
        public IReadOnlyReactiveProperty<bool> IsDrifting { get; }
    }
}