using MonkeyKart.Common;
using MonkeyKart.Common.UI;
using MonkeyKart.Networking.ConnectionManagement;
using UnityEngine;
using UniRx;
using VContainer;
using System;

namespace MonkeyKart.Home.UI
{
    /// <summary>
    /// DisconnectEventを受信し、UI表示する
    /// </summary>
    class HomeDisconnectEventPresenter : MonoBehaviour
    {
        const string TAG = "HomeDisconnectEventPresenter";

        [Inject] DialogSpawner dialogManager;
        [Inject] ConnectionManager connectionManager;

        void Start()
        {
            connectionManager.CurrentState.Subscribe(state =>
            {
                if (state is not OfflineState offlineState) return;
                string message = offlineState.Reason switch
                {
                    DisconnectReason.Init => null,
                    DisconnectReason.ShutdownByMe => null,
                    DisconnectReason.Generic => "エラーが発生しました。",
                    DisconnectReason.ServerFull => "ロビーが満員です。",
                    DisconnectReason.LobbyCreationFailed => "ロビー作成に失敗しました。",
                    DisconnectReason.HostEndedSession => "ホストがセッションを終了しました。",
                    DisconnectReason.LobbyNotFound => "ロビーが見つかりませんでした。",
                    DisconnectReason.LobbyIsLocked => "ゲームが進行中なので、現在このロビーに入ることができません。",
                    DisconnectReason.BuildIncompatible => "ビルドタイプに互換性がありません。",
                    DisconnectReason.Kicked => "ロビーからキックされました。",
                    _ => throw new InvalidOperationException()
                } ;
                if (message == null) return;

                Log.d(TAG, "Spawning disconnect dialog.");
                dialogManager.SpawnAlertDialog(
                    new DialogOptions()
                        .SetTitle(new MessageTitle(message: "エラー", bgColor: Colors.Scarlet))
                        .SetBody(new MessageBody(message: message))
                        .SetPadding(DialogPaddings.Wide)
                );
            }).AddTo(this);
        }
    }
}