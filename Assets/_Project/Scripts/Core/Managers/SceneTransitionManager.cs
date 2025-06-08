using System;
using System.Threading.Tasks;
using _Project.Scripts.Core.Scriptable_Events;
using _Project.Scripts.Core.Scriptable_Events.EventTypes.Float;
using _Project.Scripts.Core.Utilities.Scene_Management;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Core.Managers
{
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [SerializeField] private AssetReference TempScreen;

        [Header("Event References")]
        [SerializeField] private AssetReference loadingProgressRef;
        [SerializeField] private AssetReference hideLoadingScreenRef;
        [SerializeField] private AssetReference showLoadingScreenRef;
        [SerializeField] private AssetReference triggerNextSetupStepRef;
        [SerializeField] private AssetReference sceneTransitionTriggeredRef;


        private SceneLoader loader;
        private SceneUnloader unloader;

        public AsyncOperationHandle<SceneInstance> currentHandle;
        private AsyncOperationHandle<SceneInstance> TempHandle;
        private AsyncOperationHandle<SceneInstance> targetHandle;


        private AssetReference tempRef;
        private AssetReference targetRef;

        private bool isTransitioning;

        private enum TransitionState
        {
            Idle,
            LoadingTempScreen,
            UnloadingCurrent,
            LoadingTarget,
            UnloadingTempScreen
        }

        private TransitionState state = TransitionState.Idle;

        public event Action<SceneInstance> OnSceneLoaded;
        public event Action OnSceneUnloaded;

        private GameEvent TriggerNextSetupStep => EventLoader.Instance.GetEvent<GameEvent>(triggerNextSetupStepRef);
        private FloatEvent LoadingProgress => EventLoader.Instance.GetEvent<FloatEvent>(loadingProgressRef);
        private GameEvent HideLoadingScreen => EventLoader.Instance.GetEvent<GameEvent>(hideLoadingScreenRef);
        private GameEvent ShowLoadingScreen => EventLoader.Instance.GetEvent<GameEvent>(showLoadingScreenRef);
        private GameEvent SceneTransitionTriggered => EventLoader.Instance.GetEvent<GameEvent>(sceneTransitionTriggeredRef);

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            loader = new SceneLoader();
            unloader = new SceneUnloader();

            loader.OnSceneLoaded += HandleSceneLoaded;
            loader.OnSceneLoadFailed += HandleSceneLoadFailed;
            unloader.OnSceneUnloaded += HandleSceneUnloaded;
            unloader.OnSceneUnloadFailed += HandleSceneUnloadFailed;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;

                loader.OnSceneLoaded -= HandleSceneLoaded;
                loader.OnSceneLoadFailed -= HandleSceneLoadFailed;
                unloader.OnSceneUnloaded -= HandleSceneUnloaded;
                unloader.OnSceneUnloadFailed -= HandleSceneUnloadFailed;
            }
        }

        public async UniTask TransitionSceneAsync(AssetReference targetScene)
        {
            if (isTransitioning)
            {
                Debug.LogWarning("Transition already in progress.");
                return;
            }

            SceneTransitionTriggered?.Raise();
            ShowLoadingScreen?.Raise();
            await UniTask.Delay(TimeSpan.FromSeconds(1.3)); // Small delay to ensure the previous transition is fully reset

            isTransitioning = true;
            state = TransitionState.LoadingTempScreen;

            tempRef = TempScreen;
            targetRef = targetScene;

            Debug.Log($"currentHandle: {currentHandle.DebugName}");

            await loader.LoadSceneAsync(tempRef, OnSceneLoadCallback);
        }

        private async void OnSceneLoadCallback(AsyncOperationHandle<SceneInstance> handle)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load scene in state: {state}, scene: {handle.DebugName}");
                ResetTransition();
                return;
            }

            switch (state)
            {
                case TransitionState.LoadingTempScreen:
                    TempHandle = handle;
                    state = TransitionState.UnloadingCurrent;
                    if (currentHandle.IsValid())
                        await unloader.UnloadSceneAsync(currentHandle, ReportProgress);
                    state = TransitionState.LoadingTarget;
                    await loader.LoadSceneAsync(targetRef, OnSceneLoadCallback, ReportProgress);
                    break;

                case TransitionState.LoadingTarget:
                    targetHandle = handle;
                    state = TransitionState.UnloadingTempScreen;
                    await unloader.UnloadSceneAsync(TempHandle);
                    break;

                default:
                    Debug.LogWarning("Unexpected load callback state: " + state);
                    break;
            }
        }

        private async void HandleSceneUnloaded()
        {
            if (state == TransitionState.UnloadingTempScreen)
            {
                currentHandle = targetHandle;
                ResetTransition();
                OnSceneUnloaded?.Invoke();
                ShowLoadingScreen?.Raise();
                await UniTask.Delay(TimeSpan.FromSeconds(1)); // Small delay to ensure the scene is fully unloaded
            }

        }

        private void HandleSceneLoaded(SceneInstance sceneInstance)
        {
            if (state == TransitionState.LoadingTarget && sceneInstance.Scene == SceneManager.GetActiveScene())
                TriggerNextSetupStep?.Raise();

            OnSceneLoaded?.Invoke(sceneInstance);
        }

        private void HandleSceneLoadFailed(string debugName)
        {
            Debug.LogError($"Scene load failed: {debugName}");
            ResetTransition();
        }

        private void HandleSceneUnloadFailed(string debugName)
        {
            Debug.LogError($"Scene unload failed: {debugName}");
            ResetTransition();
        }

        private void ReportProgress(float progress)
        {
            float adjusted = state switch
            {
                TransitionState.UnloadingCurrent => Mathf.Clamp01(progress * 0.5f),
                TransitionState.LoadingTarget => Mathf.Clamp01(progress * 0.5f + 0.5f),
                _ => 0f
            };

            LoadingProgress?.Raise(adjusted);
        }

        private void ResetTransition()
        {
            isTransitioning = false;
            state = TransitionState.Idle;
        }
    }
}
