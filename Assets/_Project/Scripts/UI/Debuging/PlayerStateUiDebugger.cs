using TMPro;
using UnityEngine;

public class PlayerStateUiDebugger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stateTMP;

    public void SetStateTMP(string currentState)
    {
        stateTMP.SetText($"State: {currentState}");
    }
}