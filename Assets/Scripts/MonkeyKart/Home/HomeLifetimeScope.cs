using VContainer;
using VContainer.Unity;
using UnityEngine;
using MonkeyKart.Common.UI;
using MonkeyKart.Home.UI;

namespace MonkeyKart.Home
{
    public class HomeLifetimeScope : LifetimeScope
    {
        [SerializeField] DialogSpawner dialogManager;
        [SerializeField] HomeDisconnectEventPresenter connectionEventPresenter;
        [SerializeField] LoadingCanvas loadingCanvas;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(dialogManager);
            builder.RegisterComponent(connectionEventPresenter);
            builder.RegisterComponent(loadingCanvas);
        }
    }
}