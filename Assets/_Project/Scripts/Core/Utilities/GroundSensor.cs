using UnityEngine;

namespace _Project.Scripts.Core.Utilities
{
    public class GroundSensor : MonoBehaviour
    {
        public BoxCollider2D groundCheck;
        public LayerMask groundMask;
        public bool grounded;

        void FixedUpdate()
        {
            CheckGround();
        }

        private void CheckGround()
        {
            grounded = Physics2D.OverlapAreaAll(groundCheck.bounds.min, groundCheck.bounds.max, groundMask).Length > 0;
        }
    }
}
