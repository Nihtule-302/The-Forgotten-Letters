using _Project.Scripts.Core.Managers;
using TheForgottenLetters;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Player.SkillTree
{
    public class EnergyPointDisplayer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI energyPointText;

        private PlayerAbilityStats PlayerAbilityStats => PersistentSOManager.GetSO<PlayerAbilityStats>();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            UpdateEnergyPointText();
        }

        private void OnEnable()
        {
            UpdateEnergyPointText();
        }

        public void UpdateEnergyPointText()
        {
            energyPointText.text = PersistentSOManager.GetSO<PlayerAbilityStats>().energyPoints.ToString();
        }
    }
}