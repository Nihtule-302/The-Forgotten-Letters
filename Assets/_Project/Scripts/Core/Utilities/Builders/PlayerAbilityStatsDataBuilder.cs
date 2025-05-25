using System;
using System.Collections.Generic;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Utilities;

namespace TheForgottenLetters
{
    public class PlayerAbilityStatsDataBuilder
    {
        public int energyPoints { get; private set; }
        public List<string> unlockedSkills { get; private set; }
        public string lastTimeEnergyIncreasedUTC { get; private set; }

        private PlayerSkills playerSkills => PersistentSOManager.GetSO<PlayerSkills>();


        public PlayerAbilityStatsDataBuilder()
        {
            energyPoints = 0;
            lastTimeEnergyIncreasedUTC = string.Empty;
            unlockedSkills = new List<string>();
        }

        public PlayerAbilityStatsDataBuilder(PlayerAbilityStats existingData)
        {
            energyPoints = existingData.energyPoints;
            lastTimeEnergyIncreasedUTC = existingData.lastTimeEnergyIncreasedUTC;
            unlockedSkills = new List<string>(playerSkills.UnlockedSkills_names);
        }

        public PlayerAbilityStatsDataBuilder SetEnergyPoints(int points)
        {
            energyPoints = points;
            return this;
        }

        public PlayerAbilityStatsDataBuilder IncrementEnergyPoints(int amount = 1)
        {
            energyPoints += amount;
            return this;
        }

        public PlayerAbilityStatsDataBuilder DecrementEnergyPoints(int amount = 1)
        {
            energyPoints -= amount;
            return this;
        }

        public PlayerAbilityStatsDataBuilder SetlastTimeEnergyIncreased()
        {
            var timeStamp = TimeHelpers.GetCurrentUtcIsoTimestamp();

            lastTimeEnergyIncreasedUTC = timeStamp;

            return this;
        }

        public PlayerAbilityStatsDataBuilder SetlastTimeEnergyIncreased(string cairoTime)
        {
            lastTimeEnergyIncreasedUTC = cairoTime;
            return this;
        }

        public PlayerAbilityStatsDataBuilder AddSkill(string skillName)
        {
            if (!playerSkills.UnlockedSkills_names.Contains(skillName))
            {
                playerSkills.UnlockedSkills_names.Add(skillName);
                playerSkills.LoadSkillsFromNames();
                unlockedSkills = playerSkills.UnlockedSkills_names;
            }

            return this;
        }

        public PlayerAbilityStatsDataBuilder AddSkill(Skill skill)
        {
            if (!playerSkills.UnlockedSkills.Contains(skill))
            {
                playerSkills.UnlockedSkills.Add(skill);
                playerSkills.SaveSkillsToNames();
                unlockedSkills = playerSkills.UnlockedSkills_names;
            }

            return this;
        }

        public PlayerAbilityStatsDataBuilder RemoveSkill(string skillName)
        {
            if (playerSkills.UnlockedSkills_names.Contains(skillName))
            {
                playerSkills.UnlockedSkills_names.Remove(skillName);
                playerSkills.LoadSkillsFromNames();
                unlockedSkills = playerSkills.UnlockedSkills_names;
            }

            return this;
        }

        public PlayerAbilityStatsDataBuilder RemoveLatestSkill()
        {
            if (playerSkills.UnlockedSkills_names.Count > 0)
            {
                playerSkills.UnlockedSkills_names.RemoveAt(playerSkills.UnlockedSkills_names.Count - 1);
                playerSkills.LoadSkillsFromNames();
                unlockedSkills = playerSkills.UnlockedSkills_names;
            }

            return this;
        }

        public PlayerAbilityStatsDataBuilder RemoveSkill(Skill skill)
        {
            if (playerSkills.UnlockedSkills.Contains(skill))
            {
                playerSkills.UnlockedSkills.Remove(skill);
                playerSkills.SaveSkillsToNames();
                unlockedSkills = playerSkills.UnlockedSkills_names;
            }

            return this;
        }

        public PlayerAbilityStatsDataBuilder SetSkills(List<string> skills)
        {
            playerSkills.UnlockedSkills_names = skills;
            playerSkills.LoadSkillsFromNames();
            unlockedSkills = playerSkills.UnlockedSkills_names;
            return this;
        }
    }
}