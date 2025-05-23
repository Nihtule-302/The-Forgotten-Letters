using _Project.Scripts.Core.Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _Project.Scripts.Core.Utilities.Scene_Management
{
    public class SceneTransitioner : MonoBehaviour
    {
        [SerializeField] private AssetReference SceneToTransitionTo;

        public void TranstionToScene()
        {
            SceneTransitionManager.Instance.TransitionScene(SceneToTransitionTo);
        }

        public void TranstionToScene(AssetReference chossenScene)
        {
            SceneTransitionManager.Instance.TransitionScene(chossenScene);
        }
    }
}