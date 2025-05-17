using _Project.Scripts.Core.Scriptable_Events;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "PlayerHealth", menuName = "Player/Health")]
public class PlayerHealth : ScriptableObject
{
    public int maxHealth = 3;
    [SerializeField] private int currentHealth;
    public int CurrentHealth => currentHealth;

    public AssetReference playerDeathRef;
    public AssetReference updateHealthUIRef;

    public GameEvent playerDeath => EventLoader.Instance.GetEvent<GameEvent>(playerDeathRef);
    public GameEvent updateHealthUI => EventLoader.Instance.GetEvent<GameEvent>(updateHealthUIRef);

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
