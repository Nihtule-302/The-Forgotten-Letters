using TMPro;
using UnityEngine;

public class DisplayResults : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;

    private string orginalText;

    void Start()
    {
        orginalText = textMeshProUGUI.text;
    }
    
    public void DisplayResultsToText(string results)
    {
        var text = $"{orginalText}: {results}";
        textMeshProUGUI.text = text;
    }

    public void DisplayResultsToText(char results)
    {
        var text = $"{orginalText}: {results}";
        textMeshProUGUI.text = text;
    }
}
