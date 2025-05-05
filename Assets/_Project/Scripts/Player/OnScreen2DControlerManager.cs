using TheForgottenLetters;
using UnityEngine;

public class OnScreen2DControlerManager : MonoBehaviour
{
    [SerializeField] private GameObject skillButton;
    [SerializeField] private PlayerAbilityStats playerAbilityStats;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        RefreshOnScreen2DControler();
    }

    private void OnEnable() {
        RefreshOnScreen2DControler();
    }

    public void RefreshOnScreen2DControler()
    {
        if(playerAbilityStats.unlockedSkills.Count > 0)
        {
            skillButton.SetActive(true);
        }
        else
        {
            skillButton.SetActive(false);
        }
    }
}
