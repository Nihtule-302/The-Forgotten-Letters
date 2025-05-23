using _Project.Scripts.StateMachine;
using UnityEngine;

namespace _Project.Scripts.Gameplay.AI
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
            if (Vector2.Distance(Core.transform.position, destination) < thershold) IsComplete = true;
            FaceDestination();
        }

        public override void FixedDo()
        {
            var direction = (destination - (Vector2)Core.transform.position).normalized;
            Body.linearVelocity = new Vector2(direction.x * speed, Body.linearVelocityY);
        }

        private void FaceDestination()
        {
            Core.transform.localScale = new Vector3(Mathf.Sign(Body.linearVelocityX), 1, 1);
        }
    }
}