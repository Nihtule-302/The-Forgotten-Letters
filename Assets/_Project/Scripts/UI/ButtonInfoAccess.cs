using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonInfoAccess : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Image image;
    public TextMeshProUGUI Text => text;
    public Image Image => image;
}
