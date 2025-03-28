using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.Buttons.ChoiceButton
{
    public class ChoiceButtonInfoAccess : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] Image image;
        [SerializeField] DissolveControl dissolveControl;
        public TextMeshProUGUI Text => text;
        public Image Image => image;
        public DissolveControl DissolveControl => dissolveControl;

    }
}
