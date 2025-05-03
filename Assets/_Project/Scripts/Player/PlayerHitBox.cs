using _Project.Scripts.Core.Scriptable_Events;
using UnityEngine;

namespace TheForgottenLetters
{
    public class PlayerHitBox : MonoBehaviour
    {
        public GameEvent playerTakeDamage;
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy"))
            {
                playerTakeDamage.Raise();
            }
        }
    }
}
