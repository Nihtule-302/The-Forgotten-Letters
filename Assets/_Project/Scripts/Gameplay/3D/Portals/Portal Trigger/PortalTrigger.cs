using UnityEngine;

namespace _Project.Scripts.Gameplay._3D.Portals.Portal_Trigger
{
    public class PortalTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            PortalActivator portal = other.GetComponent<PortalActivator>();
            if (portal != null)
            {
                portal.RiseUp();
            }
        }
    }
}
