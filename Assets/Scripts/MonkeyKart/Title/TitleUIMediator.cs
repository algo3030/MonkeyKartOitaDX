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

    void Start()
    {
        tapAreaBtn.OnClick.Subscribe(async _ =>
        {
            (await connectionManager.Initialize())
            .OnFailure(_ =>
            {
                dialogSpawner.SpawnAlertDialog(
                new DialogOptions()
                .SetTitle(new MessageTitle(message: "�ڑ����s", bgColor: Colors.Scarlet))
                .SetBody(new MessageBody(message: "�F�؂Ɏ��s���܂����B"))
                .SetPadding(DialogPaddings.Wide)
                );
            });
        }).AddTo(this);
    }
}