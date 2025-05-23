using _Project.Scripts.Core.Managers;
using _Project.Scripts.StateMachine;
using TheForgottenLetters;
using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.States.Skill.SubStates
{
    public class EnergyBlastState : State
    {
        [SerializeField] private global::Skill level1Skill;
        [SerializeField] private global::Skill level2Skill;
        [SerializeField] private global::Skill currentSkill;
        public Transform firePoint;
        private PlayerAbilityStats PlayerAbilityStats => PersistentSOManager.GetSO<PlayerAbilityStats>();

        public override void Enter()
        {
            if (PlayerAbilityStats.playerSkills.UnlockedSkills_names.Contains(level2Skill.name))
            {
                currentSkill = level2Skill;
            }
            else if (PlayerAbilityStats.playerSkills.UnlockedSkills_names.Contains(level1Skill.name))
            {
                currentSkill = level1Skill;
            }
            else
            {
                Debug.LogError("No skill unlocked for Energy Blast State.");
                IsComplete = true;
            }
        }

        public override void Do()
        {
            Shoot();
            IsComplete = true;
        }

        public override void Exit()
        {
        }

        public void Shoot()
        {
            Instantiate(currentSkill.skillPrefab, firePoint.position, firePoint.rotation);
        }
    }
}