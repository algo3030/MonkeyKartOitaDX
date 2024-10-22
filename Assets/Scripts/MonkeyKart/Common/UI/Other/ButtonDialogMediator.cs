using UnityEngine;
using UniRx;
using MonkeyKart.Common;
using MonkeyKart.Common.UI.Button;
using VContainer;

namespace MonkeyKart.Common.UI
{
    public class ButtonDialogMediator : MonoBehaviour
    {
        [Inject] DialogSpawner dialogManager;

        [SerializeField] SimpleButton Button;
        [SerializeField] GameObject dialogPfb;

        private void Start()
        {
            Button.OnClick.Subscribe(_ => {
                dialogManager.SpawnDialog(dialogPfb);
            });
        }
    }
}