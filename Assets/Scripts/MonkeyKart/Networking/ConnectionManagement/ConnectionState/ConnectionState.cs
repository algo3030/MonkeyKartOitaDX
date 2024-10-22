using Unity.Netcode;


namespace MonkeyKart.Networking.ConnectionManagement
{
    public abstract class ConnectionState
    {
        protected ConnectionState(ConnectionManager owner)
        {
            this.owner = owner;
        }

        protected ConnectionManager owner;

        public virtual bool CanTransitionTo(ConnectionState next)
        {
            return true;
        }

        public abstract void Enter();
        public abstract void Exit();

        public virtual void OnClientConnected(ulong clientId) { }
        public virtual void OnClientDisconnected(ulong clientId) { }
        public virtual void OnServerStarted() { }
        public virtual void OnServerStopped() { }
        public virtual void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) { }
    }
}