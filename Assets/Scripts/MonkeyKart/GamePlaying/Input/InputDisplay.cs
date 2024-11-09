using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInput = MonkeyKart.GamePlaying.Input.PlayerInput;

public class InputDisplay : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] TextMeshProUGUI txt;

    // Update is called once per frame
    void Update()
    {
        txt.text = $"input: {playerInput.InputVector}";
    }
}
