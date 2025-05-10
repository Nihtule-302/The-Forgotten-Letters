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
        public int energyPoints = 0;
        [SerializeField] private PlayerHealth playerHealth;
        public PlayerSkills playerSkills => PersistentSOManager.GetSO<PlayerSkills>();

        public string lastTimeEnergyIncreasedCairoTime;

        public GameEvent playerAbilityStatsChangedEvent;

        [ContextMenu("Reset Data")]
        public void ResetData()
        {
            energyPoints = 0;
            lastTimeEnergyIncreasedCairoTime = string.Empty;
            playerSkills.ResetData();
            FinalizeUpdate();
        }

        public void UpdateData(PlayerAbilityStatsDataBuilder builder)
        {
            energyPoints = builder.energyPoints;
            lastTimeEnergyIncreasedCairoTime = builder.lastTimeEnergyIncreasedCairoTime;
            playerSkills.UpdateData(builder.unlockedSkills);
            FinalizeUpdate();
        }

        public void UpdateData(PlayerAbilityStatsDataSerializable playerAbilityStatsData)
        {
            energyPoints = playerAbilityStatsData.energyPoints;
            lastTimeEnergyIncreasedCairoTime = playerAbilityStatsData.lastTimeEnergyIncreasedCairoTime;
            playerSkills.UpdateData(playerAbilityStatsData.unlockedSkillNames);
            FinalizeUpdate();
        }

        public PlayerAbilityStatsDataBuilder GetBuilder()
        {
            return new PlayerAbilityStatsDataBuilder(this);
        }

        [ContextMenu("Add Energy Points")]
        public void AddEnergyPoint()
        {
            energyPoints += 1;
            FinalizeUpdate();
        }

        private void FinalizeUpdate()
        {
            SavePlayerDataAsync().Forget();
            playerAbilityStatsChangedEvent.Raise();
        }

        private async UniTaskVoid SavePlayerDataAsync()
        {
            await FirebaseManager.Instance.SavePlayerData(PersistentSOManager.GetSO<PlayerAbilityStats>());
        }
    }

    [FirestoreData]
    public class PlayerAbilityStatsDataSerializable
    {
        [FirestoreProperty]
        public int energyPoints { get; set; }

        [FirestoreProperty]
        public string lastTimeEnergyIncreasedCairoTime { get; set; }

        [FirestoreProperty]
        public List<string> unlockedSkillNames { get; set; } = new List<string>();

        // ✅ Default constructor required for Firestore
        public PlayerAbilityStatsDataSerializable()
        {
            energyPoints = 0;
            unlockedSkillNames = new List<string>();
            lastTimeEnergyIncreasedCairoTime = string.Empty;
        }

        // ✅ Constructor for conversion from LetterHuntData
        public PlayerAbilityStatsDataSerializable(PlayerAbilityStats data)
        {
            energyPoints = data.energyPoints;
            unlockedSkillNames = data.playerSkills.UnlockedSkills_names;
            lastTimeEnergyIncreasedCairoTime = data.lastTimeEnergyIncreasedCairoTime;
        }
        
    }

}


