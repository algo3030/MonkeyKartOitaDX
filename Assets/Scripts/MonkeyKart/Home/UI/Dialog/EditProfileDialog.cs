using MonkeyKart.Common.UI;
using TMPro;
using UnityEngine;
using UniRx;
using MonkeyKart.Common;
using MonkeyKart.Common.UI.Button;
using VContainer;

namespace MonkeyKart.Home.UI
{
    public class EditProfileDialog : MonoBehaviour
    {
        const string TAG = "EditProfileDialog";

        [Inject] ProfileManager profileManager;
        [Inject] DialogSpawner owner;

        [SerializeField] SimpleButton cancelBtn;
        [SerializeField] SimpleButton editBtn;
        [SerializeField] TextMeshProUGUI errTxt;
        [SerializeField] TMP_InputField inputField;

        void Start()
        {
            errTxt.text = string.Empty; 
            inputField.text = profileManager.PlayerName.Value;

            cancelBtn.OnClick.Subscribe(_ =>
            {
                owner.CloseDialog(gameObject);
            });

            editBtn.OnClick.Subscribe(_ =>
            {
                profileManager.ChangeName(inputField.text)
                .OnFailure(err =>
                {
                    errTxt.text = err;
                })
                .OnSuccess(_ =>
                {
                    owner.CloseDialog(gameObject);
                });
            });
        }
    }
}
