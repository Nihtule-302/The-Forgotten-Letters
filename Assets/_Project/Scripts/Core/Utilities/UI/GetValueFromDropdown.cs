using TMPro;
using UnityEngine;

public class GetValueFromDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    public int GetDropdownValue()
    {
        var dropDownIndex = dropdown.value;
        return dropDownIndex;
    }
}
