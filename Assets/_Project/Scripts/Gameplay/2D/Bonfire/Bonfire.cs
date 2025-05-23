using _Project.Scripts.Core.Scriptable_Events;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _Project.Scripts.Gameplay._2D.Bonfire
{
    public class Bonfire : IInteractable
    {
        [SerializeField] private AssetReference bonfireRestEventRef;
        [SerializeField] private AssetReference bonfireStopRestEventRef;

        private GameEvent BonfireRestEvent => EventLoader.Instance.GetEvent<GameEvent>(bonfireRestEventRef);
        private GameEvent BonfireStopRestEvent => EventLoader.Instance.GetEvent<GameEvent>(bonfireStopRestEventRef);

        public bool IsPlayerResting { get; private set; }

        public override bool IsInteractable()
        {
            return !IsPlayerResting;
        }

        public override void Interact()
        {
            if (!IsInteractable()) return;

            RestAtBonfire();
        }

        public void RestAtBonfire()
        {
            IsPlayerResting = true;
            BonfireRestEvent.Raise(); // Notify that the player is resting at the bonfire
        }

        public void StopResting()
        {
            IsPlayerResting = false;
            BonfireStopRestEvent.Raise(); // Notify that the player has stopped resting at the bonfire
        }
    }
}