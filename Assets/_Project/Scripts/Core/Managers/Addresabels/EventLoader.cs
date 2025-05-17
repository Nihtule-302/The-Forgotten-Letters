using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EventLoader : MonoBehaviour
{
    public static EventLoader Instance { get; private set; }

    [SerializeField]
    private List<AssetReference> eventReferences;

    [ShowInInspector, ReadOnly]
    private Dictionary<object, ScriptableObject> loadedEvents = new Dictionary<object, ScriptableObject>();


    public event Action OnAllEventsLoaded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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
        if (loadedEvents.TryGetValue(reference.RuntimeKey, out var so))
        {
            return so as T;
        }

        Debug.LogWarning($"Event not found for reference: {reference.RuntimeKey}");
        return null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            foreach (var kvp in loadedEvents)
            {
                Addressables.Release(kvp.Value);
            }
            loadedEvents.Clear();
            Instance = null;
        }
    }
}
