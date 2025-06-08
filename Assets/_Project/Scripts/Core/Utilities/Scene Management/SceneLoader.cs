using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Core.Utilities.Scene_Management
{
    public class SceneLoader
    {
        public event Action<SceneInstance> OnSceneLoaded;
        public event Action<string> OnSceneLoadFailed;
        
        public void LoadScene(AssetReference sceneReference, Action<AsyncOperationHandle<SceneInstance>> onComplete = null, Action<float> onProgress = null)
        {
            LoadSceneAsync(sceneReference, onComplete, onProgress).Forget(); // Optional, for non-await usage
        }
        public async UniTask LoadSceneAsync(AssetReference sceneReference, Action<AsyncOperationHandle<SceneInstance>> onComplete = null, Action<float> onProgress = null)
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
                await ProcessSuccessfulLoad(handle);
            else
                ProcessFailedLoad(handle);

            onComplete?.Invoke(handle);
        }

        private async UniTask ProcessSuccessfulLoad(AsyncOperationHandle<SceneInstance> handle)
        {
            await UniTask.DelayFrame(2);
            SceneManager.SetActiveScene(handle.Result.Scene);
            OnSceneLoaded?.Invoke(handle.Result);
        }

        private void ProcessFailedLoad(AsyncOperationHandle<SceneInstance> handle)
        {
            Debug.LogError($"Failed to load scene: {handle.DebugName}");
            OnSceneLoadFailed?.Invoke(handle.DebugName);
            Addressables.Release(handle);
        }
    }
}