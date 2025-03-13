using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickAndHoldHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Action onTap;  // Action for a tap
    public Action onHold; // Action for a hold
    public Action<float> onHoldProgress; // Tracks dissolve effect (0 to 1) 
    public Action onHoldRelease;

    private bool isHolding;
    private bool isPointerDown;
    private float holdTime = 0.5f; // Time required to trigger hold
    private float timeElapsed = 0f;

    public void SetHoldTime(float requestedHoldTime)
    {
        holdTime = Mathf.Max(0, requestedHoldTime); // Ensure holdTime is not negative
    }

    private CancellationTokenSource holdCancelToken;

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        isHolding = false;
        holdCancelToken = new CancellationTokenSource();
        DetectHold(holdCancelToken.Token).Forget();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        holdCancelToken?.Cancel(); // Stop the hold detection

        if (isHolding)
        {
            onHoldRelease?.Invoke(); // Notify that hold was released
        }
        else
        {
            onTap?.Invoke(); // Register as a tap if hold wasn’t reached
        }
    }

    private async UniTaskVoid DetectHold(CancellationToken token)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(holdTime), cancellationToken: token);

            if (isPointerDown) 
            {
                isHolding = true;
                isPointerDown = false;
                onHold?.Invoke();
            }
        }
        catch (OperationCanceledException) 
        {
            // Ignore if canceled
        }
    }
}
