using System;
using _Project.Scripts.Core.Scriptable_Events;
using _Project.Scripts.Core.Scriptable_Events.EventTypes.Float;
using _Project.Scripts.Core.Utilities.Scene_Management;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class UpdateCatalogManager : MonoBehaviour
{
    [Header("Scene to Load")]
    [SerializeField] private AssetReference sceneToLoad;

    [Header("Event References")]
    [SerializeField] private AssetReference loadingProgressRef;
    [SerializeField] private AssetReference showLoadingScreenRef;

    [Header("Events Fallback")]
    public FloatEvent onProgressUpdate;
    public GameEvent onShowLoadingScreen;

    private FloatEvent LoadingProgress => GetEventSafe(loadingProgressRef, onProgressUpdate);
    private GameEvent ShowLoadingScreen => GetEventSafe(showLoadingScreenRef, onShowLoadingScreen);

    private void Awake()
    {
        HandleUpdateFlowAsync().Forget();
    }

    private async UniTaskVoid HandleUpdateFlowAsync()
    {
        var catalogsUpdated = await CheckForCatalogUpdatesAsync();
        if (!catalogsUpdated)
        {
            var contentDownloaded = await CheckAndDownloadContentAsync();
            if (!contentDownloaded)
            {
                await ProceedToGame();
            }
        }
    }

    private async UniTask<bool> CheckForCatalogUpdatesAsync()
    {
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        await checkHandle.Task;

        if (checkHandle.Status == AsyncOperationStatus.Succeeded)
        {
            var catalogs = checkHandle.Result;
            if (catalogs.Count > 0)
            {
                ShowLoadingScreen.Raise();

                var updateHandle = Addressables.UpdateCatalogs(catalogs);
                while (!updateHandle.IsDone)
                {
                    LoadingProgress.Raise(updateHandle.PercentComplete);
                    await UniTask.Yield();
                }

                await updateHandle.Task;

                if (updateHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    Debug.Log("Catalogs updated successfully.");
                    Addressables.Release(updateHandle);
                    Addressables.Release(checkHandle);

                    await DownloadNewContentAsync();
                    return true;
                }
                else
                {
                    Debug.LogError("Failed to update catalogs.");
                }

                Addressables.Release(updateHandle);
            }
            else
            {
                Debug.Log("No catalogs to update.");
            }
        }
        else
        {
            Debug.LogError("Failed to check for catalog updates.");
        }

        Addressables.Release(checkHandle);
        return false;
    }

    private async UniTask<bool> CheckAndDownloadContentAsync()
    {
        var sizeHandle = Addressables.GetDownloadSizeAsync("DownloadOnStart");
        await sizeHandle.Task;

        if (sizeHandle.Status == AsyncOperationStatus.Succeeded)
        {
            if (sizeHandle.Result > 0)
            {
                await DownloadNewContentAsync();
                return true;
            }
            else
            {
                Debug.Log("All content already downloaded.");
            }
        }
        else
        {
            Debug.LogError("Failed to get download size.");
        }

        Addressables.Release(sizeHandle);
        return false;
    }

    private async UniTask DownloadNewContentAsync()
    {
        ShowLoadingScreen.Raise();

        var downloadHandle = Addressables.DownloadDependenciesAsync("DownloadOnStart");
        while (!downloadHandle.IsDone)
        {
            LoadingProgress.Raise(downloadHandle.PercentComplete);
            await UniTask.Yield();
        }

        await downloadHandle.Task;

        if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("New content downloaded successfully.");
        }
        else
        {
            Debug.LogError("Failed to download new content.");
        }

        Addressables.Release(downloadHandle);
        await ProceedToGame();
    }

    private async UniTask ProceedToGame()
    {
        ShowLoadingScreen.Raise();
        await LoadSceneAsync(sceneToLoad);
    }

    public async UniTask LoadSceneAsync(
        AssetReference sceneReference,
        Action<AsyncOperationHandle<SceneInstance>> onComplete = null,
        Action<float> onProgress = null)
    {
        await UniTask.DelayFrame(2);

        var handle = Addressables.LoadSceneAsync(sceneReference, LoadSceneMode.Additive);
        handle.Completed += h => ProcessLoadCompletion(h, onComplete);

        while (!handle.IsDone)
        {
            onProgress?.Invoke(handle.PercentComplete);
            await UniTask.Yield();
        }

        await handle.Task;
    }

    private async void ProcessLoadCompletion(
        AsyncOperationHandle<SceneInstance> handle,
        Action<AsyncOperationHandle<SceneInstance>> onComplete)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            await UniTask.DelayFrame(2);
            SceneManager.SetActiveScene(handle.Result.Scene);
        }
        else
        {
            Debug.LogError($"Failed to load scene: {handle.DebugName}");
        }

        onComplete?.Invoke(handle);
    }

    private T GetEventSafe<T>(AssetReference reference, T fallback) where T : ScriptableObject
    {
        try
        {
            return EventLoader.Instance.GetEvent<T>(reference);
        }
        catch
        {
            return fallback;
        }
    }
}
