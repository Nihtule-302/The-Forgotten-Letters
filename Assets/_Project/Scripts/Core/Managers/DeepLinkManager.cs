using System;
using _Project.Scripts.Core.Utilities.Scene_Management;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Core.Managers
{
    public class DeepLinkManager: MonoBehaviour
    {
        public static DeepLinkManager Instance { get; private set; }
        public string deeplinkURL;

        [Header("Deep Link Settings")]
        [SerializeField] private SceneTransitioner sceneTransitioner;
        [SerializeField] private AssetReference initialScene;

        [Header("Main Scenes")]
        [SerializeField] private AssetReference mainMenu;
        [SerializeField] private AssetReference Moon;
        [SerializeField] private AssetReference BahrelSafa;

        [Header("Minigame Scenes")]
        [SerializeField] private AssetReference drawLetters;
        [SerializeField] private AssetReference objectDetection;
        [SerializeField] private AssetReference letterHunt;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                // StartDeepLink();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void OnEnable()
        {
            FirebaseManager.Instance.OnFirebaseInitialized += StartDeepLink;
        }
        void OnDisable()
        {
            FirebaseManager.Instance.OnFirebaseInitialized -= StartDeepLink;
        }


        private void StartDeepLink()
        {
            Application.deepLinkActivated += OnDeepLinkActivated;
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                // Cold start and Application.absoluteURL not null so process Deep Link.
                HandleDeepLink(Application.absoluteURL);
            }
            // Initialize DeepLink Manager global variable.
            else
            {
                deeplinkURL = "[none]";
                sceneTransitioner.TranstionToScene(initialScene);
            }
        }

        private void OnDeepLinkActivated(string url)
        {
            HandleDeepLink(url);
        }
        [ContextMenu("Test Deep Link")]
        private void TestDeepLink()
        {
            HandleDeepLink(deeplinkURL);
        }
        public void HandleDeepLink(string url)
        {
            try
            {
                Debug.Log("Deep link received: " + url);

                var uri = new Uri(url);
                if (uri.Scheme != "theforgottenletters") return;
                
                var host = uri.Host;
                var path = uri.AbsolutePath;

                switch (host)
                {
                    case "start":
                        sceneTransitioner.TranstionToScene(mainMenu);
                        break;
                    
                    case "bahrelsafa":
                        sceneTransitioner.TranstionToScene(BahrelSafa);
                        break;
                    
                    case "moon":
                        sceneTransitioner.TranstionToScene(Moon);
                        break;
                        
                    case "letterhunt":
                        sceneTransitioner.TranstionToScene(letterHunt);
                        break;
                    
                    case "letterdrawing":
                        sceneTransitioner.TranstionToScene(drawLetters);
                        break;
                    
                    case "objectdetection":
                        sceneTransitioner.TranstionToScene(objectDetection);
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