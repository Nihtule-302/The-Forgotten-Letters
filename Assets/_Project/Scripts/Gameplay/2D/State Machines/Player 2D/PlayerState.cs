using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.Player_2D
{
    public abstract class PlayerState
    {
        public bool isComplete {get; protected set;}
        protected float startTime;

        public float time => Time.time - startTime;

        protected PlayerStateMachine machine;
        protected PlayerController player;
        protected PlayerStateFactory states;
        protected PlayerState currentChildState;
        protected PlayerState currentParentState;

        protected string stateName;

        public PlayerState(PlayerStateMachine stateMachine, PlayerController player, PlayerStateFactory playerStateFactory)
        {
            machine = stateMachine;
            this.player = player;
            states = playerStateFactory;
        }
        
        public abstract void Enter();
        public abstract void Do();
        public abstract void FixedDo();
        public abstract void InitializeChildrenStates();
        public abstract void Exit();

        public void Dos()
        {
            Do();
            currentChildState?.Dos();
        }

        public void FixedDos()
        {
            FixedDo();
            currentChildState?.FixedDos();
        }

        protected void SwitchState(PlayerState newState)
        {
            machine.ChangeState(newState);
        }

        protected void SetChildState(PlayerState childState)
        {
            currentChildState = childState;
            currentChildState.SetParentState(this);
        }
        protected void SetParentState(PlayerState parentState)
        {
            currentParentState = parentState;
        }
        public override string ToString()
        {
            return stateName;
        }
    }
}
