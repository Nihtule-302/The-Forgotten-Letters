using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PersistentSOManager : MonoBehaviour
{
    public static PersistentSOManager Instance { get; private set; }
    public static event Action OnAllSOsLoaded; // Completion event

    [System.Serializable]
    public class SOReference
    {
        public AssetReferenceT<ScriptableObject> reference;
    }

    [Tooltip("Assign persistent ScriptableObjects here")]
    public List<SOReference> persistentSOList = new List<SOReference>();

    private Dictionary<Type, ScriptableObject> loadedSOs = new Dictionary<Type, ScriptableObject>();
    private List<AsyncOperationHandle> activeHandles = new List<AsyncOperationHandle>();
    private readonly object dictLock = new object();
    private int totalLoads;
    private int completedLoads;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ValidateAndLoad();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ValidateAndLoad()
    {
#if UNITY_EDITOR
        RunEditorValidations();
#endif

        RemoveNullAndDuplicateReferences();
        LoadAllPersistentSOs();
    }

    private void LoadAllPersistentSOs()
    {
        if (persistentSOList.Count == 0)
        {
            OnAllSOsLoaded?.Invoke();
            return;
        }

        totalLoads = persistentSOList.Count;
        foreach (var soRef in persistentSOList)
        {
            var handle = soRef.reference.LoadAssetAsync<ScriptableObject>();
            activeHandles.Add(handle);

            handle.Completed += op =>
            {
                lock (dictLock)
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        var loadedSO = op.Result;
                        var soType = loadedSO.GetType();

                        if (!loadedSOs.ContainsKey(soType))
                        {
                            loadedSOs.Add(soType, loadedSO);
                            Debug.Log($"Successfully loaded: {soType.Name}");
                        }
                        else
                        {
                            Debug.LogError($"Duplicate type detected: {soType.Name}. " +
                                           "Only one SO per type allowed.");
                        }
                    }
                    else
                    {
                        Debug.LogError($"Failed to load: {soRef.reference.RuntimeKey}");
                    }

                    completedLoads++;
                    if (completedLoads == totalLoads)
                    {
                        OnAllSOsLoaded?.Invoke();
                    }
                }
            };
        }
    }

    public static T GetSO<T>() where T : ScriptableObject
    {
        if (Instance == null)
        {
            Debug.LogError("PersistentSOManager instance not found!");
            return null;
        }

        lock (Instance.dictLock)
        {
            if (Instance.loadedSOs.TryGetValue(typeof(T), out ScriptableObject so))
            {
                return so as T;
            }
        }

        Debug.LogWarning($"SO of type '{typeof(T).Name}' not found. " +
                       "Did you forget to add it to PersistentSOManager?");
        return null;
    }

    private void RemoveNullAndDuplicateReferences()
    {
        // Remove null entries
        persistentSOList = persistentSOList
            .Where(x => x.reference != null)
            .ToList();

        // Remove duplicates by RuntimeKey
        persistentSOList = persistentSOList
            .GroupBy(x => x.reference.RuntimeKey)
            .Select(g => g.First())
            .ToList();
    }

    private void OnDestroy()
    {
        foreach (var handle in activeHandles)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        loadedSOs.Clear();
        activeHandles.Clear();
    }

#if UNITY_EDITOR
    private void RunEditorValidations()
    {
        var missingRefs = persistentSOList
            .Where(x => x.reference == null)
            .ToList();

        if (missingRefs.Count > 0)
        {
            Debug.LogError("PersistentSOManager: Found null references in persistentSOList!");
        }

        // Type collision check
        var tempTypeList = new List<Type>();
        foreach (var soRef in persistentSOList.Where(x => x.reference != null))
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(soRef.reference.AssetGUID);
            var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

            if (asset == null)
            {
                Debug.LogError($"Invalid reference in persistentSOList: {soRef.reference.AssetGUID}");
                continue;
            }

            var assetType = asset.GetType();
            if (tempTypeList.Contains(assetType))
            {
                Debug.LogError($"Duplicate type detected in editor: {assetType.Name}. " +
                             "Only one SO per type allowed!");
            }
            tempTypeList.Add(assetType);
        }
    }
#endif
}