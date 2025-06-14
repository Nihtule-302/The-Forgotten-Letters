using System;
using System.Collections.Generic;
using System.Globalization;
using _Project.Scripts.Core.Scriptable_Events.EventTypes.Float;
using TheForgottenLetters;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _Project.Scripts.Core.Managers
{
    public class SkillManager : MonoBehaviour
    {
        public List<Skill> allSkills = new();
        private readonly Dictionary<string, Skill> skillLookup = new();

        public static SkillManager Instance { get; private set; }

        [SerializeField] private float decayIntervalMinutes = 1f; // Time before skill decay
        [SerializeField] private double decayProgressRatio = 0f;

        [SerializeField] private float decayCheckCooldown = 60f; // Check every X seconds
        private float decayCheckTimerElapsed = 0f;

        [Header("Events")]
        [SerializeField] private FloatEvent decayProgressEvent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildSkillDictionary();
        }

        private void BuildSkillDictionary()
        {
            skillLookup.Clear();
            foreach (var skill in allSkills)
            {
                if (!skillLookup.ContainsKey(skill.name))
                    skillLookup.Add(skill.name, skill);
                else
                    Debug.LogWarning($"Duplicate skill name found: {skill.name}");
            }
        }

        public Skill GetSkillByName(string skillName)
        {
            if (skillLookup.TryGetValue(skillName, out var skill))
                return skill;

            Debug.LogWarning($"Skill with name '{skillName}' not found.");
            return null;
        }

        public List<Skill> ConvertNamesToSkills(List<string> skillNames)
        {
            var result = new List<Skill>();
            foreach (var name in skillNames)
            {
                var skill = GetSkillByName(name);
                if (skill != null)
                    result.Add(skill);
            }
            return result;
        }

        private void Update()
        {
            decayCheckTimerElapsed += Time.deltaTime;

            if (decayCheckTimerElapsed >= decayCheckCooldown)
            {
                decayProgressEvent.Raise((float)decayProgressRatio);
                decayCheckTimerElapsed = 0f;
                ApplySkillDecayIfDue();

            }
        }

        private void ApplySkillDecayIfDue()
        {
            string isoFormat = "o";
            string lastUtcTimestamp = PersistentSOManager.GetSO<PlayerAbilityStats>().lastTimeEnergyIncreasedUTC;

            if (PersistentSOManager.GetSO<PlayerAbilityStats>().playerSkills.UnlockedSkills_names.Count <= 0)
            {
                return;
            }


            if (DateTime.TryParseExact(lastUtcTimestamp, isoFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime lastTimeUtc))
            {
                lastTimeUtc = lastTimeUtc.ToUniversalTime();
                DateTime currentUtc = DateTime.UtcNow;
                TimeSpan timeSinceLastIncrease = currentUtc - lastTimeUtc;

                // Handle negative time (stored time is in the future)
                if (timeSinceLastIncrease < TimeSpan.Zero)
                {
                    decayProgressRatio = 0;
                    Debug.Log($"decayProgressRatio: {decayProgressRatio}");
                    return;
                }

                TimeSpan decayInterval = TimeSpan.FromMinutes(decayIntervalMinutes);
                decayProgressRatio = Math.Clamp(timeSinceLastIncrease.TotalMinutes / decayInterval.TotalMinutes, 0, 1);

                if (timeSinceLastIncrease >= decayInterval)
                {
                    var playerAbilityStatsBuilder = PersistentSOManager.GetSO<PlayerAbilityStats>().GetBuilder();
                    playerAbilityStatsBuilder
                        .RemoveLatestSkill()
                        .SetlastTimeEnergyIncreased()
                        .UpdateLocalData()
                        .SaveDataToFirebase();
                    decayProgressRatio = 0f;
                }
            }
            else
            {
                Debug.LogWarning("Failed to parse lastTimeEnergyIncreasedUTC.");
            }
        }
    }
}
