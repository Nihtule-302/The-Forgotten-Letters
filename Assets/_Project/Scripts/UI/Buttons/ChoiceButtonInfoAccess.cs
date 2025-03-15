using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceButtonInfoAccess : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Image image;
    [SerializeField] DissolveControl dissolveControl;
    public TextMeshProUGUI Text => text;
    public Image Image => image;
    public DissolveControl DissolveControl => dissolveControl;

}
