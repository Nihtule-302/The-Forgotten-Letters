using System.Collections;
using UnityEngine;
using _Project.Scripts.Core.Managers;

public class DrawLine : MonoBehaviour
{
    [SerializeField] private InputManagerSO inputManager;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private SpriteMask spriteMask;  // The sprite mask

    [SerializeField, Range(0f, 1000f)] private float minDistanceThreshold = 200f;
    private float MinDistanceThreshold => minDistanceThreshold / 1000f;

    private Coroutine drawingCoroutine;
    private bool continueToDraw = false;
    private bool isDrawing = false;

    private Vector3 position;

    void Start()
    {
        inputManager.Click += HandleClick;
        inputManager.EnablePlayerActions();
    }

    void Update()
    {
        position = Camera.main.ScreenToWorldPoint(inputManager.DrawPointerPosition);
        position.z = 0;

        if (continueToDraw && !isDrawing)
        {
            StartLine();
        }
        else if (!continueToDraw && isDrawing)
        {
            FinishLine();
        }

        if (IsInsideMask(position))
        {
           HandleClick(inputManager.IsClicking);
        }
    }

    private void HandleClick(bool isClicking)
    {
        continueToDraw = isClicking;
    }

    private void StartLine()
    {
        drawingCoroutine = StartCoroutine(Draw());
        isDrawing = true;
    }

    private void FinishLine()
    {
        if (drawingCoroutine != null)
        {
            StopCoroutine(drawingCoroutine);
            drawingCoroutine = null;
        }
        isDrawing = false;
    }

    private IEnumerator Draw()
    {
        GameObject newLineObject = Instantiate(linePrefab);
        LineRenderer line = newLineObject.GetComponent<LineRenderer>();
        line.positionCount = 0;

        Vector3 lastPosition = Vector3.positiveInfinity;

        while (true)
        {
            // Check if the position is within the bounds of the sprite mask
            if (IsInsideMask(position))
            {
                if (Vector3.Distance(position, lastPosition) > MinDistanceThreshold)
                {
                    lastPosition = position;
                    line.positionCount++;
                    line.SetPosition(line.positionCount - 1, position);
                }
            }
            else
            {
                continueToDraw = false;
            }

            yield return null;
        }
    }

    // Check if the position is inside the sprite mask
    private bool IsInsideMask(Vector3 position)
    {
        // Get the bounds of the sprite mask in world space
        Bounds maskBounds = spriteMask.bounds;

        return maskBounds.Contains(position);
    }
}
