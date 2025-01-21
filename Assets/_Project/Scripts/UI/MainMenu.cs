using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.UI
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject scenesMenu;
        [SerializeField] private GameObject settingsMenu;

        private void Awake()
        {
            ShowMainMenu();
        }

        #region MainMenu Methods

        public void StartGame()
        {
            ShowMenu(scenesMenu);
        }

        public void OpenSettings()
        {
            ShowMenu(settingsMenu);
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

        #region SceneMenu Methods

        public void OpenBahrElSafaScene()
        {
            LoadScene("Bahr El Safa");
        }

        public void OpenMoonScene()
        {
            LoadScene("Moon");
        }

        public void ReturnToMainMenu()
        {
            ShowMainMenu();
        }

        #endregion

        #region Helper Methods

        private void ShowMainMenu()
        {
            ShowMenu(mainMenu);
        }

        private void ShowMenu(GameObject menuToShow)
        {
            if (menuToShow == settingsMenu) return;
            
            mainMenu.SetActive(menuToShow == mainMenu);
            scenesMenu.SetActive(menuToShow == scenesMenu);
            settingsMenu.SetActive(menuToShow == settingsMenu);
        }

        private void LoadScene(string sceneName)
        {
            try
            {
                SceneManager.LoadScene(sceneName);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load scene: {sceneName}. Error: {e.Message}");
            }
        }

        #endregion
    }
}