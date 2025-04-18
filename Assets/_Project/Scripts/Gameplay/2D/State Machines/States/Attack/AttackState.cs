using _Project.Scripts.Core.StateMachine;
using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.States.Attack
{
    public class AttackState : State
    {
        public AnimationClip anim;
        public float animationSpeed = 1f;
        AnimatorStateInfo stateInfo;
        public override void Enter()
        {
            animator.Play(anim.name);
            animator.speed = animationSpeed;
            
        }
        public override void Do()
        {
            stateInfo = animator.GetCurrentAnimatorStateInfo(0); // 0 is the layer index
            if (stateInfo.IsName(anim.name) && stateInfo.normalizedTime >= 1f)
            {
                isComplete = true;
            }
        }
        public override void Exit(){}
    }
}
