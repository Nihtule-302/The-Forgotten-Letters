using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Mini_Games.Puzzles
{
    public class InGameDrawLetterUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject trigger;
        [SerializeField] private GameObject visualCue;

        [SerializeField] private GameObject drawLetterPuzzelScreen;
        [SerializeField] private GameObject game2Dui;

        private void Awake()
        {
            drawLetterPuzzelScreen.SetActive(false);
        }

        public void ShowDrawLetterPuzzle()
        {
            drawLetterPuzzelScreen.SetActive(true);
            game2Dui.SetActive(false);
        }

        public void HideDrawLetterPuzzle()
        {
            drawLetterPuzzelScreen.SetActive(false);
            game2Dui.SetActive(true);
        }

        public void DeactivateInteractivity()
        {
            trigger.SetActive(false);
            visualCue.SetActive(false);
        }
    }
}