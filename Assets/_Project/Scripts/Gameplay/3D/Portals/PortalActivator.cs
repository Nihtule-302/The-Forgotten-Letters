using UnityEngine;

namespace _Project.Scripts.Gameplay._3D.Portals
{
    public class PortalActivator : MonoBehaviour
    {
        public float riseDistance = 2f;    // How high the portal will rise
        public float riseDuration = 1f;    // Duration of the rise animation

        public void RiseUp()
        {
            LeanTween.cancel(gameObject);
            // Animate the portal's Y-position upward.
            transform.LeanMoveLocalY(transform.position.y + riseDistance, riseDuration).setEase(LeanTweenType.easeOutQuad);
        
        }
    }
}
