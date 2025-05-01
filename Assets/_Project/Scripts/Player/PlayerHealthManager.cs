using UnityEngine;

public class PlayerHealthManager : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;

    [ContextMenu("Take Damage")]
    public void TakeDamage()
    {
        playerHealth.TakeDamage(1);
    }
    public void Die()
    {
        Debug.Log("Player has died.");
    }
}
