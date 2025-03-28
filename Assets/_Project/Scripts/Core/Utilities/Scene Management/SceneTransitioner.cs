using _Project.Scripts.Core.Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _Project.Scripts.Core.Utilities.Scene_Management
{
    public class SceneTransitioner : MonoBehaviour
    {
        [SerializeField] AssetReference SceneToTransitionTo;
    
        public void TranstionToScene()
        {
            SceneTransitionManager.Instance.TransitionScene(SceneToTransitionTo);
        }
    }
}
