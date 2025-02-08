using Unity.VisualScripting;
using UnityEngine;

namespace _Project.Scripts.Gameplay.State_Machines.Player_2D
{
    public abstract class Player2DState
    {
        public abstract void EnterState(Player2DStateMachine stateMachine);
        public abstract void UpdateState(Player2DStateMachine stateMachine);
        public abstract void ExitState(Player2DStateMachine stateMachine);
    }
}
