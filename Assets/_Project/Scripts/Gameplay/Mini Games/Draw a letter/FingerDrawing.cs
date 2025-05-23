using System.Collections;
using _Project.Scripts.Core.Managers;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Mini_Games.Draw_a_letter
{
    public class FingerDrawing : MonoBehaviour
    {
        [SerializeField] private RawImage displayImage;

        [FormerlySerializedAs("Classifier")] [SerializeField] private HandwrittenClassifier classifier;

        // [SerializeField] private Transform fingerTipMarkerTransform;
        [SerializeField] private float delayToSend = 1f;

        [SerializeField] private InputManagerSO input;

        [SerializeField] private int imageSize;
        [SerializeField] private int brushSize;
        [SerializeField] private FilterMode filterMode = FilterMode.Point;
        private Coroutine _checkForSendCoroutine;
        private Texture2D _drawingTexture;

        private bool _hasDrawn;
        private float _lastDrawTime;
        private Camera _mainCamera;
        private int DisplayImageWidth => (int)displayImage.rectTransform.rect.width;
        private int DisplayImageHeight => (int)displayImage.rectTransform.rect.height;

        private void Start()
        {
            _drawingTexture = new Texture2D(DisplayImageWidth, DisplayImageHeight, TextureFormat.RGB24, false);
            _drawingTexture.filterMode = filterMode;
            displayImage.texture = _drawingTexture;

            displayImage.color = Color.white;

            _mainCamera = Camera.main;
            input.EnablePlayerActions();
            ClearTexture();
        }

        private void Update()
        {
            var isDrawing = input.IsClicking;

            _drawingTexture.filterMode = filterMode;

            if (isDrawing)
            {
                if (_checkForSendCoroutine != null)
                {
                    StopCoroutine(_checkForSendCoroutine);
                    _checkForSendCoroutine = null;
                }

                Draw(input.DrawPointerPosition);
                _hasDrawn = true;
                _lastDrawTime = Time.time;
            }
            else if (_hasDrawn && Time.time - _lastDrawTime > delayToSend && _checkForSendCoroutine == null)
            {
                _checkForSendCoroutine = StartCoroutine(CheckForSend());
            }
        }

        private IEnumerator CheckForSend()
        {
            yield return new WaitForSeconds(delayToSend);
            classifier.ExecuteModel(_drawingTexture);
            _hasDrawn = false;
            _checkForSendCoroutine = null;
        }

        private void Draw(Vector3 position)
        {
            Vector2 screenPoint = _mainCamera.WorldToScreenPoint(position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(displayImage.rectTransform, screenPoint, _mainCamera,
                out var localPoint);
            var normalizedPoint = Rect.PointToNormalized(displayImage.rectTransform.rect, localPoint);
            AddPixel(normalizedPoint);
        }

        private void AddPixel(Vector2 normalizedPoint)
        {
            var texX = (int)(normalizedPoint.x * _drawingTexture.width);
            var texY = (int)(normalizedPoint.y * _drawingTexture.height);

            var radius = brushSize / 2;
            var sqrRadius = radius * radius;

            for (var x = -radius; x <= radius; x++)
            for (var y = -radius; y <= radius; y++)
                // Check if within circle
                if (x * x + y * y <= sqrRadius)
                {
                    var px = texX + x;
                    var py = texY + y;

                    if (px >= 0 && px < _drawingTexture.width && py >= 0 && py < _drawingTexture.height)
                        _drawingTexture.SetPixel(px, py, Color.white);
                }

            _drawingTexture.Apply();
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
            var zeroes = new byte[DisplayImageWidth * DisplayImageHeight * 3];
            _drawingTexture.LoadRawTextureData(zeroes);
            _drawingTexture.Apply();
        }
    }
}