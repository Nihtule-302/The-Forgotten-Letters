using _Project.Scripts.Core.Scriptable_Events;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _Project.Scripts.Player
{
    public class PlayerHitBox : MonoBehaviour
    {
        [SerializeField] private AssetReference playerTakeDamageRef;
        public GameEvent PlayerTakeDamage => EventLoader.Instance.GetEvent<GameEvent>(playerTakeDamageRef);

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy")) PlayerTakeDamage.Raise();
        }
    }
}