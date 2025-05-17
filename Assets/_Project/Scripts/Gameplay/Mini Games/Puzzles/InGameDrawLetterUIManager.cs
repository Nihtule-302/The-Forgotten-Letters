using UnityEngine;

public class InGameDrawLetterUIManager : MonoBehaviour
{
    
    [SerializeField] private GameObject Trigger;
    [SerializeField] private GameObject VisualCue;
    
    [SerializeField] private GameObject DrawLetterPuzzelScreen;
    [SerializeField] private GameObject Game2DUI;

    void Awake()
    {
        DrawLetterPuzzelScreen.SetActive(false);
    }

    public void ShowDrawLetterPuzzle()
    {
        DrawLetterPuzzelScreen.SetActive(true);
        Game2DUI.SetActive(false);
    }
    public void HideDrawLetterPuzzle()
    {
        DrawLetterPuzzelScreen.SetActive(false);
        Game2DUI.SetActive(true);
    }
    public void DeactivateInteractivity()
    {
        Trigger.SetActive(false);
        VisualCue.SetActive(false);
    }
}
