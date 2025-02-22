using _Project.Scripts.Gameplay.Portals;
using UnityEngine;

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
