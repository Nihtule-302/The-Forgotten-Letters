using System;
using UnityEngine;

namespace TheForgottenLetters
{
    public class HiddenDoor : IInteractable
    {
        [SerializeField] private float lockedPositionY;
        [SerializeField] private float unlockedPositionY;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float delay = 0.5f;
        public bool isUnlocked {get; private set; }

        [SerializeField] InGameDrawLetterUIManager uiManager;

        public override void Interact()
        {
            if (!IsInteractable()) return;

            uiManager.ShowDrawLetterPuzzle();
        }

        public override bool IsInteractable()
        {
            return !isUnlocked;
        }

        public void UnlockDoor()
        {
            isUnlocked = true;
            uiManager.HideDrawLetterPuzzle(); 
            uiManager.DeactivateInteractivity();
            MoveDoor();
        }

        private void MoveDoor()
        {
            LeanTween.moveLocalY(gameObject,unlockedPositionY, moveSpeed)
                .setDelay(delay)
                .setOnComplete(() =>
                {
                    isUnlocked = true; // Set the door to unlocked after moving
                    Debug.Log("Door is now unlocked and moved to the unlocked position.");
                });
        }

        
    }
}
