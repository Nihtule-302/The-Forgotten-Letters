using TMPro;
using UnityEngine;

public class ButtonInfoAccess : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    public TextMeshProUGUI Text => text;
    void Start()
    {
        if (text != null) return;       
        
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

}
