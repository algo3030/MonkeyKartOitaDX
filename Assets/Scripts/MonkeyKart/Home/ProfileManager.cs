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
        const string DEFAULT_PLAYER_NAME = "�������̃����L�[";
        const int MAX_NAME_LENGTH = 12;
        ReactiveProperty<string> playerName = new(DEFAULT_PLAYER_NAME);
        public IReadOnlyReactiveProperty<string> PlayerName => playerName;


        public Result<Unit, string> ChangeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "���O���󔒂ł�";
            if (name.Length >= MAX_NAME_LENGTH) return $"���O��{MAX_NAME_LENGTH}�����܂łł�";
            playerName.Value = name;
            return Unit.Default;
        }
    }
}