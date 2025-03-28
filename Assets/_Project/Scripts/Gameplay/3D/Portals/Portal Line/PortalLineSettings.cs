using UnityEngine;

namespace _Project.Scripts.Gameplay._3D.Portals.Portal_Line
{
    [CreateAssetMenu(menuName = "The Forgotten Letters/PortalLineSettings")]
    public class PortalLineSettings : ScriptableObject
    {
        public GameObject portalPrefab;      
        public int portalCount = 10;         
        public float spacingBetweenPortals = 2f;           
        public float movementSpeed = 2f;       
        public float resetZPosition = -10f; 
        public float spawnZPosition = 0f; 
        public float spawnYPosition = 0f;
    }
}

