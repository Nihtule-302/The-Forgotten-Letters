using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Core.Utilities.Scene_Management
{
    public class SceneUnloader
    {
        public event Action OnSceneUnloaded;
        public event Action<string> OnSceneUnloadFailed;

        public void UnloadScene(AsyncOperationHandle<SceneInstance> sceneHandle, Action<float> onProgress = null)
        {
            UnloadSceneAsync(sceneHandle, onProgress).Forget(); // Optional, for non-await usage
        }

        public async UniTask UnloadSceneAsync(
            AsyncOperationHandle<SceneInstance> sceneHandle,
            Action<float> onProgress = null)
        {
            if (!sceneHandle.IsValid())
            {
                Debug.LogWarning("Invalid scene handle provided for unloading.");
                return;
            }

            var sceneToUnload = sceneHandle.Result.Scene;

            await UniTask.NextFrame(); // optional frame delay

            var unloadHandle = Addressables.UnloadSceneAsync(sceneHandle, false);

            while (!unloadHandle.IsDone)
            {
                onProgress?.Invoke(unloadHandle.PercentComplete);
                await UniTask.Yield();
            }

            if (!unloadHandle.IsValid())
            {
                Debug.LogError("Handle is not valid in OnSceneLoadCallback");
                return;
            }

            if (unloadHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(unloadHandle);
                OnSceneUnloaded?.Invoke();
            }
            else
            {
                Debug.LogError($"Failed to unload scene: {unloadHandle.DebugName}");
                OnSceneUnloadFailed?.Invoke(unloadHandle.DebugName);
            }
        }
    }
}
