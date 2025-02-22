using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Gameplay._3D.Portals
{
    public class InfinitePortalLine : MonoBehaviour
    {
        [SerializeField] private PortalLineSettings portalLineSettings;
        [SerializeField] Vector3 rotation;

        private List<GameObject> portals = new List<GameObject>();

        void Start()
        {
            // Instantiate the portals and position them in a line.
            for (int i = 0; i < portalLineSettings.portalCount; i++)
            {
                Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, portalLineSettings.spawnZPosition + i * portalLineSettings.spacingBetweenPortals);
                GameObject portal = Instantiate(portalLineSettings.portalPrefab, spawnPosition, Quaternion.Euler(rotation), transform);
                
                portals.Add(portal);
            }
        }

        void Update()
        {
           
        }
    }
}
