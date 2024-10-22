using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using MonkeyKart.Common;

namespace MonkeyKart.Home
{
    public class ProfileManager
    {
        const string DEFAULT_PLAYER_NAME = "名無しのモンキー";
        const int MAX_NAME_LENGTH = 12;
        ReactiveProperty<string> playerName = new(DEFAULT_PLAYER_NAME);
        public IReadOnlyReactiveProperty<string> PlayerName => playerName;


        public Result<Unit, string> ChangeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "名前が空白です";
            if (name.Length >= MAX_NAME_LENGTH) return $"名前は{MAX_NAME_LENGTH}文字までです";
            playerName.Value = name;
            return Unit.Default;
        }
    }
}