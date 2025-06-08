using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EventLoader : MonoBehaviour
{
    [SerializeField] private List<AssetReference> eventReferences;

    [ShowInInspector] [ReadOnly] private Dictionary<object, ScriptableObject> loadedEvents = new();

    public static EventLoader Instance { get; private set; }
    public static event Action OnInitialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        OnInitialized?.Invoke();
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        try
        {
            await LoadAllEventsAsync();
            Debug.Log("All events loaded successfully.");
            OnAllEventsLoaded?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load all events: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            foreach (var kvp in loadedEvents) Addressables.Release(kvp.Value);
            loadedEvents.Clear();
            Instance = null;
        }
    }


    public event Action OnAllEventsLoaded;


    public async Task LoadAllEventsAsync()
    {
        if (eventReferences.Count == 0)
        {
            OnAllEventsLoaded?.Invoke();
            return;
        }

        foreach (var assetRef in eventReferences)
        {
            if (assetRef == null)
            {
                Debug.LogWarning("Null AssetReference found in eventReferences list.");
                continue;
            }

            var handle = assetRef.LoadAssetAsync<ScriptableObject>();
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedEvents[assetRef.RuntimeKey] = handle.Result;
                Debug.Log($"Loaded event: {assetRef.RuntimeKey}");
            }
            else
            {
                Debug.LogError($"Failed to load asset: {assetRef.RuntimeKey}");
            }
        }
    }

    public T GetEvent<T>(AssetReference reference) where T : ScriptableObject
    {
        if (loadedEvents.TryGetValue(reference.RuntimeKey, out var so)) return so as T;

        Debug.LogWarning($"Event not found for reference: {reference.RuntimeKey}");
        return null;
    }
}