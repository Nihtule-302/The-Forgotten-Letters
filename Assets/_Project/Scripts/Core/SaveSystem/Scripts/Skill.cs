using System.Collections.Generic;
using _Project.Scripts.Core.Managers;
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
        if (PersistentSOManager.GetSO<PlayerAbilityStats>().energyPoints < energyPointsNeeded)
        {
            return false;
        }
        return true;
    }

    public bool IsSkillUnlocked(PlayerAbilityStats playerAbilityStats)
    {
       return PersistentSOManager.GetSO<PlayerAbilityStats>().playerSkills.UnlockedSkills.Contains(this);
    }

    

    public bool UnlockSkill(PlayerAbilityStats playerAbilityStats)
    {
        if (CanUnlockSkill(playerAbilityStats) && !IsSkillUnlocked(playerAbilityStats))
        {
            var databuilder = PersistentSOManager.GetSO<PlayerAbilityStats>().GetBuilder();

            databuilder
                .DecrementEnergyPoints(energyPointsNeeded)
                .AddSkill(this)
                .SetlastTimeEnergyIncreased();
            
            PersistentSOManager.GetSO<PlayerAbilityStats>().UpdateData(databuilder);
            return true;
        }
        return false;
    }

    
}