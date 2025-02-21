using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Core.Managers
{
    public class DeepLinkManager: MonoBehaviour
    {
        public static DeepLinkManager Instance { get; private set; }
        public string deeplinkURL;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;                
                Application.deepLinkActivated += OnDeepLinkActivated;
                if (!string.IsNullOrEmpty(Application.absoluteURL))
                {
                    // Cold start and Application.absoluteURL not null so process Deep Link.
                    HandleDeepLink(Application.absoluteURL);
                }
                // Initialize DeepLink Manager global variable.
                else deeplinkURL = "[none]";
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDeepLinkActivated(string url)
        {
            HandleDeepLink(url);
        }

        private void HandleDeepLink(string url)
        {
            try
            {
                Debug.Log("Deep link received: " + url);

                Uri uri = new Uri(url);
                if (uri.Scheme != "theforgottenletters") return;
                
                string host = uri.Host;
                string path = uri.AbsolutePath;

                switch (host)
                {
                    case "start":
                        SceneManager.LoadScene("Main Menu");
                        break;
                    
                    case "bahrelsafa":
                        SceneManager.LoadScene("Bahr El Safa");
                        break;
                    
                    case "moon":
                        SceneManager.LoadScene("Moon");
                        break;

                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error handling deep link: " + ex.Message);
            }
        }
    }
}