using System.Collections.Generic;
using TheForgottenLetters;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Player/Skill")]
public class Skill : ScriptableObject
{
    [Header("Skill Info")]
    public Sprite Icon;
    public string skillDescription;
    public int levelNeeded = 1;
    public int energyPointsNeeded = 5;
    public int skillDamage = 1;

    [Header("Skill Prefab")]
    public GameObject skillPrefab;

    public bool CanUnlockSkill(PlayerAbilityStats playerAbilityStats)
    {
        if (playerAbilityStats.EnergyPoints < energyPointsNeeded)
        {
            return false;
        }
        return true;
    }

    public bool IsSkillUnlocked(PlayerAbilityStats playerAbilityStats)
    {
       return playerAbilityStats.unlockedSkills.Contains(this);
    }

    

    public bool UnlockSkill(PlayerAbilityStats playerAbilityStats)
    {
        if (CanUnlockSkill(playerAbilityStats) && !IsSkillUnlocked(playerAbilityStats))
        {
            playerAbilityStats.AddSkill(this);
            playerAbilityStats.EnergyPoints -= energyPointsNeeded;
            return true;
        }
        return false;
    }
}
