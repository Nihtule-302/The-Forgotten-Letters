using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace _Project.Scripts.Gameplay.Portals
{
    public class Portal : MonoBehaviour
    {
        [SerializeField] private AssetReference sceneToTravelTo;


        void OnCollisionEnter(Collision other)
        {
            Debug.Log($"Collision!!!!!!!!!! {other.gameObject.name}");
            if (other.gameObject.CompareTag("Player"))
            {
                SceneTransitionManager.Instance.TransitionScene(sceneToTravelTo);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            Debug.Log($"Trigger!!!!!!!!!! {other.gameObject.name}");
            if (other.gameObject.CompareTag("Player"))
            {
                SceneTransitionManager.Instance.TransitionScene(sceneToTravelTo);
            }
        }
    }
}

