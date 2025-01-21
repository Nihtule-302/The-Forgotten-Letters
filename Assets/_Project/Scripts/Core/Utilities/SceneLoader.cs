using _Project.Scripts.Core.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Core.Utilities
{
    public class SceneLoader : ISceneLoader
    {
        public void LoadScene(string sceneName)
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
    }
}
