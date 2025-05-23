using System.Collections;
using UnityEngine;

namespace _Project.Scripts.Player
{
    public class PlayerHealthManager : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private SpriteRenderer characterSpriteRenderer;
        [SerializeField] private float flashDuration = 0.2f;

        [ContextMenu("Take Damage")]
        public void TakeDamage()
        {
            playerHealth.TakeDamage(1);
        }

        public void Die()
        {
            Debug.Log("Player has died.");
        }

        public void FlashRed()
        {
            StartCoroutine(ChangeColor());
        }

        private IEnumerator ChangeColor()
        {
            characterSpriteRenderer.color = Color.red;
            yield return new WaitForSeconds(flashDuration);
            characterSpriteRenderer.color = Color.white;
        }

        [ContextMenu("Reset Health")]
        public void Heal()
        {
            playerHealth.ResetHealth();
        }
    }
}