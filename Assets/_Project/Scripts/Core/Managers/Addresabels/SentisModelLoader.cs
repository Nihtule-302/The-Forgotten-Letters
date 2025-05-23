using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SentisModelLoader : MonoBehaviour
{
    [SerializeField] private List<AssetReference> sentisModelReferences;

    private readonly Dictionary<AssetReference, ScriptableObject> loadedModels = new();
    public static SentisModelLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAllModelsAsync().ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                Debug.LogError("Failed to load all models.");
            }
            else
            {
                Debug.Log("All models loaded successfully.");
                OnAllModelsLoaded?.Invoke();
            }
        });
    }

    public event Action OnAllModelsLoaded;

    public async Task LoadAllModelsAsync()
    {
        if (sentisModelReferences.Count == 0 || sentisModelReferences == null)
        {
            Debug.LogWarning("No Sentis models to load.");
            OnAllModelsLoaded?.Invoke();
            return;
        }

        foreach (var assetRef in sentisModelReferences)
        {
            var handle = assetRef.LoadAssetAsync<ScriptableObject>();
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
                loadedModels[assetRef] = handle.Result;
            else
                Debug.LogError($"Failed to load asset: {assetRef.RuntimeKey}");
        }
    }

    public T GetModel<T>(AssetReference reference) where T : ScriptableObject
    {
        if (loadedModels.TryGetValue(reference, out var so)) return so as T;

        Debug.LogWarning($"Model not found for reference: {reference.RuntimeKey}");
        return null;
    }
}