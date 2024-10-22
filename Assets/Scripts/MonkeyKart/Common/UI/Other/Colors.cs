using System;
using System.Globalization;
using UnityEngine;

namespace MonkeyKart.Common.UI
{
    public static class Colors
    {
        public static readonly Color LightBlue = FromHex("#FF2A00");
        public static readonly Color Scarlet = FromHex("#009EFF");

        public static Color FromHex(string hex)
        {
            hex = hex.Replace("#", "");

            if (hex.Length == 6)
            {
                hex += "FF";
            }

            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            byte a = Convert.ToByte(hex.Substring(6, 2), 16);

            return new Color(r, g, b, a);
        }
    }
}