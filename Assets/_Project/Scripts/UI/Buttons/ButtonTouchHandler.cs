using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace _Project.Scripts.UI.Buttons
{
    public class ButtonTouchHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler,
        IPointerExitHandler
    {
        private CancellationTokenSource holdCancelToken;
        private float holdTime = 0.5f;

        private bool isHolding;
        private bool isInsideButton;
        private bool isPointerDown;
        public Action onHold;
        public Action<float> onHoldProgress;
        public Action onHoldRelease;
        public Action onTap;

        public void OnPointerDown(PointerEventData eventData)
        {
            isPointerDown = true;
            isInsideButton = true;
            isHolding = false;
            holdCancelToken = new CancellationTokenSource();
            DetectHold(holdCancelToken.Token).Forget();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isInsideButton = true;

            // ✅ If the finger is already down when re-entering, restart hold detection
            if (Mouse.current.leftButton.isPressed || Touchscreen.current?.primaryTouch.press.isPressed == true)
            {
                isPointerDown = true; // ✅ Ensure it properly registers a hold
                holdCancelToken = new CancellationTokenSource();
                DetectHold(holdCancelToken.Token).Forget();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isInsideButton = false;

            if (isPointerDown)
            {
                isPointerDown = false;
                holdCancelToken?.Cancel();
                onHoldRelease?.Invoke();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!isPointerDown) return;

            isPointerDown = false;
            holdCancelToken?.Cancel();

            if (isHolding)
                onHoldRelease?.Invoke();
            else if (isInsideButton) onTap?.Invoke();

            isInsideButton = false; // Reset when released
        }

        public void SetHoldTime(float requestedHoldTime)
        {
            holdTime = Mathf.Max(0, requestedHoldTime);
        }

        private async UniTaskVoid DetectHold(CancellationToken token)
        {
            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(holdTime), cancellationToken: token);

                if (isPointerDown && isInsideButton)
                {
                    isHolding = true;
                    onHold?.Invoke();
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}