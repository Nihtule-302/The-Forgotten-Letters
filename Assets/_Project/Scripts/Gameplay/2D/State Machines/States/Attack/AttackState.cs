using _Project.Scripts.StateMachine;
using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.States.Attack
{
    public class AttackState : State
    {
        public AnimationClip anim;
        public float animationSpeed = 1f;
        private AnimatorStateInfo _stateInfo;

        public override void Enter()
        {
            Animator.Play(anim.name);
            Animator.speed = animationSpeed;
        }

        public override void Do()
        {
            _stateInfo = Animator.GetCurrentAnimatorStateInfo(0); // 0 is the layer index
            if (_stateInfo.IsName(anim.name) && _stateInfo.normalizedTime >= 1f) IsComplete = true;
        }

        public override void Exit()
        {
        }
    }
}