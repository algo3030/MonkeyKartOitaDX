using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonkeyKart.Common.UI;
using MonkeyKart.SceneManagement;
using VContainer;
using MonkeyKart.Common;
using UniRx;
using MonkeyKart.Networking.ConnectionManagement;
using Cysharp.Threading.Tasks;
using MonkeyKart.Common.UI.Button;

public class TitleUIMediator : MonoBehaviour
{
    [Inject] ConnectionManager connectionManager;
    [Inject] DialogSpawner dialogSpawner;

    [SerializeField] SimpleButton tapAreaBtn;
    bool connecting = false;

    void Start()
    {
        tapAreaBtn.OnClick.Subscribe(async _ =>
        {
            if (connecting) return;
            connecting = true;
            (await connectionManager.Initialize())
            .OnFailure(_ =>
            {
                connecting = false;
                dialogSpawner.SpawnAlertDialog(
                new DialogOptions()
                .SetTitle(new MessageTitle(message: "接続失敗", bgColor: Colors.Scarlet))
                .SetBody(new MessageBody(message: "ログインに失敗しました。"))
                .SetPadding(DialogPaddings.Wide)
                );
            });
        }).AddTo(this);
    }
}