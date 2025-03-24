using UnityEngine;

public class IdleState : State
{
    public AnimationClip anim;
    public override void Enter()
    {
        animator.Play(anim.name);
    }
    public override void Do()
    {
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
