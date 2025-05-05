using System.Collections.Generic;
using _Project.Scripts.Core.Scriptable_Events;
using UnityEngine;

namespace TheForgottenLetters
{
    [CreateAssetMenu(fileName = "PlayerAbilityStats", menuName = "Player/Player Ability Stats")]
    public class PlayerAbilityStats : ScriptableObject
    {
        [Header("Player Ability Stats")]
        [SerializeField] private int energyPoints = 0;
        public int EnergyPoints 
        {
            get => energyPoints; 
            set => SetEnergyPoints(value);
        }

       
        [Header("Player Health Scriptable Object")]
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Skills Enabled")]
        public List<Skill> unlockedSkills = new List<Skill>();

        public GameEvent playerAbilityStatsChangedEvent;

        [ContextMenu("Add Energy Points")]
        public void AddEnergyPoint()
        {
            EnergyPoints += 1;
        }
        public void AddEnergyPoints(int amount = 1)
        {
            EnergyPoints += amount;
        }

        private void SetEnergyPoints(int value)
        {
            energyPoints = value;
            if (energyPoints < 0)
            {
                energyPoints = 0;
            }
            playerAbilityStatsChangedEvent.Raise();
        }

        public void AddSkill(Skill skill)
        {
            if (!unlockedSkills.Contains(skill))
            {
                unlockedSkills.Add(skill);
                playerAbilityStatsChangedEvent.Raise();
            }
        }

        public void RemoveSkill(Skill skill)
        {
            if (unlockedSkills.Contains(skill))
            {
                unlockedSkills.Remove(skill);
                playerAbilityStatsChangedEvent.Raise();
            }
        }

        private void OnValidate()
        {
            playerAbilityStatsChangedEvent.Raise();
        }

    }
}
