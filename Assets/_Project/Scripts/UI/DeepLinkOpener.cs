using UnityEngine;

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
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager"))
            {
                AndroidJavaObject intent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);
                return intent != null;
            }
        }
        catch
        {
            return false;
        }
    }
}
