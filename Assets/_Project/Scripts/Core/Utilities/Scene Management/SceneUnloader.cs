using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace _Project.Scripts.Core.Utilities.Scene_Management
{
    public class SceneUnloader
    {
        public event Action OnSceneUnloaded;
        public event Action<string> OnSceneUnloadFailed;

        public void UnloadScene(AsyncOperationHandle<SceneInstance> sceneHandle)
        {
            if (!sceneHandle.IsValid())
            {
                Debug.LogWarning("Invalid scene handle provided for unloading.");
                return;
            }

            Addressables.UnloadSceneAsync(sceneHandle)
                .Completed += handle => ProcessUnloadCompletion(handle);
        }

        private void ProcessUnloadCompletion(AsyncOperationHandle<SceneInstance> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                ProcessSuccessfulUnload(handle);
            }
            else
            {
                ProcessFailedUnload(handle);
            }
        }

        private void ProcessSuccessfulUnload(AsyncOperationHandle<SceneInstance> handle)
        {
            OnSceneUnloaded?.Invoke();
        }

        private void ProcessFailedUnload(AsyncOperationHandle<SceneInstance> handle)
        {
            Debug.LogError($"Failed to unload scene: {handle.DebugName}");
            OnSceneUnloadFailed?.Invoke(handle.DebugName);
            Addressables.Release(handle);
        }
    }
}
