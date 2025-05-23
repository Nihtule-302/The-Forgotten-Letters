using System;
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

        public void LoadScene(AssetReference sceneReference,
            Action<AsyncOperationHandle<SceneInstance>> onComplete = null)
        {
            Addressables.LoadSceneAsync(sceneReference, LoadSceneMode.Additive)
                .Completed += handle => ProcessLoadCompletion(handle, onComplete);
        }

        private void ProcessLoadCompletion(AsyncOperationHandle<SceneInstance> handle,
            Action<AsyncOperationHandle<SceneInstance>> onComplete)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
                ProcessSuccessfulLoad(handle);
            else
                ProcessFailedLoad(handle);

            onComplete?.Invoke(handle);
        }

        private void ProcessSuccessfulLoad(AsyncOperationHandle<SceneInstance> handle)
        {
            // Set the newly loaded scene as the active scene.
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