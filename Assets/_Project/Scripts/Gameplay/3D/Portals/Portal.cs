using System;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace _Project.Scripts.Gameplay.Portals
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

