using Unity.Netcode.Components;
using UnityEngine;

namespace MonkeyKart.GamePlaying.Util
{
    [DisallowMultipleComponent]
    public class ClientNetworkTransform : NetworkTransform
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}