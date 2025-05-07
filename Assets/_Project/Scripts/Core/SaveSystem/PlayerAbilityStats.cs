using System.Collections.Generic;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Scriptable_Events;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
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
            // SavePlayerData().Forget();
        }
        public void AddEnergyPoints(int amount = 1)
        {
            EnergyPoints += amount;
            // SavePlayerData().Forget();
        }

        public void SetEnergyPoints(int value)
        {
            energyPoints = value;
            if (energyPoints < 0)
            {
                energyPoints = 0;
            }
            playerAbilityStatsChangedEvent.Raise();
            //  SavePlayerData().Forget();
        }

        public void AddSkill(Skill skill)
        {
            if (!unlockedSkills.Contains(skill))
            {
                unlockedSkills.Add(skill);
                playerAbilityStatsChangedEvent.Raise();
                //  SavePlayerData().Forget();
            }
        }

        public void RemoveSkill(Skill skill)
        {
            if (unlockedSkills.Contains(skill))
            {
                unlockedSkills.Remove(skill);
                playerAbilityStatsChangedEvent.Raise();
                //  SavePlayerData().Forget();
            }
        }

        private void OnValidate()
        {
            playerAbilityStatsChangedEvent.Raise();
            //  SavePlayerData().Forget();
        }

        public void Reset()
        {
            energyPoints = 0;
            unlockedSkills.Clear();
            playerAbilityStatsChangedEvent.Raise();
            //  SavePlayerData().Forget();
        }

        private async UniTaskVoid SavePlayerData()
        {
            await FirebaseManager.Instance.SavePlayerData(PersistentSOManager.GetSO<PlayerAbilityStats>());
        }
    }

    [FirestoreData]
    public class PlayerAbilityStatsSerializable
    {
        [FirestoreProperty]
        public int energyPoints { get; set; }

        [FirestoreProperty]
        public List<SkillSerializable> unlockedSkills { get; set; }

        // ✅ Default constructor required for Firestore
        public PlayerAbilityStatsSerializable()
        {
            energyPoints = 0;
            unlockedSkills = new List<SkillSerializable>();
        }

        // ✅ Constructor for conversion from LetterHuntData
        public PlayerAbilityStatsSerializable(PlayerAbilityStats data)
        {
            energyPoints = data.EnergyPoints;
            unlockedSkills = new List<SkillSerializable>();
            foreach (var skill in data.unlockedSkills)
            {
                unlockedSkills.Add(new SkillSerializable(skill));
            }
        }
    }
}
