using System.Collections.Generic;
using Firebase.Firestore;
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

[FirestoreData]
public class SkillSerializable
{
    [FirestoreProperty]
    public string skillName { get; set; }

    // ✅ Default constructor required for Firestore
    public SkillSerializable()
    {
        skillName = string.Empty;
    }

    // ✅ Constructor for conversion from LetterHuntData
    public SkillSerializable(Skill data)
    {
        skillName = data.name;
    }
}