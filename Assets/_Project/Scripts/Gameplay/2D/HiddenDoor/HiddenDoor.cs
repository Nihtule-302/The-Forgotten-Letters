using _Project.Scripts.Gameplay.Mini_Games.Puzzles;
using UnityEngine;

namespace _Project.Scripts.Gameplay._2D.HiddenDoor
{
    public class HiddenDoor : IInteractable
    {
        [SerializeField] private float lockedPositionY;
        [SerializeField] private float unlockedPositionY;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float delay = 0.5f;

        [SerializeField] private InGameDrawLetterUIManager uiManager;
        public bool IsUnlocked { get; private set; }

        public override void Interact()
        {
            if (!IsInteractable()) return;

            uiManager.ShowDrawLetterPuzzle();
        }

        public override bool IsInteractable()
        {
            return !IsUnlocked;
        }

        public void UnlockDoor()
        {
            IsUnlocked = true;
            uiManager.HideDrawLetterPuzzle();
            uiManager.DeactivateInteractivity();
            MoveDoor();
        }

        private void MoveDoor()
        {
            LeanTween.moveLocalY(gameObject, unlockedPositionY, moveSpeed)
                .setDelay(delay)
                .setOnComplete(() =>
                {
                    IsUnlocked = true; // Set the door to unlocked after moving
                    Debug.Log("Door is now unlocked and moved to the unlocked position.");
                });
        }
    }
}