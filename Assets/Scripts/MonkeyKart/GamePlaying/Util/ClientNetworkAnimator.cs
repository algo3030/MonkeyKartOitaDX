using Unity.Netcode.Components;
using UnityEngine;

namespace MonkeyKart.GamePlaying.Util
{
    [DisallowMultipleComponent]
    public class ClientNetworkAnimator : NetworkAnimator
    {
        protected override bool OnIsServerAuthoritative()
        {
            return false;
        }
    }
}