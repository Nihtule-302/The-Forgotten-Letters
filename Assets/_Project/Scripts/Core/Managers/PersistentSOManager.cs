using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Collections.Generic;

public class PersistentSOManager : MonoBehaviour
{
    public static PersistentSOManager Instance { get; private set; }

    [System.Serializable]
    public class SOReference
    {
        public AssetReferenceT<ScriptableObject> reference;
    }

    public List<SOReference> persistentSOList = new List<SOReference>(); // Assign in Inspector
    private Dictionary<Type, ScriptableObject> loadedSOs = new Dictionary<Type, ScriptableObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllPersistentSOs();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadAllPersistentSOs()
    {
        foreach (var soRef in persistentSOList)
        {
            soRef.reference.LoadAssetAsync<ScriptableObject>().Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    ScriptableObject loadedSO = handle.Result;
                    loadedSOs[loadedSO.GetType()] = loadedSO;
                    Debug.Log($"Loaded SO: {loadedSO.GetType().Name}");
                }
                else
                {
                    Debug.LogError($"Failed to load SO: {soRef.reference.RuntimeKey}");
                }
            };
        }
    }

    public static T GetSO<T>() where T : ScriptableObject
    {
        if (Instance.loadedSOs.TryGetValue(typeof(T), out ScriptableObject so))
        {
            return so as T;
        }
        Debug.LogWarning($"SO of type '{typeof(T).Name}' not found.");
        return null;
    }
}
