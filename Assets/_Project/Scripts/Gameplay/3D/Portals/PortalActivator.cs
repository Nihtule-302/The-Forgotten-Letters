    using Sirenix.OdinInspector;
using UnityEngine;


public class PortalActivator : MonoBehaviour
{
    public float riseDistance = 2f;    // How high the portal will rise
    public float riseDuration = 1f;    // Duration of the rise animation

    [Button]
    public void RiseUp()
    {
        LeanTween.cancel(gameObject);
        // Animate the portal's Y-position upward.
        transform.LeanMoveLocalY(transform.position.y + riseDistance, riseDuration).setEase(LeanTweenType.easeOutQuad);
        
    }
}
