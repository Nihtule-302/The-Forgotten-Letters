using System;
using System.Collections;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Scriptable_Events;
using _Project.Scripts.Core.Utilities.Scene_Management;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class AdressableLoadingManager : MonoBehaviour
{

    [Header("Scene to Load")] [SerializeField]
    private AssetReference sceneToLoad;

    [Header("Are Assets Loaded")] public bool areEventsReady;

    // public bool areModelsReady = false;
    public bool areLogicalSOsReady;
    private bool finished;

    private SceneLoader sceneLoader;
    private bool transitioning;
    public static AdressableLoadingManager Instance { get; private set; }

    [Header("EventRef")]
    [SerializeField] AssetReference showLoadingScreenRef; 
    [SerializeField] AssetReference hideLoadingScreenRef;
    [SerializeField] private AssetReference triggerNextSetupStepRef;

    GameEvent TriggerNextSetupStep => EventLoader.Instance.GetEvent<GameEvent>(triggerNextSetupStepRef);
    GameEvent ShowLoadingScreenTrue => EventLoader.Instance.GetEvent<GameEvent>(showLoadingScreenRef);
    GameEvent HideLoadingScreenTrue => EventLoader.Instance.GetEvent<GameEvent>(hideLoadingScreenRef);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            sceneLoader = new SceneLoader();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        EventLoader.Instance.OnAllEventsLoaded += OnEventsReady;
        // SentisModelLoader.Instance.OnAllModelsLoaded += OnModelsReady;
        PersistentSOManager.Instance.OnAllSOsLoaded += OnLogicalSOsReady;
        transitioning = false;
        ShowLoadingScreenTrue?.Raise();
    }

    private void Update()
    {
        if (finished) return;
        if (transitioning) return;
        // Check if all addressables are loaded
        if (areEventsReady && areLogicalSOsReady)
        {
            transitioning = true;
            CheckAllAddressablesReady();
        }
    }

    private void OnDestroy()
    {
        EventLoader.Instance.OnAllEventsLoaded -= OnEventsReady;
        // SentisModelLoader.Instance.OnAllModelsLoaded -= OnModelsReady;
        PersistentSOManager.Instance.OnAllSOsLoaded -= OnLogicalSOsReady;
    }

    public event Action OnAddressablesReady;

    private void OnEventsReady()
    {
        areEventsReady = true;
    }

    // private void OnModelsReady()
    // {
    //     areModelsReady = true;
    //     CheckAllAddressablesReady();
    // }

    private void OnLogicalSOsReady()
    {
        areLogicalSOsReady = true;
    }

    private void CheckAllAddressablesReady()
    {
        Debug.Log("All addressables are ready. Loading scene: " + sceneToLoad.RuntimeKey);

        LoadSceneAsync().Forget();
        
    }
    private async UniTaskVoid LoadSceneAsync()
    {
        await UniTask.Delay(500); // 0.5 second delay
        await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
        await UniTask.Yield(PlayerLoopTiming.PostLateUpdate); // Similar to two WaitForEndOfFrame()

        sceneLoader.OnSceneLoaded += async sceneInstance =>
        {
            Debug.Log($"Scene {sceneInstance.Scene.name} loaded successfully. Current active scene: {SceneManager.GetActiveScene().name}");

            if (sceneInstance.Scene == SceneManager.GetActiveScene())
            {
                // Optional delay before raising the event
                await UniTask.Delay(1000); // Adjust this delay as needed
                TriggerNextSetupStep?.Raise();
                Debug.Log("Hello from AddressableLoadingManager, TriggerNextSetupStep raised");
            }
        };

        await sceneLoader.LoadSceneAsync(sceneToLoad, handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("Scene loaded successfully.");
                finished = true;
                OnAddressablesReady?.Invoke();
            }
            else
            {
                Debug.LogError("Failed to load scene: " + handle.DebugName);
            }
        });
    }


    // Add your methods and properties here for managing addressables
}