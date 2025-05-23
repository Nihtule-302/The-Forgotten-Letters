using _Project.Scripts.Core.Managers;
using TheForgottenLetters;
using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Player/Skill")]
public class Skill : ScriptableObject
{
    [Header("Skill Info")] public Sprite Icon;

    public string skillDescription;
    public int levelNeeded = 1;
    public int energyPointsNeeded = 5;
    public int skillDamage = 1;

    [Header("Skill Prefab")] public GameObject skillPrefab;

    public bool CanUnlockSkill()
    {
        if (PersistentSOManager.GetSO<PlayerAbilityStats>().energyPoints < energyPointsNeeded) return false;
        return true;
    }

    public bool IsSkillUnlocked()
    {
        return PersistentSOManager.GetSO<PlayerAbilityStats>().playerSkills.UnlockedSkills_names.Contains(name);
    }


    public bool UnlockSkill()
    {
        if (CanUnlockSkill() && !IsSkillUnlocked())
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