using UnityEngine;

public class AirState : State
{
    public AnimationClip anim;
    public override void Enter()
    {
        animator.Play(anim.name);
    }
    public override void Do()
    {
        var time = Helpers.Map(body.linearVelocityY, input.stats.jumpSpeed, -input.stats.jumpSpeed, 0,1,true);
        animator.Play(anim.name,0,time);
        animator.speed = 0;
        if (input.grounded)
        {
            isComplete = true;
        }
    }
    public override void Exit()
    {

    }
}
