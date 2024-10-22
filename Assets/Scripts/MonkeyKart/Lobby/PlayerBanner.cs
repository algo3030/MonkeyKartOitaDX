using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MonkeyKart.LobbyScene
{
    class PlayerBanner : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI playerDisplayNameTxt;
        [SerializeField] GameObject contents;
        [SerializeField] Image bg;

        public void Enable(bool ownedByLocalClient, LobbyPlayerState state)
        {
            if (ownedByLocalClient)
            {
                bg.color = Color.yellow;
            }
            playerDisplayNameTxt.text = state.PlayerName;
            contents.SetActive(true);
        }

        public void Disable()
        {
            playerDisplayNameTxt.text = string.Empty;
            contents.SetActive(false);
        }
    }
}