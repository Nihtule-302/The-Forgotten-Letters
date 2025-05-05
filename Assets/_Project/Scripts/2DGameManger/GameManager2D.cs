using _Project.Scripts.UI;
using UnityEngine;

namespace TheForgottenLetters
{
    public class GameManager2D : MonoBehaviour
    {
        [SerializeField] private GameObject HUD;
        [SerializeField] private GameObject onScreenControls;
        [SerializeField] private PauseMenu pauseMenu;
        [SerializeField] private GameObject gameOverMenu;
        [SerializeField] private GameObject meditationMenu;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            pauseMenu.StartMenu();
            HUD.SetActive(true);
            onScreenControls.SetActive(true);
            gameOverMenu.SetActive(false);
            meditationMenu.SetActive(false);
        }


        public void GameOver()
        {
            HUD.SetActive(false);
            onScreenControls.SetActive(false);
            gameOverMenu.SetActive(true);
            meditationMenu.SetActive(false);

            Time.timeScale = 0f; // Pause the game
        }
        public void MeditationMenu()
        {
            HUD.SetActive(false);
            onScreenControls.SetActive(false);
            gameOverMenu.SetActive(false);
            meditationMenu.SetActive(true);
        }
        public void ResumeGame()
        {
            HUD.SetActive(true);
            onScreenControls.SetActive(true);
            gameOverMenu.SetActive(false);
            meditationMenu.SetActive(false);
            

            Time.timeScale = 1f; // Resume the game
        }
        public void PauseGame()
        {
            // HUD.SetActive(false);
            onScreenControls.SetActive(false);
            // gameOverMenu.SetActive(false);
            // meditationMenu.SetActive(false);

            Time.timeScale = 0f; // Pause the game
        }

    }
}
