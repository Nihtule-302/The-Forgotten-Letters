using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Project.Scripts.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject pauseIcon;
        [SerializeField] private GameObject pauseBackground;
        
        private void Awake()
        {
            ResumeGame();
        }

        #region Buttons

        public void Pause()
        {
            PauseGame();
        }
        
        public void Resume()
        {
            ResumeGame();
        }

        public void Save() {}

        public void ReturnToMainMenu()
        {
            SceneManager.LoadScene("Main Menu");
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        private void PauseGame()
        {
            pauseIcon.SetActive(false);
            pauseMenu.SetActive(true);
            pauseBackground.SetActive(true);
        }

        private void ResumeGame()
        {
            pauseIcon.SetActive(true);
            pauseMenu.SetActive(false);
            pauseBackground.SetActive(false);
        }
    }
}
