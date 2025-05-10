using System;
using _Project.Scripts.Core.Managers;
using TheForgottenLetters;
using TMPro;
using UnityEngine;

public class EnergyPointDisplayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI energyPointText;
    private PlayerAbilityStats playerAbilityStats => PersistentSOManager.GetSO<PlayerAbilityStats>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateEnergyPointText();
    }

    private void OnEnable() {
        UpdateEnergyPointText();
    }

    public void UpdateEnergyPointText()
    {
        energyPointText.text = PersistentSOManager.GetSO<PlayerAbilityStats>().energyPoints.ToString();
    }
}
