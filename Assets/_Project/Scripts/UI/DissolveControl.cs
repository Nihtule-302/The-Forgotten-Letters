using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI
{
    public class DissolveControl : MonoBehaviour
    {
        [SerializeField] private Image DissolveImage;

        public Color baseDissolveColor;
        public Color edgeColor;
        public float edgeWidth;

        public float dissolveStart = 1f;
        public float dissolveEnd;
        public float dissolveDuration = 3f;

        private float currentDissolveValue;

        private Material dissolveMaterialInstance;

        public Action OnDissolveComplete;
        public Action OnResetDissolveComplete;

        // Cache original colors
        private Color originalBaseColor;
        private Color originalEdgeColor;

        private void Start()
        {
            InitializeMaterial();

            OnDissolveComplete += () => Debug.Log("Dissolve effect completed successfully!");
            OnResetDissolveComplete += () => Debug.Log("Reset dissolve effect completed successfully!");

            // Cache the original colors
            originalBaseColor = baseDissolveColor;
            originalEdgeColor = edgeColor;
        }

        private void OnEnable()
        {
            // Subscribe to events
            OnDissolveComplete += () => Debug.Log("Dissolve effect completed successfully!");
            OnResetDissolveComplete += () => Debug.Log("Reset dissolve effect completed successfully!");

            currentDissolveValue = dissolveStart;
            SetDissolveAmount(currentDissolveValue);
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            OnDissolveComplete -= () => Debug.Log("Dissolve effect completed successfully!");
            OnResetDissolveComplete -= () => Debug.Log("Reset dissolve effect completed successfully!");

            currentDissolveValue = dissolveStart;
            SetDissolveAmount(currentDissolveValue);
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

        public void SetDissolveSettings()
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
            RunDissolveTween(dissolveStart, () =>
            {
                RestoreOriginalColorsIfChanged();
                callback?.Invoke();
            }, OnResetDissolveComplete);
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
            completeEvent?.Invoke();
        }

        private void RestoreOriginalColorsIfChanged()
        {
            var isBaseColorChanged = dissolveMaterialInstance.GetColor("_Base_Color") != originalBaseColor;
            var isEdgeColorChanged = dissolveMaterialInstance.GetColor("_Edge_Color") != originalEdgeColor;

            if (isBaseColorChanged)
            {
                dissolveMaterialInstance.SetColor("_Base_Color", originalBaseColor);
                Debug.Log("Base color restored to original.");
            }

            if (isEdgeColorChanged)
            {
                dissolveMaterialInstance.SetColor("_Edge_Color", originalEdgeColor);
                Debug.Log("Edge color restored to original.");
            }
        }
    }

    public class DissolveData
    {
        public Color baseDissolveColor;
        public float dissolveDuration = 3f;
        public float dissolveEnd = 0f;
        public float dissolveStart = 1f;
        public Color edgeColor;
        public float edgeWidth;
    }
}