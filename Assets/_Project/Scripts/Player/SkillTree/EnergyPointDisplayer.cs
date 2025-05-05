using System;
using TheForgottenLetters;
using TMPro;
using UnityEngine;

public class EnergyPointDisplayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI energyPointText;
    [SerializeField] private PlayerAbilityStats playerAbilityStats;
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
        energyPointText.text = playerAbilityStats.EnergyPoints.ToString();
    }
}
