using System.Collections.Generic;
using _Project.Scripts.Core.Managers;
using UnityEngine;

namespace _Project.Scripts.Gameplay._3D.Portals.Portal_Line
{
    public class PathOfDoors : MonoBehaviour
    {
        [SerializeField] private PortalLineSettings portalLineSettings;
        [SerializeField] Vector3 rotation;

        [SerializeField] float totalElapsedTime;
        [SerializeField] float initialRaiseInterval;
        [SerializeField] private InputManagerSO input;

        private List<GameObject> portals = new List<GameObject>();

        void Start()
        {
            EnablePlayerInput();
            InitializePortalArray();
        }

        private void EnablePlayerInput()
        {
            input?.EnablePlayerActions();
        }

        private void InitializePortalArray()
        {
            // Instantiate the portals and position them in a line.
            for (int i = 0; i < portalLineSettings.portalCount; i++)
            {
                Vector3 spawnPosition = new Vector3(transform.position.x, portalLineSettings.spawnYPosition, transform.position.z + portalLineSettings.spawnZPosition + i * portalLineSettings.spacingBetweenPortals);
                GameObject portal = Instantiate(portalLineSettings.portalPrefab, spawnPosition, Quaternion.Euler(rotation), transform);
                
                portals.Add(portal);
            }
        }

        void Update()
        {
            HandleInput();
            HandlePortalLooping();
            RaiseNextPortal();
        }

        private void HandleInput()
        {
            HandleRightLeftInput();
            HandleForwardBackwardInput();
        }

        private void HandleRightLeftInput()
        {
        }

        private void HandleForwardBackwardInput()
        {
        }

        private void HandlePortalLooping()
        {
        }

        private void RaiseNextPortal()
        {
            int nextPortalIndex = Mathf.FloorToInt(totalElapsedTime) % portals.Count;  // Loop through portals cyclically
            portals[nextPortalIndex].GetComponent<PortalActivator>().RiseUp();
        }
    }
}

// Start:
//   Initialize an array to store the doors.
//   Set the number of doors, distance from the player, and spacing between doors.
//   Instantiate the doors and add them to the array.
  
//   For each door:
//     Calculate position:
//       - If the index is odd, place the door to the right.
//       - If the index is even, place the door to the left.

//   Set a loop threshold to control when the doors reset.
//   Initialize a timer for the door-raising sequence.
  
// Main Loop (Update):
//   Handle player input:
//     - If input is left/right, move the parent object.
//     - If input is forward/backward, move all the doors forward/backward.

//   Loop the doors:
//     - If a door moves past the loop threshold, reset its position to the start.

//   Door Raising Logic:
//     - Increment the door-raising timer.
//     - If the timer exceeds the current interval:
//       - Call the raise method on the next door in the array.
//       - Reduce the interval to increase speed.
//       - Reset the timer.