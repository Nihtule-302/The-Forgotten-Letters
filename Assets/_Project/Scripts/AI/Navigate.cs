using _Project.Scripts.Core.StateMachine;
using UnityEngine;

namespace _Project.Scripts.AI
{
    public class Navigate : State
    {
        public Vector2 destination;
        public float speed = 1;
        public float thershold = 0.1f;
        public State animation;

        public override void Enter()
        {
            SetChild(animation, true);
        }

        public override void Do()
        {
            if (Vector2.Distance(core.transform.position, destination) < thershold)
            {
                isComplete = true;
            }
            FaceDestination();
        }

        public override void FixedDo()
        {
            var direction = (destination - (Vector2)core.transform.position).normalized;
            body.linearVelocity = new(direction.x * speed, body.linearVelocityY);
        }

        private void FaceDestination()
        {
            core.transform.localScale = new(Mathf.Sign(body.linearVelocityX), 1, 1);
        }
    }
}
