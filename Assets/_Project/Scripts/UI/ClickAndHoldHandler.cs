using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickAndHoldHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Action onTap;  // Action for a tap
    public Action onHold; // Action for a hold

    private bool isHolding;
    private bool isPointerDown;
    private float holdTime = 0.5f; // Time required to trigger hold

    public void SetHoldTime(float requstedHoldTime)
    {
        if (requstedHoldTime < 0)
        {
            this.holdTime = 0;
        }
        else
        {
            holdTime = requstedHoldTime;
        }
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
        holdCancelToken?.Cancel(); // Stop the hold detection if released early

        if (!isHolding) 
        {
            onTap?.Invoke();
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
