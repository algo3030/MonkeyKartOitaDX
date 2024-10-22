namespace MonkeyKart.Networking.ConnectionManagement
{
    public abstract class OnlineState: ConnectionState
    {
        protected OnlineState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        public abstract void OnUserRequestedShutDown();
        public void OnTransportFailure()
        {
            owner.ChangeState(new OfflineState(owner, DisconnectReason.Generic));
        }
    }
}