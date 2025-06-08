using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Scriptable_Events;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;


namespace _Project.Scripts.Core.Utilities.Scene_Management
{
    public class SceneAdder : MonoBehaviour
    {
        [SerializeField] private AssetReference SceneToAdd;
        [SerializeField] private AssetReference triggerNextSetupStepRef;

        GameEvent TriggerNextSetupStep
        {
            get
            {
                if (triggerNextSetupStepRef == null || string.IsNullOrEmpty(triggerNextSetupStepRef.AssetGUID) || !triggerNextSetupStepRef.RuntimeKeyIsValid())
                {
                    Debug.LogError("TriggerNextSetupStep reference is not set or invalid.");
                    return null;
                }
                try
                {
                    return EventLoader.Instance.GetEvent<GameEvent>(triggerNextSetupStepRef);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error checking TriggerNextSetupStep event: {ex.Message}");
                    return null;
                }
            }
        }

        SceneLoader sceneLoader;

        public async UniTask AddSceneAsync(bool setInitialGameScreen = false)
        {
            sceneLoader ??= new SceneLoader();
            sceneLoader.OnSceneLoaded += sceneInstance =>
            {
                Debug.Log($"Scene {sceneInstance.Scene.name} loaded successfully.");
                if (sceneInstance.Scene == SceneManager.GetActiveScene())
                {
                    if (triggerNextSetupStepRef == null || string.IsNullOrEmpty(triggerNextSetupStepRef.AssetGUID) || !triggerNextSetupStepRef.RuntimeKeyIsValid())
                    {
                        Debug.LogError("TriggerNextSetupStep is not set or invalid.");
                        return;
                    }
                    var triggerEvent = TriggerNextSetupStep;
                    if (triggerEvent != null)
                    {
                        triggerEvent.Raise();
                    }
                }
            };

            if (setInitialGameScreen)
            {
                await sceneLoader.LoadSceneAsync(SceneToAdd, OnSceneLoadCallback);
            }
            else
            { 
                await sceneLoader.LoadSceneAsync(SceneToAdd);
            }
        }

        public async UniTask AddSceneAsync(AssetReference chossenScene, bool setInitialGameScreen = false)
        {
            sceneLoader ??= new SceneLoader();
            sceneLoader.OnSceneLoaded += sceneInstance =>
            {
                Debug.Log($"Scene {sceneInstance.Scene.name} loaded successfully., SceneManager.GetActiveScene(): {SceneManager.GetActiveScene().name}, sceneInstance.Scene == SceneManager.GetActiveScene(): {sceneInstance.Scene == SceneManager.GetActiveScene()} ");
                
                if (sceneInstance.Scene == SceneManager.GetActiveScene())
                {
                    if (triggerNextSetupStepRef == null || !triggerNextSetupStepRef.RuntimeKeyIsValid())
                    {
                        Debug.LogError("TriggerNextSetupStep is not set or invalid.");
                        return;
                    }
                    TriggerNextSetupStep?.Raise();
                }
            };

            if (setInitialGameScreen)
            {
                await sceneLoader.LoadSceneAsync(chossenScene, OnSceneLoadCallback);
            }
            else
            { 
                await sceneLoader.LoadSceneAsync(chossenScene);
            }
        }

        private void OnSceneLoadCallback(AsyncOperationHandle<SceneInstance> handle)
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load scene: {handle.DebugName}");
                return;
            }

            if (SceneTransitionManager.Instance != null)
            {
                Debug.Log($"Scene loaded successfully: {handle.DebugName}, setting currentHandle in SceneTransitionManager.");
                SceneTransitionManager.Instance.currentHandle = handle;
            }
            else
            {
                Debug.LogWarning("SceneTransitionManager instance not ready to assign currentHandle.");
            }
        }
    }
}