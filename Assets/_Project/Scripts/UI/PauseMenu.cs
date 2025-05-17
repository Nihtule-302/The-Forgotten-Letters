using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Scriptable_Events;
using _Project.Scripts.Core.Utilities;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _Project.Scripts.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("Menu Objects")]
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject pauseIcon;
        [SerializeField] private GameObject pauseBackground;

        [Header("Game Events")]
        [SerializeField] private AssetReference onPauseGameEventRef;
        [SerializeField] private AssetReference onResumeGameEventRef;

        private GameEvent onPauseGameEvent => EventLoader.Instance.GetEvent<GameEvent>(onPauseGameEventRef);
        private GameEvent onResumeGameEvent => EventLoader.Instance.GetEvent<GameEvent>(onResumeGameEventRef);


        [Header("Scene To Load")]
        [SerializeField] private AssetReference MainMenu;
    


        public void StartMenu()
        {
            ResumeGame();
        }


        #region Buttons

        public void Pause()
        {
            PauseGame();
            onPauseGameEvent.Raise();
        }
        
        public void Resume()
        {
            ResumeGame();
            onResumeGameEvent.Raise();
        }

        public void Save() {}

        public void ReturnToMainMenu()
        {
            SceneTransitionManager.Instance.TransitionScene(MainMenu);
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
