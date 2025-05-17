using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace _Project.Scripts.Core.Managers
{
    public class PersistentSOManager : MonoBehaviour
    {
        public static PersistentSOManager Instance { get; private set; }
        public event Action OnAllSOsLoaded; // Completion event

    

        [Tooltip("Assign persistent ScriptableObjects here")]
        public List<SOReference> persistentSOList = new List<SOReference>();

        private ConcurrentDictionary<Type, ScriptableObject> loadedSOs = new ConcurrentDictionary<Type, ScriptableObject>();
        private List<AsyncOperationHandle> activeHandles = new List<AsyncOperationHandle>();
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
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        var loadedSO = op.Result;
                        var soType = loadedSO.GetType();

                        if (loadedSOs.TryAdd(soType, loadedSO)) // ✅ Thread-safe add
                        {
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

                    // ✅ Thread-safe counter update
                    if (Interlocked.Increment(ref completedLoads) == totalLoads)
                    {
                        OnAllSOsLoaded?.Invoke();
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

            if (Instance.loadedSOs.TryGetValue(typeof(T), out ScriptableObject so))
            {
                return so as T;
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
    }

    [System.Serializable]
    public class SOReference
    {
        public AssetReferenceT<ScriptableObject> reference;
    }
}