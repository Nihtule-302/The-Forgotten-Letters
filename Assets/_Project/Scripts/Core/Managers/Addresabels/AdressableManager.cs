using System;
using System.Collections;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Utilities.Scene_Management;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class AdressableManager : MonoBehaviour
{
    public static AdressableManager Instance { get; private set; }

    public event Action OnAddressablesReady;

    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreen;

    [Header("Scene to Load")]
    [SerializeField] private AssetReference sceneToLoad;

    [Header("Are Assets Loaded")]
    public bool areEventsReady = false;
    // public bool areModelsReady = false;
    public bool areLogicalSOsReady = false;

    SceneLoader sceneLoader;
    private bool finished = false;
    private bool transitioning;

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

    void Start()
    {
        EventLoader.Instance.OnAllEventsLoaded += OnEventsReady;
        // SentisModelLoader.Instance.OnAllModelsLoaded += OnModelsReady;
        PersistentSOManager.Instance.OnAllSOsLoaded += OnLogicalSOsReady;
        loadingScreen.SetActive(true);
        transitioning = false;
    }
    void Update()
    {
        if(finished)
        {
            return;
        }
        if (transitioning)
        {
            return;
        }
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
        Debug.Log("All addressables are ready. Loading scene: " + sceneToLoad.RuntimeKey.ToString());

        StartCoroutine(LoadSceneAfterDelay());
        OnAddressablesReady?.Invoke();
        loadingScreen.SetActive(false);
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        sceneLoader.LoadScene(sceneToLoad, handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("Scene loaded successfully.");
                finished = true;
            }
            else
            {
                Debug.LogError("Failed to load scene: " + handle.DebugName);
            }
        });
    }


    // Add your methods and properties here for managing addressables
}
