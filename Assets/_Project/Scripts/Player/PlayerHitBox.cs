using _Project.Scripts.Core.Scriptable_Events;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TheForgottenLetters
{
    public class PlayerHitBox : MonoBehaviour
    {
        [SerializeField] private AssetReference playerTakeDamageRef;
        public GameEvent playerTakeDamage => EventLoader.Instance.GetEvent<GameEvent>(playerTakeDamageRef);
        
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy"))
            {
                playerTakeDamage.Raise();
            }
        }
    }
}
