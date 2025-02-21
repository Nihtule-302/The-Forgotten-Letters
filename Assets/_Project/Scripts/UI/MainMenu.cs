
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.UI
{
    public class MainMenu : MonoBehaviour
    {

        [Header("Menu Prefabs")]
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject scenesMenu;
        [SerializeField] private GameObject settingsMenu;

        [Header("Scenes To Load")]
        [SerializeField] private AssetReference bahrElSafa;
        [SerializeField] private AssetReference moon;


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
            SceneTransitionManager.Instance.TransitionScene(bahrElSafa);
        }

        public void OpenMoonScene()
        {
            SceneTransitionManager.Instance.TransitionScene(moon);
        }

        #endregion
        
        #region SettingsMenu Methods

        public void GameSettings()
        {
        }

        public void AudioSettings()
        {
        }
        
        public void VideoSettings()
        {
        }

        #endregion

        #region Helper Methods
        
        public void ReturnToMainMenu()
        {
            ShowMainMenu();
        }

        private void ShowMainMenu()
        {
            ShowMenu(mainMenu);
        }

        private void ShowMenu(GameObject menuToShow)
        {
            //if (menuToShow == settingsMenu) return;
            
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