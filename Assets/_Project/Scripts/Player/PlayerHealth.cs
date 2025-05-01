using _Project.Scripts.Core.Scriptable_Events;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerHealth", menuName = "Scriptable Objects/PlayerHealth")]
public class PlayerHealth : ScriptableObject
{
    public int maxHealth = 3;
    private int currentHealth;
    public int CurrentHealth => currentHealth;

    public GameEvent playerDeath;
    public GameEvent playerTakeDamage;

    private void OnEnable()
    {
      currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
      currentHealth -= damage;

      playerTakeDamage.Raise();
      if (currentHealth <= 0)
      {
        playerDeath.Raise();
      }
    }
}
