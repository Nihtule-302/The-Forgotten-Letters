using _Project.Scripts.Core.Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SceneTransitioner : MonoBehaviour
{
    [SerializeField] AssetReference SceneToTransitionTo;
    
    public void TranstionToScene()
    {
        SceneTransitionManager.Instance.TransitionScene(SceneToTransitionTo);
    }
}
