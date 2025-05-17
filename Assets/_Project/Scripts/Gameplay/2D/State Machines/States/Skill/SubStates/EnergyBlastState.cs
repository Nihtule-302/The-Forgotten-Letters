using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.StateMachine;
using TheForgottenLetters;
using UnityEngine;

public class EnergyBlastState : State
{
    private PlayerAbilityStats playerAbilityStats => PersistentSOManager.GetSO<PlayerAbilityStats>();
    [SerializeField] private Skill level1Skill;
    [SerializeField] private Skill level2Skill;
    [SerializeField] private Skill currentSkill;
    public Transform firePoint;

    public override void Enter()
    {
        if(playerAbilityStats.playerSkills.UnlockedSkills_names.Contains(level2Skill.name))
        {
            currentSkill = level2Skill;
        }
        else if(playerAbilityStats.playerSkills.UnlockedSkills_names.Contains(level1Skill.name))
        {
            currentSkill = level1Skill;
        }
        else
        {
            Debug.LogError("No skill unlocked for Energy Blast State.");
            isComplete = true;
            return;
        }
    }
    public override void Do()
    {
        Shoot();
        isComplete = true;
    }
    public override void Exit(){}

    public void Shoot()
    {
        Instantiate(currentSkill.skillPrefab, firePoint.position, firePoint.rotation);
    }
}
