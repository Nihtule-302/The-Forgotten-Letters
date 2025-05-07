using System.Collections.Generic;
using _Project.Scripts.Core.StateMachine;
using _Project.Scripts.Gameplay._2D.State_Machines.States.Grounded.SubStates;
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
            if (machine.state == navigate)
            {
                if (CloseEnough(target.position))
                {
                    SetChild(idle, true);
                    // target.gameObject.SetActive(false);
                    body.linearVelocity = new Vector2(0, body.linearVelocityY);
                }
                else if (!InVision(target.position))
                {
                    body.linearVelocity = new Vector2(0, body.linearVelocityY);
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
                if (machine.state.time > 2)
                {
                    isComplete = true;
                }
            }

            if (target == null)
            {
                isComplete = true;
                return;
            }
        }

        public override void Exit()
        {
            base.Exit();
        }

        public bool CloseEnough(Vector2 targetPos)
        {
            return Vector2.Distance(core.transform.position, targetPos) < collectRadius;
        }

        public bool InVision(Vector2 targetPos)
        {
            return Vector2.Distance(core.transform.position, targetPos) < vision;
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
