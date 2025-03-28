using TMPro;
using UnityEngine;

namespace _Project.Scripts.Core.Utilities.UI
{
    public class GetValueFromDropdown : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown dropdown;
        public int GetDropdownValue()
        {
            var dropDownIndex = dropdown.value;
            return dropDownIndex;
        }
    }
}
