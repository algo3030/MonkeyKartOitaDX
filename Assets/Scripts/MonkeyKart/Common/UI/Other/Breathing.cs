using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MonkeyKart.Common.UI
{
    public class Breathing : MonoBehaviour
    {
        [SerializeField] float period;
        Image image;
        Color color;

        private void Awake()
        {
            image = GetComponent<Image>();
            color = image.color;
        }

        // Update is called once per frame
        void Update()
        {
            image.color = new Color(r: color.r, g: color.g, b: color.b, a: Mathf.Cos(Time.time / period * (2 * Mathf.PI)));
        }
    }

}