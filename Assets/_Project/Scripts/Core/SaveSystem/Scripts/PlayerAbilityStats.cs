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
        // ========== Public Fields ==========
        public int energyPoints;
        public string lastTimeEnergyIncreasedUTC;

        [SerializeField] private PlayerHealth playerHealth;

        public AssetReference playerAbilityStatsChangedEventRef;

        // ========== Cached References ==========
        public PlayerSkills playerSkills => PersistentSOManager.GetSO<PlayerSkills>();

        public GameEvent playerAbilityStatsChangedEvent =>
            EventLoader.Instance.GetEvent<GameEvent>(playerAbilityStatsChangedEventRef);

        // ========== Context Menu Actions ==========
        [ContextMenu("Reset and Save Data To Firebase")]
        public void ResetAndSaveDataToFirebase()
        {
            ResetData();
            SaveData();
        }

        [ContextMenu("Reset Data")]
        public void ResetData()
        {
            energyPoints = 0;
            lastTimeEnergyIncreasedUTC = string.Empty;
            playerSkills.ResetData();
        }

        [ContextMenu("Save Data")]
        private void SaveData()
        {
            SaveDataAsync().Forget();
        }

        // ========== Async Save ==========
        private async UniTask SaveDataAsync()
        {
            var builder = GetBuilder();
            if (builder == null)
            {
                Debug.LogError("PlayerAbilityStatsDataBuilder is null.");
                return;
            }
            await builder.SaveDataToFirebaseAsync();
        }

        // ========== Data Update Methods ==========
        public void UpdateLocalData(PlayerAbilityStatsDataBuilder builder)
        {
            energyPoints = builder.energyPoints;
            lastTimeEnergyIncreasedUTC = builder.lastTimeEnergyIncreasedUTC;
            playerSkills.UpdateLocalData(builder.unlockedSkills);
            playerAbilityStatsChangedEvent.Raise();
        }

        public void UpdateLocalData(PlayerAbilityStatsDataSerializable data)
        {
            energyPoints = data.energyPoints;
            lastTimeEnergyIncreasedUTC = data.lastTimeEnergyIncreasedUTC;
            playerSkills.UpdateLocalData(data.unlockedSkillNames);
            playerAbilityStatsChangedEvent.Raise();
        }

        // ========== Builder Getter ==========
        public PlayerAbilityStatsDataBuilder GetBuilder()
        {
            return new PlayerAbilityStatsDataBuilder(this);
        }

        // ========== Utility Methods ==========
        [ContextMenu("Add Energy Point")]
        public void AddEnergyPoint()
        {
            GetBuilder()
                .IncrementEnergyPoints(1)
                .UpdateLocalData()
                .SaveDataToFirebase();
        }
    }

    [FirestoreData]
    public class PlayerAbilityStatsDataSerializable
    {
        [FirestoreProperty] public int energyPoints { get; set; }
        [FirestoreProperty] public string lastTimeEnergyIncreasedUTC { get; set; }
        [FirestoreProperty] public List<string> unlockedSkillNames { get; set; } = new();

        // Default constructor required for Firestore
        public PlayerAbilityStatsDataSerializable()
        {
            energyPoints = 0;
            lastTimeEnergyIncreasedUTC = string.Empty;
            unlockedSkillNames = new List<string>();
        }

        // Constructor for conversion from PlayerAbilityStats
        public PlayerAbilityStatsDataSerializable(PlayerAbilityStats data)
        {
            energyPoints = data.energyPoints;
            unlockedSkillNames = data.playerSkills.UnlockedSkills_names;
            lastTimeEnergyIncreasedUTC = data.lastTimeEnergyIncreasedUTC;
        }
    }
}
