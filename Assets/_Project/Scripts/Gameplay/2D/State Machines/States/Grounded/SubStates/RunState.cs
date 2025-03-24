using UnityEngine;

public class RunState : State
{
    public AnimationClip anim;
    public override void Enter()
    {
        animator.Play(anim.name);
        animator.speed = 1;
    }
    public override void Do()
    {
        var velX = body.linearVelocityX;

        //animator.speed = Mathf.Abs(velX);

        // var time = Helpers.Map(velX, input.stats.maxXSpeed, -input.stats.maxXSpeed, 0,1,false);
        // animator.Play(anim.name,0,time);
        // animator.speed = time;

        if (!input.grounded)
        {
            isComplete = true;
        }
    }
    public override void FixedDo()
    {
        base.FixedDo();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
