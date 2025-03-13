using UnityEngine;
using UnityEngine.UI;
using System; // Required for Action

public class DissolveControl : MonoBehaviour
{
    [SerializeField] private Image DissolveImage;

    private Material dissolveMaterialInstance;
    
    [SerializeField] private float dissolveStart = 1f;
    [SerializeField] private float dissolveEnd = 0f;
    [SerializeField] private float dissolveDuration = 0.5f;

    private float currentDissolveValue;
    public Action OnDissolveComplete; // Action to call when dissolve completes

    private void Start()
    {
        InitializeMaterial();
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
        }
    }

    public void StartDissolveEffect(Action callback = null)
    {
        RunDissolveTween(dissolveEnd, callback, true);
    }

    public void ResetDissolveEffect(Action callback = null)
    {
        RunDissolveTween(dissolveStart, callback, false);
    }

    private void RunDissolveTween(float targetValue, Action callback, bool invokeCompleteEvent)
    {
        LeanTween.cancel(gameObject);
        
        LeanTween.value(gameObject, currentDissolveValue, targetValue, dissolveDuration)
            .setOnUpdate(SetDissolveAmount)
            .setOnComplete(() => HandleDissolveComplete(callback, invokeCompleteEvent));
    }

    private void SetDissolveAmount(float value)
    {
        currentDissolveValue = value;
        dissolveMaterialInstance.SetFloat("_DissolveAmount", currentDissolveValue);
    }

    private void HandleDissolveComplete(Action callback, bool invokeCompleteEvent)
    {
        Debug.Log("Dissolve effect completed!");
        callback?.Invoke();
        if (invokeCompleteEvent)
        {
            OnDissolveComplete?.Invoke();
        }
    }
}
