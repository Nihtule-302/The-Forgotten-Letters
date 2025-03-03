using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.State_Machines.Player_2D
{
    public class PlayerStateMachine : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private TextMeshProUGUI stateText;

        public PlayerController Player => player;

        PlayerStateFactory states;

        public PlayerState CurrentState { get; private set; }
        
        void Awake()
        {
            player = GetComponent<PlayerController>();

            states = new PlayerStateFactory(this);
        }

        void Start()
        {
            InitializeState();
        }

        public void InitializeState()
        {
            if (player.grounded)
            {
                CurrentState = states.Idle();
            }
            else
            {
                CurrentState = states.Falling();
            }
            CurrentState.Enter();
        }

        public void ChangeState(PlayerState newState)
        {
            //Debug.Log($"State: {newState.GetType()}");
            CurrentState.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }

        private void Update()
        {
            CurrentState.Dos();
            stateText.SetText($"State: {CurrentState.ToString()}");
        }

        private void FixedUpdate()
        {
            CurrentState.FixedDos();
        }
    }
}
