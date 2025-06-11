using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SentisModelLoader : MonoBehaviour
{
    [SerializeField] private List<AssetReference> sentisModelReferences;

    private readonly Dictionary<object, ModelAsset> loadedModels = new();
    private readonly Dictionary<object, AsyncOperationHandle> modelHandles = new();
    public static SentisModelLoader Instance { get; private set; }
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
        LoadAllModelsAsync().Forget();
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            foreach (var kvp in loadedModels) Addressables.Release(kvp.Value);
            loadedModels.Clear();
            Instance = null;
        }
    }

    public event Action OnAllModelsLoaded;

    public async UniTask LoadAllModelsAsync()
    {
        if (sentisModelReferences.Count == 0)
        {
            OnAllModelsLoaded?.Invoke();
            return;
        }

        List<UniTask> loadTasks = new List<UniTask>();
        foreach (var assetRef in sentisModelReferences)
        {
            loadTasks.Add(LoadModelAsync(assetRef));
        }

        // Wait for all tasks to complete with a timeout
        await UniTask.WhenAll(loadTasks).Timeout(TimeSpan.FromSeconds(30));

        OnAllModelsLoaded?.Invoke();
        Debug.Log("All models loaded successfully.");
    }

    // Load a specific model on demand
    public async UniTask LoadModelAsync(AssetReference reference) 
    {
        if (reference == null)
        {
            Debug.LogWarning("Tried to load a null reference.");
            return;
        }

        if (IsModelLoaded(reference))
        {
            Debug.Log($"Model already loaded: {reference.RuntimeKey}");
            return;
        }

        object key = reference.RuntimeKey;

        var handle = reference.LoadAssetAsync<ModelAsset>();
        await handle.ToUniTask();

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            loadedModels[key] = handle.Result;
            modelHandles[key] = handle;
            Debug.Log($"Loaded model on demand: {key}");
            return; // Return after success
        }

        Debug.LogError($"Failed to load model: {key}");
    }

    public void UnloadModel(AssetReference reference)
    {
        if (!IsModelLoaded(reference))
        {
            Debug.LogWarning($"Model not loaded for reference: {reference.RuntimeKey}");
            return;
        }

        if (loadedModels.TryGetValue(reference.RuntimeKey, out var model))
        {
            Addressables.Release(model);
            loadedModels.Remove(reference.RuntimeKey);
        }

        if (modelHandles.TryGetValue(reference.RuntimeKey, out var handle))
        {
            Addressables.Release(handle);
            modelHandles.Remove(reference.RuntimeKey);
        }
    }

    public ModelAsset GetModel(AssetReference reference)
    {
        if (loadedModels.TryGetValue(reference.RuntimeKey, out ModelAsset model)) return model;

        Debug.LogWarning($"Model not found for reference: {reference.RuntimeKey}");
        return null;
    }

    public bool IsModelLoaded(AssetReference reference)
    {
        return loadedModels.ContainsKey(reference.RuntimeKey);
    }
}
