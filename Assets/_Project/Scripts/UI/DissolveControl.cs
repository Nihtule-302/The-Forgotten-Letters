using UnityEngine;
using UnityEngine.UI;
using System; // Required for Action

public class DissolveControl : MonoBehaviour
{
    [SerializeField] private Image DissolveImage;

    private Material dissolveMaterialInstance;

    public Color baseDissolveColor;
    public Color edgeColor;
    public float edgeWidth;
    
    public float dissolveStart = 1f;
    public float dissolveEnd = 0f;
    public float dissolveDuration = 3f;

    private float currentDissolveValue;
    
    // Separate events for each action
    public Action OnDissolveComplete;       // Called when StartDissolveEffect finishes
    public Action OnResetDissolveComplete;  // Called when ResetDissolveEffect finishes

    private void Start()
    {
        InitializeMaterial();

        // Subscribe to the events
        OnDissolveComplete += () => Debug.Log("Dissolve effect completed successfully!");
        OnResetDissolveComplete += () => Debug.Log("Reset dissolve effect completed successfully!");
    }

    private void InitializeMaterial()
    {
        if (DissolveImage != null)
        {
            dissolveMaterialInstance = new Material(DissolveImage.material);
            DissolveImage.material = dissolveMaterialInstance;
        }

        if (dissolveMaterialInstance != null)
        {
            currentDissolveValue = dissolveStart;
            SetDissolveAmount(currentDissolveValue);
            SetDissolveSettings();
        }
    }

    private void SetDissolveSettings()
    {
        dissolveMaterialInstance.SetFloat("_Edge_Width", edgeWidth);
        dissolveMaterialInstance.SetColor("_Edge_Color", edgeColor);
        dissolveMaterialInstance.SetColor("_Base_Color", baseDissolveColor);
    }

    public void StartDissolveEffect(Action callback = null)
    {
        RunDissolveTween(dissolveEnd, callback, OnDissolveComplete);
    }

    public void ResetDissolveEffect(Action callback = null)
    {
        RunDissolveTween(dissolveStart, callback, OnResetDissolveComplete);
    }

    private void RunDissolveTween(float targetValue, Action callback, Action completeEvent)
    {
        LeanTween.cancel(gameObject);
        
        LeanTween.value(gameObject, currentDissolveValue, targetValue, dissolveDuration)
            .setOnUpdate(SetDissolveAmount)
            .setOnComplete(() => InvokeCompletionEvent(callback, completeEvent));
    }

    private void SetDissolveAmount(float value)
    {
        currentDissolveValue = value;
        dissolveMaterialInstance.SetFloat("_Dissolve", currentDissolveValue);
    }

    private void InvokeCompletionEvent(Action callback, Action completeEvent)
    {
        callback?.Invoke();
        completeEvent?.Invoke(); // Calls the appropriate event
    }
}

public class DissolveData
{
    public float dissolveStart = 1f;
    public float dissolveEnd = 0f;
    public float dissolveDuration = 3f;

    public Color baseDissolveColor;
    public Color edgeColor;
    public float edgeWidth;

}
