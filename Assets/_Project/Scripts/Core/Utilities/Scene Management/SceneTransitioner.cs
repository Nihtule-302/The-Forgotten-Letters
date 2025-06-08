using _Project.Scripts.Core.Managers;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _Project.Scripts.Core.Utilities.Scene_Management
{
    public class SceneTransitioner : MonoBehaviour
    {
        [SerializeField] private AssetReference SceneToTransitionTo;

        public void TranstionToScene()
        {
            TranstionToSceneAsync(SceneToTransitionTo).Forget(); // Optional, for non-await usage
        }

        public void TranstionToScene(AssetReference chossenScene)
        {
            TranstionToSceneAsync(chossenScene).Forget(); // Optional, for non-await usage
        }

        public async UniTask TranstionToSceneAsync()
        {
            await SceneTransitionManager.Instance.TransitionSceneAsync(SceneToTransitionTo);
        }

        public async UniTask TranstionToSceneAsync(AssetReference chossenScene)
        {
            await SceneTransitionManager.Instance.TransitionSceneAsync(chossenScene);
        }
    }
}