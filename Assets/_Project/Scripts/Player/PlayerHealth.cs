using _Project.Scripts.Core.Scriptable_Events;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerHealth", menuName = "Scriptable Objects/PlayerHealth")]
public class PlayerHealth : ScriptableObject
{
    public int maxHealth = 3;
    [SerializeField] private int currentHealth;
    public int CurrentHealth => currentHealth;

    public GameEvent playerDeath;
    public GameEvent updateHealthUI;

    private void OnEnable()
    {
      currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
      currentHealth -= damage;
      updateHealthUI.Raise();
      if (currentHealth <= 0)
      {
        playerDeath.Raise();
      }
    }

    public void ResetHealth()
    {
      currentHealth = maxHealth;
      updateHealthUI.Raise();
    }
}
