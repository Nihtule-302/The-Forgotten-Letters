using System;
using System.Collections.Generic;
using _Project.Scripts.Core.Scriptable_Events;
using _Project.Scripts.Core.Utilities.Scene_Management;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _Project.Scripts.Core.Managers
{
    public class DeepLinkManager : MonoBehaviour
    {
        public string deeplinkURL;

        [Header("Deep Link Settings")]
        [SerializeField] private SceneAdder sceneAdder;

        [SerializeField] private AssetReference initialScene;

        [Header("Main Scenes")]
        [SerializeField] private AssetReference mainMenu;
        [SerializeField] private AssetReference Moon;
        [SerializeField] private AssetReference BahrelSafa;

        [Header("Minigame Scenes")]
        [SerializeField] private AssetReference drawLetters;
        [SerializeField] private AssetReference objectDetection;
        [SerializeField] private AssetReference letterHunt;

        [Header("Event Ref")]
        [SerializeField] private AssetReference hideLoadingScreen;
        private GameEvent HideLoadingScreen => EventLoader.Instance.GetEvent<GameEvent>(hideLoadingScreen);

        public static DeepLinkManager Instance { get; private set; }

        private readonly Dictionary<string, AssetReference> deepLinkScenes = new Dictionary<string, AssetReference>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // Initialize deep link scenes dictionary
                deepLinkScenes.Add("start", mainMenu);
                deepLinkScenes.Add("bahrelsafa", BahrelSafa);
                deepLinkScenes.Add("moon", Moon);
                deepLinkScenes.Add("letterhunt", letterHunt);
                deepLinkScenes.Add("letterdrawing", drawLetters);
                deepLinkScenes.Add("objectdetection", objectDetection);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsInitialized)
            {
                FirebaseManager.Instance.OnFirebaseInitialized += StartDeepLink;
            }
            else
            {
                FirebaseManager.OnFirebaseManagerReady += OnFirebaseManagerReadyHandler;
            }

            Application.deepLinkActivated += OnDeepLinkActivated;
        }

        private void OnDisable()
        {
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.OnFirebaseInitialized -= StartDeepLink;
            }
            FirebaseManager.OnFirebaseManagerReady -= OnFirebaseManagerReadyHandler;

            Application.deepLinkActivated -= OnDeepLinkActivated;
        }

        private void OnFirebaseManagerReadyHandler()
        {
            if (FirebaseManager.Instance != null)
            {
                FirebaseManager.Instance.OnFirebaseInitialized += StartDeepLink;
            }
            FirebaseManager.OnFirebaseManagerReady -= OnFirebaseManagerReadyHandler;
        }

        /// <summary>
        /// Starts handling deep links after Firebase initialization.
        /// </summary>
        private async void StartDeepLink()
        {
            Debug.Log("DeepLinkManager: StartDeepLink called.");

            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                // Cold start with deep link URL present
                await HandleDeepLink(Application.absoluteURL);
                return;
            }

            deeplinkURL = "[none]";
            await sceneAdder.AddSceneAsync(initialScene, true);
            HideLoadingScreen.Raise();
        }

        private async void OnDeepLinkActivated(string url)
        {
            await HandleDeepLink(url);
        }

        /// <summary>
        /// Processes the given deep link URL and transitions to the corresponding scene.
        /// </summary>
        public async UniTask HandleDeepLink(string url)
        {
            try
            {
                Debug.Log("Deep link received: " + url);
                var uri = new Uri(url);

                if (uri.Scheme != "theforgottenletters")
                    return;

                if (deepLinkScenes.TryGetValue(uri.Host.ToLowerInvariant(), out var sceneRef))
                {
                    await sceneAdder.AddSceneAsync(sceneRef, true);
                    HideLoadingScreen.Raise();
                }
                else
                {
                    Debug.LogWarning($"DeepLinkManager: Unknown deep link host '{uri.Host}'");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Error handling deep link: " + ex.Message);
            }
        }
    }
}
