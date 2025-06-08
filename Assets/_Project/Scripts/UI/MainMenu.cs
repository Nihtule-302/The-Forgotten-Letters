using _Project.Scripts.Core.Managers;
using TheForgottenLetters;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _Project.Scripts.UI
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Menu Prefabs")] [SerializeField]
        private GameObject mainMenu;

        [SerializeField] private GameObject scenesMenu;
        [SerializeField] private GameObject settingsMenu;

        [Header("Buttons")] [SerializeField] private GameObject StartGameButton;

        [SerializeField] private GameObject LoginOrSignButton;
        [SerializeField] private GameObject SignOutButton;


        [Header("Scenes To Load")] [SerializeField]
        private AssetReference bahrElSafa;

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
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion

        #region SceneMenu Methods

        public void OpenBahrElSafaScene()
        {
            SceneTransitionManager.Instance.TransitionSceneAsync(bahrElSafa);
        }

        public void OpenMoonScene()
        {
            SceneTransitionManager.Instance.TransitionSceneAsync(moon);
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
            ShowAuthButtons();
        }

        private void ShowMenu(GameObject menuToShow)
        {
            //if (menuToShow == settingsMenu) return;

            mainMenu.SetActive(menuToShow == mainMenu);
            scenesMenu.SetActive(menuToShow == scenesMenu);
            settingsMenu.SetActive(menuToShow == settingsMenu);
        }

        private void ShowAuthButtons()
        {
            if (Auth.Instance.IsLoggedIn)
            {
                StartGameButton.SetActive(true);
                LoginOrSignButton.SetActive(false);
                SignOutButton.SetActive(true);
            }
            else
            {
                StartGameButton.SetActive(false);
                LoginOrSignButton.SetActive(true);
                SignOutButton.SetActive(false);
            }
        }

        #endregion
    }
}