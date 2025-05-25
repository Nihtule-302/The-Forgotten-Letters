using Unity.UI.Shaders.Sample;
using UnityEngine;


public class SliderValueHandler : MonoBehaviour
{
    [SerializeField] Meter meter;

    public void SetSliderValue(float value)
    {
        meter.Value = value;
    }

    public float GetSliderValue()
    {
        return meter.Value;
    }

    
}
