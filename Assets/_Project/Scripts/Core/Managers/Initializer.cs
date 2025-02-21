using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Core.Managers
{
    public class Initializer : MonoBehaviour
    {
        [SerializeField] private AssetReference sceneToLoad;

        void Awake()
        {
            Addressables.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single, activateOnLoad:true);
        }
    }
}

