using MonkeyKart.Common.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MonkeyKart.LobbyScene
{
    public class LobbyLifetimeScope : LifetimeScope
    {
        [SerializeField] DialogSpawner dialogManager;
        [SerializeField] ClientLobbyUI lobbyUIMediator;
        [SerializeField] ServerLobbyManager serverLobbyManager;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.RegisterComponent(dialogManager);
            builder.RegisterComponent(lobbyUIMediator);
            builder.RegisterComponent(serverLobbyManager);
        }
    }
}