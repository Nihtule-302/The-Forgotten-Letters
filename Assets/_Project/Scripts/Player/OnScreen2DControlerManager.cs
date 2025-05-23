using _Project.Scripts.Core.Managers;
using TheForgottenLetters;
using UnityEngine;

namespace _Project.Scripts.Player
{
    public class OnScreen2DControlerManager : MonoBehaviour
    {
        [SerializeField] private GameObject skillButton;
        private PlayerAbilityStats PlayerAbilityStats => PersistentSOManager.GetSO<PlayerAbilityStats>();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            RefreshOnScreen2DControler();
        }

        private void OnEnable()
        {
            RefreshOnScreen2DControler();
        }

        public void RefreshOnScreen2DControler()
        {
            if (PlayerAbilityStats.playerSkills.UnlockedSkills.Count > 0)
                skillButton.SetActive(true);
            else
                skillButton.SetActive(false);
        }
    }
}