using _Project.Scripts.Core.Managers;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _Project.Scripts.Gameplay._3D.Portals
{
    public class Portal : MonoBehaviour
    {
        [SerializeField] private AssetReference sceneToTravelTo;

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                SceneTransitionManager.Instance.TransitionScene(sceneToTravelTo);
            }
        }
    }
}

