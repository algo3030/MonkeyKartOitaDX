using VContainer;
using VContainer.Unity;
using UnityEngine;
using MonkeyKart.Common.UI;

namespace MonkeyKart.TitleScene
{
    public class TitleLifetimeScope : LifetimeScope
    {
        [SerializeField] TitleUIMediator titleUIMediator;
        [SerializeField] DialogSpawner dialogSpawner;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.RegisterComponent(titleUIMediator);
            builder.RegisterComponent(dialogSpawner);
        }
    }
}