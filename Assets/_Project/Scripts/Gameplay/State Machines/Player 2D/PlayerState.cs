using Unity.VisualScripting;
using UnityEngine;

namespace _Project.Scripts.Gameplay.State_Machines.Player_2D
{
    public abstract class PlayerState
    {
        public abstract void EnterState(PlayerStateMachine stateMachine);
        public abstract void UpdateState(PlayerStateMachine stateMachine);
        public abstract void ExitState(PlayerStateMachine stateMachine);
    }
}
