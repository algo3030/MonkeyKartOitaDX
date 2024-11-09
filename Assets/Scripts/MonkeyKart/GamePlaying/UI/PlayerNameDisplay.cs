using TMPro;
using UnityEngine;

namespace MonkeyKart.GamePlaying.UI
{
    public class PlayerNameDisplay : MonoBehaviour
    {
        RectTransform myRectTfm;
        [SerializeField]TextMeshProUGUI text;
        [SerializeField] Vector3 offset = new Vector3(0, 2.3f, 0);
        Transform targetTfm;
    
        void Awake()
        {
            enabled = false;
        }

        public void Init(Transform targetTfm, ulong clientId, string playerName)
        {
            this.targetTfm = targetTfm;
            text.text = $"P{clientId + 1} " +playerName;
        }
 
        void Start() {
            myRectTfm = GetComponent<RectTransform>();
        }
 
        void Update() {
            myRectTfm.position 
                = RectTransformUtility.WorldToScreenPoint(Camera.main, targetTfm.position + offset);
        }
    }
}
