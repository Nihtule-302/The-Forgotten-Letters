using _Project.Scripts.Core.StateMachine;
using _Project.Scripts.Core.Utilities;
using UnityEngine;

public class JumpState : State
{
    public AnimationClip anim;
    public float jumpSpeed;
    public float airGravity;

    public override void Enter()
    {
        body.gravityScale = airGravity;
    }
    public override void Do()
    {
        var time = Helpers.Map(body.linearVelocityY, jumpSpeed, 0, 0,1,true);
        animator.Play(anim.name,0,time);
        animator.speed = 0;
        if (body.linearVelocityY < 0)
        {
            isComplete = true;
        }
    }
    public override void Exit()
    {
    }
}
