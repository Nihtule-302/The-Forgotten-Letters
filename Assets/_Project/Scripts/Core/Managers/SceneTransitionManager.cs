using System;
using _Project.Scripts.Core.Utilities.Scene_Management;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace _Project.Scripts.Core.Managers
{
    public class SceneTransitionManager : MonoBehaviour
    {
        #region Unity Methods

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitUtilities();
            SubscribeEvents();
        }

        #endregion

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                UnsubscribeEvents();
            }
        }

        private void UnsubscribeEvents()
        {
            if (loader != null)
            {
                loader.OnSceneLoaded -= SceneLoadedHandler;
                loader.OnSceneLoadFailed -= SceneLoadFailedHandler;
            }

            if (unloader != null)
            {
                unloader.OnSceneUnloaded -= SceneUnloadedHandler;
                unloader.OnSceneUnloadFailed -= SceneUnloadFailedHandler;
            }
        }

        #region Fields

        public static SceneTransitionManager Instance { get; private set; }
        [SerializeField] private AssetReference loaderSceen;

        private SceneLoader loader;
        private SceneUnloader unloader;
        private AsyncOperationHandle<SceneInstance> currentScene;


        private AsyncOperationHandle<SceneInstance> loadingHandle;
        private AsyncOperationHandle<SceneInstance> targetHandle;
        private AssetReference loadingRef;
        private AssetReference targetRef;

        private bool isTransitioning;

        // Simplified transition states.
        private enum TransitionState
        {
            Idle,
            LoadingScreenIn,
            UnloadingCurrent,
            LoadingTarget,
            UnloadingScreen
        }

        private TransitionState state = TransitionState.Idle;

        #endregion

        #region Events

        public event Action<SceneInstance> OnSceneLoaded;
        public event Action OnSceneUnloaded;

        #endregion

        #region Initialization

        private void InitUtilities()
        {
            loader = new SceneLoader();
            unloader = new SceneUnloader();
        }

        private void SubscribeEvents()
        {
            loader.OnSceneLoaded += SceneLoadedHandler;
            loader.OnSceneLoadFailed += SceneLoadFailedHandler;
            unloader.OnSceneUnloaded += SceneUnloadedHandler;
            unloader.OnSceneUnloadFailed += SceneUnloadFailedHandler;
        }

        #endregion

        #region Transition Methods

        public void TransitionScene(AssetReference targetScene)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("Transition already in progress.");
                return;
            }

            isTransitioning = true;
            state = TransitionState.LoadingScreenIn;
            loadingRef = loaderSceen;
            targetRef = targetScene;

            // Step 1: Load the loading screen.
            loader.LoadScene(loadingRef, OnSceneLoadedCallback);
        }

        private void OnSceneLoadedCallback(AsyncOperationHandle<SceneInstance> handle)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                HandleLoadFailure(handle);
                return;
            }

            switch (state)
            {
                case TransitionState.LoadingScreenIn:
                    ProcessLoadingScreenIn(handle);
                    break;
                case TransitionState.LoadingTarget:
                    ProcessLoadingTarget(handle);
                    break;
                default:
                    Debug.LogWarning("OnSceneLoadedCallback called in unexpected state: " + state);
                    break;
            }
        }

        private void ProcessLoadingScreenIn(AsyncOperationHandle<SceneInstance> handle)
        {
            loadingHandle = handle;

            if (currentScene.IsValid())
            {
                state = TransitionState.UnloadingCurrent;
                unloader.UnloadScene(currentScene);
            }
            else
            {
                state = TransitionState.LoadingTarget;
                loader.LoadScene(targetRef, OnSceneLoadedCallback);
            }
        }

        private void ProcessLoadingTarget(AsyncOperationHandle<SceneInstance> handle)
        {
            targetHandle = handle;
            state = TransitionState.UnloadingScreen;
            unloader.UnloadScene(loadingHandle);
        }

        #endregion

        #region Event Handlers

        private void SceneUnloadedHandler()
        {
            if (state == TransitionState.UnloadingCurrent)
            {
                // Step 3: Load the target scene after unloading the current scene.
                state = TransitionState.LoadingTarget;
                loader.LoadScene(targetRef, OnSceneLoadedCallback);
            }
            else if (state == TransitionState.UnloadingScreen)
            {
                // Transition complete.
                currentScene = targetHandle;
                ResetTransition();
                OnSceneUnloaded?.Invoke();
            }
        }

        private void SceneLoadedHandler(SceneInstance sceneInstance)
        {
            // For direct loads (like the initial scene), forward the event.
            OnSceneLoaded?.Invoke(sceneInstance);
        }

        private void SceneLoadFailedHandler(string debugName)
        {
            Debug.LogError($"Failed to load scene: {debugName}");
            ResetTransition();
        }

        private void SceneUnloadFailedHandler(string debugName)
        {
            Debug.LogError($"Failed to unload scene: {debugName}");
            ResetTransition();
        }

        #endregion

        #region Helper Methods

        private void OnInitialLoadComplete(AsyncOperationHandle<SceneInstance> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                currentScene = handle;
                OnSceneLoaded?.Invoke(handle.Result);
            }
            else
            {
                Debug.LogError($"Failed to load initial scene: {handle.DebugName}");
            }
        }

        private void HandleLoadFailure(AsyncOperationHandle<SceneInstance> handle)
        {
            Debug.LogError($"Failed to load scene: {handle.DebugName} in state: {state}");
            ResetTransition();
        }

        private void ResetTransition()
        {
            isTransitioning = false;
            state = TransitionState.Idle;
        }

        #endregion
    }
}