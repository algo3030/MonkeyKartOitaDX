using UnityEngine;

namespace MonkeyKart.Common
{
    public static class Log
    {
        public static void d(string tag, string message) 
        {
            Debug.Log($"<color=cyan>[{tag}]</color> {message}");
        }

        public static void e(string tag, string message)
        {
            Debug.LogError($"<color=red>[{tag}]</color> {message}");
        }

        public static void w(string tag, string message)
        {
            Debug.LogWarning($"<color=yellow>[{tag}]</color> {message}");
        }
    }
}