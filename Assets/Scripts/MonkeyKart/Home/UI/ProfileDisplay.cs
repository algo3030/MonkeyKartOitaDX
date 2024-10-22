using MonkeyKart.Common;
using MonkeyKart.Home;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UniRx;
using VContainer;

public class ProfileDisplay : MonoBehaviour
{
    [Inject] ProfileManager profileManager;

    [SerializeField] TextMeshProUGUI nameTxt;

    private void Start()
    {
        profileManager.PlayerName.Subscribe(name =>
        {
            nameTxt.text = name;
        });
    }
}
