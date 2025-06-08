using _Project.Scripts.Core.Scriptable_Events;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AddresableLoaderAndUnloader : MonoBehaviour
{
    [SerializeField] private AssetReference assetReference;
    [SerializeField] private GameObject _instance;

    [ContextMenu("Load Asset")]
    public async UniTask LoadAssetAsync()
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(assetReference);
        await handle.Task;

        if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            _instance = Instantiate(handle.Result);
        }
        else
        {
            Debug.LogError("Failed to load asset: " + handle.OperationException);
        }
    }

    public async UniTaskVoid LoadAssetAsync(AssetReference reference)
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(reference);
        await handle.Task;

        if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
        {
            _instance = Instantiate(handle.Result);
        }
        else
        {
            Debug.LogError("Failed to load asset: " + handle.OperationException);
        }
    }

    [ContextMenu("Unload Asset")]
    public void UnloadAsset()
    {
        UnloadAssetAsync().Forget();
    }

    public void UnloadAsset(GameObject instance, AssetReference reference)
    {
       UnloadAssetAsync(instance, reference).Forget();
    }

    public async UniTask UnloadAssetAsync()
    {
        if (_instance != null)
        {
            Destroy(_instance);
            _instance = null;
        }

        var _currentHandle = Addressables.LoadAssetAsync<GameObject>(assetReference);
        await _currentHandle.Task;

        Addressables.Release(_currentHandle);
    }

    public async UniTask UnloadAssetAsync(GameObject instance, AssetReference reference)
    {
        if (instance != null)
        {
            Destroy(instance);
        }

        var _currentHandle = Addressables.LoadAssetAsync<GameObject>(reference);
        await _currentHandle.Task;

        Addressables.Release(_currentHandle);
    }
}
