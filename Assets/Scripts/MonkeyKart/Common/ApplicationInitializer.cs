using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonkeyKart.Common
{
    static class ApplicationInitializer
    {
        const string TAG = "ApplicationInitializer";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            /*
            if (SceneManager.GetActiveScene().name == MonkeyKartScenes.INITIALIZATION) return;
            SceneManager.LoadSceneAsync(MonkeyKartScenes.INITIALIZATION);
            */
        }
    }
}