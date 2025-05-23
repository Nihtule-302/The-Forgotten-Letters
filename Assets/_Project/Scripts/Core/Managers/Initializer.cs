using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _Project.Scripts.Core.Managers
{
    public class Initializer : MonoBehaviour
    {
        [SerializeField] private AssetReference sceneToLoad;

        private void Awake()
        {
            Addressables.LoadSceneAsync(sceneToLoad);
        }
    }
}