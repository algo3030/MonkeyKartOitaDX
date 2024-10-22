namespace MonkeyKart.Networking.ConnectionManagement
{
    public enum DisconnectReason
    {
        Init, // 最初のOfflineStateに渡す
        Generic, // その他

        // クライアントの接続中断
        ServerFull,
        LobbyNotFound,
        LobbyIsLocked,
        BuildIncompatible,

        // ホストの初期化
        LobbyCreationFailed,


        // 接続後
        HostEndedSession,
        ShutdownByMe, // 自らセッションを切断した場合
        Kicked
    }
}