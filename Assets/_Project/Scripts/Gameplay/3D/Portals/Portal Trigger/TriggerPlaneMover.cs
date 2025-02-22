using UnityEngine;

public class TriggerPlaneMover : MonoBehaviour
{
    public Transform startPoint;
    public Transform targetPoint;
    public float duration = 10f;

    public LeanTweenType easeType;

    void Awake()
    {
        startPoint = transform;
    }

    void Start()
    {
        // Move the trigger plane from startPosition to targetPosition over 'duration' seconds.
        transform.position = startPoint.position;
        LeanTween.move(gameObject, targetPoint, duration)
                 .setEase(easeType);
    }
}
