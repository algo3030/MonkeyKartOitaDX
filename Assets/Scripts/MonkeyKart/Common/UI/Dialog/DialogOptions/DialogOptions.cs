using UnityEngine;
using System;
using System.Drawing;

namespace MonkeyKart.Common.UI
{
    public static class DialogPaddings
    {
        public static readonly Vector2 Wide = new Vector2(300, 150);
        public static readonly Vector2 Small = new Vector2(150, 100);
    }

    /// <summary>
    /// ダイアログ生成のパラメーター。
    /// </summary>
    public class DialogOptions
    {

        /// <summary>
        /// Horizontal, Vertical
        /// </summary>
        public Vector2 Padding { get; private set; } = DialogPaddings.Small;
        public DialogTitle Title { get; private set; } = new NoneTitle();
        public DialogBody Body { get; private set; } = new NoneBody();

        public DialogOptions SetPadding(Vector2 padding)
        {
            this.Padding = padding;
            return this;
        }

        public DialogOptions SetTitle(DialogTitle title) 
        {
            this.Title = title;
            return this;
        }

        public DialogOptions SetBody(DialogBody body)
        {
            this.Body = body;
            return this;
        }
    }
}