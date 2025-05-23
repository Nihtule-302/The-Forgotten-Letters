using UnityEngine;

namespace _Project.Scripts.Gameplay._3D.Portals.Portal_Trigger
{
    public class TriggerPlaneMover : MonoBehaviour
    {
        public Transform startPoint;
        public Transform targetPoint;
        public float duration = 10f;

        public LeanTweenType easeType;

        private void Awake()
        {
            startPoint = transform;
        }

        private void Start()
        {
            // Move the trigger plane from startPosition to targetPosition over 'duration' seconds.
            transform.position = startPoint.position;
            LeanTween.move(gameObject, targetPoint, duration)
                .setEase(easeType);
        }
    }
}