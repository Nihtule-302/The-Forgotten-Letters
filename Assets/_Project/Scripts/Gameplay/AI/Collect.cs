using _Project.Scripts.Gameplay._2D.State_Machines.States.PlayerGroundControl.SubStates;
using _Project.Scripts.StateMachine;
using UnityEngine;

namespace _Project.Scripts.Gameplay.AI
{
    public class Collect : State
    {
        public Transform player;
        public Transform target;
        public Navigate navigate;

        public IdleState idle;
        public float collectRadius;

        public float vision = 1;

        public override void Enter()
        {
            navigate.destination = target.position;
            SetChild(navigate, true);
        }

        public override void Do()
        {
            if (Machine.State == navigate)
            {
                if (CloseEnough(target.position))
                {
                    SetChild(idle, true);
                    // target.gameObject.SetActive(false);
                    Body.linearVelocity = new Vector2(0, Body.linearVelocityY);
                }
                else if (!InVision(target.position))
                {
                    Body.linearVelocity = new Vector2(0, Body.linearVelocityY);
                    SetChild(idle, true);
                }
                else
                {
                    navigate.destination = target.position;
                    SetChild(navigate, true);
                }
            }
            else
            {
                if (Machine.State.Time > 2) IsComplete = true;
            }

            if (target == null) IsComplete = true;
        }

        public override void Exit()
        {
            base.Exit();
        }

        public bool CloseEnough(Vector2 targetPos)
        {
            return Vector2.Distance(Core.transform.position, targetPos) < collectRadius;
        }

        public bool InVision(Vector2 targetPos)
        {
            return Vector2.Distance(Core.transform.position, targetPos) < vision;
        }

        public void CheckForTarget()
        {
            if (InVision(player.position) && player.gameObject.activeSelf)
            {
                target = player;
                return;
            }

            target = null;
        }
    }
}