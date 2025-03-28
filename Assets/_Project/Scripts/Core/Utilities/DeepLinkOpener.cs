using UnityEngine;

namespace _Project.Scripts.Core.Utilities
{
    public class DeepLinkOpener : MonoBehaviour
    {
        public string fallbackWebURL = "https://aliwafa.itch.io/the-forgotten-letters";

        public void OpenDeepLink()
        {
            if (IsAppInstalled("com.nihtule.deeplinktest"))
            {
                Application.OpenURL("deeplinktest://app");
            }
            else
            {
                Application.OpenURL(fallbackWebURL);
            }
        }

        private bool IsAppInstalled(string packageName)
        {
            try
            {
                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager"))
                {
                    var intent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);
                    return intent != null;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
