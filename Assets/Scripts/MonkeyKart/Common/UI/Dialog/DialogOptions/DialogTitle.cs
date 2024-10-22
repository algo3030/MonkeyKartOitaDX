using UnityEngine;

namespace MonkeyKart.Common.UI
{
    public abstract class DialogTitle { }

    public class NoneTitle: DialogTitle { }

    public class MessageTitle : DialogTitle
    {
        public Color Background {  get; private set; }
        public string Message { get; private set; }
        public MessageTitle(string message, Color bgColor)
        {
            Message = message;
            Background = bgColor;
        }
    }
    public class IconTitle : DialogTitle
    {
        public enum IconType
        {
            Warn
        }

        public Color Background { get; private set; }
        public IconType Icon { get; private set; }
        public IconTitle(IconType icon, Color bgColor)
        {
            Icon = icon;
            Background = bgColor;
        }
    }

}