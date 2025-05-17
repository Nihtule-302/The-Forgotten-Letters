using System.Collections;
using _Project.Scripts.Core.Managers;
using UnityEngine;
using UnityEngine.UI;

public class FingerDrawing : MonoBehaviour
{
    [SerializeField] private RawImage displayImage;
    [SerializeField] private HandwrittenClassifier Classifier;
    // [SerializeField] private Transform fingerTipMarkerTransform;
    [SerializeField] private float delayToSend = 1f;

    [SerializeField] private InputManagerSO input;

    [SerializeField] private int imageSize; 

    bool hasDrawn = false;
    float lastDrawTime;
    Camera mainCamera;
    Texture2D drawingTexture;
    Coroutine checkForSendCoroutine;
    [SerializeField] private int brushSize;
    [SerializeField] private FilterMode filterMode = FilterMode.Point;
    private int displayImageWidth => (int) displayImage.rectTransform.rect.width;
    private int displayImageHeight => (int) displayImage.rectTransform.rect.height;

    void Start()
    {
        drawingTexture = new Texture2D(displayImageWidth,displayImageHeight,TextureFormat.RGB24, false);
        drawingTexture.filterMode = filterMode;
        displayImage.texture = drawingTexture;

        displayImage.color = Color.white;
        
        mainCamera = Camera.main;
        input.EnablePlayerActions();
        ClearTexture();
    }

    void Update()
    {
        bool isDrawing =  input.IsClicking;

        drawingTexture.filterMode = filterMode;

        if (isDrawing)
        {   
            if (checkForSendCoroutine != null)
            {
                StopCoroutine(checkForSendCoroutine);
                checkForSendCoroutine = null;
            }

            Draw(input.DrawPointerPosition);
            hasDrawn = true;
            lastDrawTime = Time.time;
        }
        else if(hasDrawn && Time.time - lastDrawTime > delayToSend && checkForSendCoroutine == null)
        {
            checkForSendCoroutine = StartCoroutine(CheckForSend());
        }
    }

    private IEnumerator CheckForSend()
    {
        yield return new WaitForSeconds(delayToSend);
        Classifier.ExecuteModel(drawingTexture);
        hasDrawn = false;
        checkForSendCoroutine = null;

    }

    private void Draw(Vector3 position)
    {
        Vector2 screenPoint = mainCamera.WorldToScreenPoint(position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(displayImage.rectTransform, screenPoint, mainCamera, out Vector2 localPoint);
        Vector2 normalizedPoint = Rect.PointToNormalized(displayImage.rectTransform.rect, localPoint);
        AddPixel(normalizedPoint);
    }

    private void AddPixel(Vector2 normalizedPoint)
    {
        int texX = (int)(normalizedPoint.x * drawingTexture.width);
        int texY = (int)(normalizedPoint.y * drawingTexture.height);

        int radius = brushSize / 2;
        int sqrRadius = radius * radius;

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                // Check if within circle
                if (x * x + y * y <= sqrRadius)
                {
                    int px = texX + x;
                    int py = texY + y;

                    if (px >= 0 && px < drawingTexture.width && py >= 0 && py < drawingTexture.height)
                    {
                        drawingTexture.SetPixel(px, py, Color.white );
                    }
                }
            }
        }

        drawingTexture.Apply();
    }




    // public void ClearTexture()
    // {
    //     Color[] clearImageColors = new Color[drawingTexture.width * drawingTexture.height];
    //     for (int i = 0; i < clearImageColors.Length; i++)
    //     {
    //         clearImageColors[i] = Color.black;
    //     }
    //     drawingTexture.SetPixels(clearImageColors);
    //     drawingTexture.Apply();
    // }

    public void ClearTexture()
    {
        byte[] zeroes = new byte[displayImageWidth * displayImageHeight*3];
        drawingTexture.LoadRawTextureData(zeroes);
        drawingTexture.Apply();
    }
}
