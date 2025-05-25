using System.Collections.Generic;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Scriptable_Events;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TheForgottenLetters
{
    [CreateAssetMenu(fileName = "PlayerAbilityStats", menuName = "Player/Player Ability Stats")]
    public class PlayerAbilityStats : ScriptableObject
    {
        public int energyPoints;
        [SerializeField] private PlayerHealth playerHealth;

        public string lastTimeEnergyIncreasedUTC;

        public AssetReference playerAbilityStatsChangedEventRef;
        public PlayerSkills playerSkills => PersistentSOManager.GetSO<PlayerSkills>();

        public GameEvent playerAbilityStatsChangedEvent =>
            EventLoader.Instance.GetEvent<GameEvent>(playerAbilityStatsChangedEventRef);

        [ContextMenu("Reset Data")]
        public void ResetData()
        {
            energyPoints = 0;
            lastTimeEnergyIncreasedUTC = string.Empty;
            playerSkills.ResetData();
            FinalizeUpdate();
        }

        public void UpdateData(PlayerAbilityStatsDataBuilder builder)
        {
            energyPoints = builder.energyPoints;
            lastTimeEnergyIncreasedUTC = builder.lastTimeEnergyIncreasedUTC;
            playerSkills.UpdateData(builder.unlockedSkills);
            FinalizeUpdate();
        }

        public void UpdateData(PlayerAbilityStatsDataSerializable playerAbilityStatsData)
        {
            energyPoints = playerAbilityStatsData.energyPoints;
            lastTimeEnergyIncreasedUTC = playerAbilityStatsData.lastTimeEnergyIncreasedUTC;
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
            // if (SavePlayerDataAsync().Status == UniTaskStatus.Pending)
            //     return;
            SavePlayerDataAsync().Forget();
            playerAbilityStatsChangedEvent.Raise();
        }

        private async UniTask SavePlayerDataAsync()
        {
            await FirebaseManager.Instance.SavePlayerData(PersistentSOManager.GetSO<PlayerAbilityStats>());
        }
    }

    [FirestoreData]
    public class PlayerAbilityStatsDataSerializable
    {

        [FirestoreProperty] public int energyPoints { get; set; }

        [FirestoreProperty] public string lastTimeEnergyIncreasedUTC { get; set; }

        [FirestoreProperty] public List<string> unlockedSkillNames { get; set; } = new();
        
        // ✅ Default constructor required for Firestore
        public PlayerAbilityStatsDataSerializable()
        {
            energyPoints = 0;
            unlockedSkillNames = new List<string>();
            lastTimeEnergyIncreasedUTC = string.Empty;
        }

        // ✅ Constructor for conversion from LetterHuntData
        public PlayerAbilityStatsDataSerializable(PlayerAbilityStats data)
        {
            energyPoints = data.energyPoints;
            unlockedSkillNames = data.playerSkills.UnlockedSkills_names;
            lastTimeEnergyIncreasedUTC = data.lastTimeEnergyIncreasedUTC;
        }

    }
}