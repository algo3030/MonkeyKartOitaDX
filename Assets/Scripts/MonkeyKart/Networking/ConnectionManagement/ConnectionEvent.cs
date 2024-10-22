namespace MonkeyKart.Networking.ConnectionManagement
{
    public enum DisconnectReason
    {
        Init, // �ŏ���OfflineState�ɓn��
        Generic, // ���̑�

        // �N���C�A���g�̐ڑ����f
        ServerFull,
        LobbyNotFound,
        LobbyIsLocked,
        BuildIncompatible,

        // �z�X�g�̏�����
        LobbyCreationFailed,


        // �ڑ���
        HostEndedSession,
        ShutdownByMe, // ����Z�b�V������ؒf�����ꍇ
        Kicked
    }
}