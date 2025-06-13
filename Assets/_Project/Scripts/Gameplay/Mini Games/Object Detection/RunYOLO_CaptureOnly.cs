using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Core.Scriptable_Events.EventTypes.String;
using TMPro;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Mini_Games.Object_Detection
{
    public enum CameraTypes
    {
        Back,
        Front
    }

    public class RunYoloCaptureOnly : MonoBehaviour
    {
        
        private const int ImageWidth = 640;
        private const int ImageHeight = 640;

        [Header("References")] [Tooltip("Drag a YOLO model .onnx file here")]
        public AssetReference modelAssetRef;
        public ModelAsset modelAsset => SentisModelLoader.Instance.GetModel(modelAssetRef);

        [Tooltip("Drag the classes.txt here")] public TextAsset classesAsset;

        [Tooltip("Create a Raw Image in the scene and link it here")]
        public RawImage displayImage;

        [Tooltip("Drag a border box texture here")]
        public Texture2D borderTexture;

        [Tooltip("Select an appropriate TextMeshPro font asset for the labels")]
        public TMP_FontAsset tmpFont;

        [Tooltip("Button to resume camera after capturing")]
        public Button tryAgainButton;

        [Tooltip("UI Image to highlight the detection zone")]
        public Image detectionZoneOverlay;

        [Header("Camera Settings")]

        // Camera display size limits
        [SerializeField]
        private CameraTypes cameraType = CameraTypes.Back;

        [SerializeField] private float maxDisplayWidth = 1280f;
        [SerializeField] private float maxDisplayHeight = 720f;

        [SerializeField] [Range(0.1f, 2f)]
        private float screenSizeRatio = 1.3f; // How much of the screen to use (80% by default)

        [Header("events")] [SerializeField] private AssetReference predectionLabelRef;

        [Header("Model Settings")] [SerializeField] [Range(0, 1)]
        private float iouThreshold = 0.5f;

        [SerializeField] [Range(0, 1)] private float scoreThreshold = 0.5f;

        [SerializeField] [Range(0.1f, 1f)]
        private float detectionZoneRatio = 0.5f; // Percentage of image center to consider

        [SerializeField] private BackendType backend = BackendType.CPU;

        private readonly List<GameObject> _boxPool = new();

        // Image uvRect
        private readonly Rect _defaultRect = new(0f, 0f, 1f, 1f);
        private readonly Rect _fixedRect = new(0f, 1f, 1f, -1f);
        private WebCamDevice _activeCameraDevice;
        private WebCamTexture _activeCameraTexture;
        private WebCamDevice _backCameraDevice;
        private WebCamTexture _backCameraTexture;
        private Sprite _borderSprite;

        private Texture2D _capturedImage;

        private Tensor<float> _centersToCorners;
        private Transform _displayLocation;
        private IEnumerator _executionSchedule;
        private bool _executionStarted;

        // Device cameras
        private WebCamDevice _frontCameraDevice;

        private WebCamTexture _frontCameraTexture;
        private Tensor<float> _inputTensor;
        private bool _isCamAvailable;
        private string[] _labels;
        private Model _model;

        // Image rotation
        private Vector3 _rotationVector = new(0f, 0f, 0f);
        private RenderTexture _targetRT;
        private WebCamTexture _webcamTexture;
        private Worker _worker;
        private StringEvent PredectionLabel => EventLoader.Instance.GetEvent<StringEvent>(predectionLabelRef);

        private void Start()
        {
            if (_activeCameraTexture != null && _activeCameraTexture.isPlaying)
            {
                _activeCameraTexture.Stop();
                _activeCameraTexture = null;
            }

            displayImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            displayImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            displayImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            Application.targetFrameRate = 60;
            // Removed forced orientation to support different device orientations

            _labels = classesAsset.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            LoadModel();

            _targetRT = new RenderTexture(ImageWidth, ImageHeight, 0);
            _displayLocation = displayImage.transform;
            SetupInput();

            _borderSprite = Sprite.Create(
                borderTexture,
                new Rect(0, 0, borderTexture.width, borderTexture.height),
                new Vector2(borderTexture.width / 2f, borderTexture.height / 2f)
            );

            // Hide the try again button at start
            if (tryAgainButton != null) tryAgainButton.gameObject.SetActive(false);

            UpdateDetectionZoneVisual(); // Initial update for the detection zone visual
        }

        void OnDestroy()
        {
            if (_activeCameraTexture != null)
            {
                if (_activeCameraTexture.isPlaying)
                    _activeCameraTexture.Stop();

                _activeCameraTexture = null;
            }
        }

        private void Update()
        {
            HandleCameraDisplay();
        }

        private void SetupInput()
        {
            InitializeCam();
        }

        private void InitializeCam()
        {
            // Check for device cameras
            var devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                Debug.Log("No device cameras found");
                _isCamAvailable = false;
                return;
            }

            _isCamAvailable = true;

            FindFrontAndBackCam(devices);
            CreateCameraTextures();
            SetCameraFilterModes();
            SetCameraToUse();
        }

        private void FindFrontAndBackCam(WebCamDevice[] devices)
        {
            for (var i = 0; i < devices.Length; i++)
                if (devices[i].isFrontFacing)
                {
                    _frontCameraDevice = devices[i];
                    Debug.Log($"Found front camera: {devices[i].name}");
                }
                else
                {
                    _backCameraDevice = devices[i];
                    Debug.Log($"Found back camera: {devices[i].name}");
                }
        }

        private void CreateCameraTextures()
        {
            // Default to YOLO dimensions
            int width = 640, height = 640;

            // Try to find the front camera device with available resolutions
            var frontDevice = WebCamTexture.devices.FirstOrDefault(d => d.name == _frontCameraDevice.name);
            var backDevice = WebCamTexture.devices.FirstOrDefault(d => d.name == _backCameraDevice.name);

            // Check if devices support availableResolutions (Unity 2020.1+)
#if UNITY_2020_1_OR_NEWER
            if (frontDevice.availableResolutions != null && frontDevice.availableResolutions.Length > 0)
            {
                // Find closest resolution to 640x640
                var closest = frontDevice.availableResolutions.OrderBy(r =>
                    Math.Abs(r.width - 640) + Math.Abs(r.height - 640)).First();
                width = closest.width;
                height = closest.height;
            }
#endif
            Debug.Log($"Using front camera: {frontDevice.name} with resolution {width}x{height}");
            Debug.Log($"Using back camera: {backDevice.name} with resolution {width}x{height}");
            _frontCameraTexture = new WebCamTexture(_frontCameraDevice.name, width, height);
            _backCameraTexture = new WebCamTexture(_backCameraDevice.name, width, height);

            
        }

        private void SetCameraFilterModes()
        {
            // Set camera filter modes for a smoother looking image
            _frontCameraTexture.filterMode = FilterMode.Trilinear;
            _backCameraTexture.filterMode = FilterMode.Trilinear;
        }

        private void SetCameraToUse()
        {
            // Set the camera to use by default based on selected camera type
            if (cameraType == CameraTypes.Front)
            {
                SetActiveCamera(_frontCameraTexture);
                Debug.Log($"Using front camera: {_frontCameraTexture.deviceName}");
            }
            else // default to back camera
            {
                SetActiveCamera(_backCameraTexture);
                Debug.Log($"Using back camera: {_backCameraTexture.deviceName}");
            }
        }

        public void SetActiveCamera(WebCamTexture cameraToUse)
        {
            if (_activeCameraTexture != null)
            {
                _activeCameraTexture.Stop();
                Debug.Log($"Stopping previous camera: {_activeCameraTexture.deviceName}");
            }

            _activeCameraTexture = cameraToUse;
            _webcamTexture = _activeCameraTexture; // Keep reference for compatibility with existing code
            _activeCameraDevice = WebCamTexture.devices.FirstOrDefault(device =>
                device.name == cameraToUse.deviceName);

            displayImage.texture = _activeCameraTexture;
            _activeCameraTexture.Stop();
            _activeCameraTexture.Play();
        }

        /// <summary>
        ///     Switch between front and back camera
        /// </summary>
        public void SwitchCamera()
        {
            if (!_isCamAvailable) return;

            if (_activeCameraTexture == _frontCameraTexture)
            {
                cameraType = CameraTypes.Back;
                SetActiveCamera(_backCameraTexture);
                Debug.Log($"Switched to back camera: {_backCameraTexture.deviceName}");
            }
            else
            {
                cameraType = CameraTypes.Front;
                SetActiveCamera(_frontCameraTexture);
                Debug.Log($"Switched to front camera: {_frontCameraTexture.deviceName}");
            }
        }

        public void CaptureFrame()
        {
            _webcamTexture.Pause();
            Debug.Log("Capturing frame from webcam...");

            _capturedImage = new Texture2D(_webcamTexture.width, _webcamTexture.height);
            _capturedImage.SetPixels(_webcamTexture.GetPixels());
            _capturedImage.Apply();

            // Add these debug logs
            Debug.Log($"Webcam: {_webcamTexture.width}x{_webcamTexture.height}");
            Debug.Log($"Captured: {_capturedImage.width}x{_capturedImage.height}");
            Debug.Log($"Display: {displayImage.rectTransform.rect.size}");

            var resized = ResizeTexture(_capturedImage, ImageWidth, ImageHeight);
            displayImage.texture = resized;
            Graphics.Blit(resized, _targetRT);

            // Show the try again button when we've captured a frame
            if (tryAgainButton != null) tryAgainButton.gameObject.SetActive(true);

            if (_capturedImage != null) Destroy(_capturedImage);

            ExecuteMl();
            UpdateDetectionZoneVisual(); // Update visual after capture (displayImage texture changes)
        }

        /// <summary>
        ///     Resume camera feed after capturing an image
        /// </summary>
        public void ResumeCamera()
        {
            Debug.Log("Resuming camera feed...");
            // Clear any previous annotations
            ClearAnnotations();

            // Hide the try again button
            if (tryAgainButton != null) tryAgainButton.gameObject.SetActive(false);

            // Resume the webcam
            if (_webcamTexture != null)
            {
                // Set the display texture back to the webcam texture
                displayImage.texture = _webcamTexture;
                // Start playing the webcam again
                _webcamTexture.Play();
                Debug.Log($"Resumed camera: {_webcamTexture.deviceName}");
            }

            UpdateDetectionZoneVisual(); // Update visual when resuming camera
        }

        private Texture2D ResizeTexture(Texture2D source, int width, int height)
        {
            var rt = RenderTexture.GetTemporary(width, height);
            Graphics.Blit(source, rt);
            RenderTexture.active = rt;
            var result = new Texture2D(width, height);
            result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            result.Apply();
            RenderTexture.ReleaseTemporary(rt);
            Debug.Log($"Resized texture from {source.width}x{source.height} to {width}x{height}");
            return result;
        }

        private void LoadModel()
        {
            var model1 = ModelLoader.Load(modelAsset);

            _centersToCorners = new Tensor<float>(new TensorShape(4, 4),
                new[]
                {
                    1, 0, 1, 0,
                    0, 1, 0, 1,
                    -0.5f, 0, 0.5f, 0,
                    0, -0.5f, 0, 0.5f
                });

            var graph = new FunctionalGraph();
            var inputs = graph.AddInputs(model1);
            var modelOutput = Functional.Forward(model1, inputs)[0];
            var boxCoords = modelOutput[0, ..4, ..].Transpose(0, 1);
            var allScores = modelOutput[0, 4.., ..];
            var scores = Functional.ReduceMax(allScores, 0);
            var classIDs = Functional.ArgMax(allScores);
            var boxCorners = Functional.MatMul(boxCoords, Functional.Constant(_centersToCorners));
            var indices = Functional.NMS(boxCorners, scores, iouThreshold, scoreThreshold);
            var coords = boxCorners.IndexSelect(0, indices);
            var labelIDs = classIDs.IndexSelect(0, indices);

            _model = graph.Compile(coords, labelIDs);
            SetupEngine(_model, backend);
            _inputTensor = new Tensor<float>(new TensorShape(1, 3, ImageHeight, ImageWidth));

            Debug.Log($"input shape {_inputTensor.shape}");
        }

        public void ExecuteMl()
        {
            Debug.Log("🚀 YOLO started");

            // Check if inference is already running or if worker is not set up
            if (_executionStarted || _worker == null)
            {
                Debug.LogWarning("YOLO execution skipped: Already running or worker not initialized.");
                return;
            }

            if (_inputTensor == null)
            {
                Debug.LogError("YOLO execution skipped: inputTensor is null.");
                return;
            }

            try // Add try-catch block for robustness during inference and tensor operations
            {
                // Ensure the input texture (targetRT) is valid before converting
                if (_targetRT == null || !_targetRT.IsCreated())
                {
                    Debug.LogError("YOLO execution skipped: targetRT is null or not created.");
                    return;
                }

                TextureConverter.ToTensor(_targetRT, _inputTensor, new TextureTransform());
                _executionSchedule = _worker.ScheduleIterable(_inputTensor);
                _executionStarted = true;

                // --- Ensure the schedule runs to completion ---
                // This loop iterates through the schedule steps. For simple models it might be quick,
                // but it's the correct way to ensure the worker finishes processing.
                while (_executionSchedule.MoveNext())
                {
                }
                // --- Execution Complete ---

                Debug.Log("✅ YOLO inference finished");

                // --- Corrected Output Processing ---
                // Peek and read back the output tensors. Use ReadbackAndClone to get data on CPU.
                // Output 0 should be the bounding box coordinates after NMS
                // Output 1 should be the corresponding class IDs after NMS
                using var outputCoordsTensor = _worker.PeekOutput("output_0") as Tensor<float>;
                using var labelIDsTensor = _worker.PeekOutput("output_1") as Tensor<int>;

                // Check if tensors are null before attempting to read
                if (outputCoordsTensor == null || labelIDsTensor == null)
                {
                    Debug.LogError("Failed to retrieve output tensors from the model worker.");
                    _executionStarted = false; // Reset flag
                    return;
                }

                // Clone tensors to CPU memory
                using var outputCoords = outputCoordsTensor.ReadbackAndClone();
                using var labelIDs = labelIDsTensor.ReadbackAndClone();


                Debug.Log($"📦 Boxes detected after NMS: {outputCoords.shape[0]}");
                // Optional: Log shape for debugging, check if it's as expected (e.g., [num_boxes, 4])
                if (outputCoords.shape.rank > 1) Debug.Log($"Tensor shape outputCoords: {outputCoords.shape}");
                if (labelIDs.shape.rank > 0) Debug.Log($"Tensor shape labelIDs: {labelIDs.shape}");


                // Get the actual display size from the RawImage RectTransform
                // This is crucial for positioning calculations in DrawBox
                var displayWidthUI = displayImage.rectTransform.rect.width; // UI element width
                var displayHeightUI = displayImage.rectTransform.rect.height; // UI element height

                // Track the closest box to the center (of the original 640x640 image space)
                var closestDistance = float.MaxValue;
                BoundingBox? closestBox = null; // Use nullable struct to track if a valid box is found

                // Define the central detection zone boundaries based on imageWidth, imageHeight and detectionZoneRatio
                var zoneHalfWidth = ImageWidth * detectionZoneRatio / 2f;
                var zoneHalfHeight = ImageHeight * detectionZoneRatio / 2f;
                var imageCenterX = ImageWidth / 2f;
                var imageCenterY = ImageHeight / 2f;

                var zoneMinX = imageCenterX - zoneHalfWidth;
                var zoneMaxX = imageCenterX + zoneHalfWidth;
                var zoneMinY = imageCenterY - zoneHalfHeight;
                var zoneMaxY = imageCenterY + zoneHalfHeight;

                var boxesFound = outputCoords.shape[0];
                ClearAnnotations(); // Clear any boxes drawn previously

                for (var n = 0; n < Mathf.Min(boxesFound, 200); n++) // Limit processed boxes if needed
                {
                    // Read corner coordinates [x_min, y_min, x_max, y_max] from NMS output tensor
                    var xMin = outputCoords[n, 0];
                    var yMin = outputCoords[n, 1];
                    var xMax = outputCoords[n, 2];
                    var yMax = outputCoords[n, 3];

                    // --- Calculate center, width, and height FROM corners ---
                    var width = xMax - xMin;
                    var height = yMax - yMin;
                    var centerX = xMin + width * 0.5f;
                    var centerY = yMin + height * 0.5f;

                    // --- Basic sanity check for coordinates ---
                    // Ensure dimensions are positive and coords are roughly within image bounds
                    // Adjust tolerance (e.g., > 0.1f) if tiny boxes are problematic
                    if (width <= 0f || height <= 0f ||
                        xMin < 0f || yMin < 0f || xMax < 0f || yMax < 0f || // Basic check
                        xMax > ImageWidth || yMax > ImageHeight || xMin > ImageWidth ||
                        yMin > ImageHeight) // Bounds check
                    {
                        Debug.LogWarning(
                            $"Invalid or out-of-bounds box coordinates skipped at index {n}: xmin={xMin:F1}, ymin={yMin:F1}, xmax={xMax:F1}, ymax={yMax:F1}, w={width:F1}, h={height:F1}");
                        continue; // Skip this invalid or out-of-bounds box
                    }

                    // --- NEW: Check if the box center is within the defined detection zone ---
                    var isInDetectionZone = centerX >= zoneMinX && centerX <= zoneMaxX &&
                                            centerY >= zoneMinY && centerY <= zoneMaxY;

                    if (!isInDetectionZone)
                        // Optional: Log that the box was skipped
                        // Debug.Log($"Box {n} ({labels[labelIDs[n]]}) at ({centerX:F1}, {centerY:F1}) skipped: outside detection zone [{zoneMinX:F1}-{zoneMaxX:F1}, {zoneMinY:F1}-{zoneMaxY:F1}].");
                        continue; // Skip this box if it's not in the detection zone
                    // --- End of new check ---

                    // Calculate distance from the center of the *original 640x640 image space*
                    // This helps pick the object perceived as most central in the capture
                    var deltaX = Mathf.Abs(centerX - ImageWidth * 0.5f);
                    var deltaY = Mathf.Abs(centerY - ImageHeight * 0.5f);
                    var distanceFromCenter = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);

                    // If this box is closer to the center than the previous closest valid box
                    if (distanceFromCenter < closestDistance)
                    {
                        closestDistance = distanceFromCenter;
                        var labelIndex = labelIDs[n]; // Get the class ID for this box

                        // Ensure labelIndex is valid
                        if (labelIndex < 0 || labelIndex >= _labels.Length)
                        {
                            Debug.LogWarning($"Invalid label index {labelIndex} detected for box {n}. Skipping.");
                            continue; // Skip if label index is bad
                        }


                        closestBox = new BoundingBox
                        {
                            // Store the CALCULATED center, width, height
                            CenterX = centerX,
                            CenterY = centerY,
                            Width = width,
                            Height = height,
                            Label = _labels[labelIndex],
                            OrigCenterX = centerX, // Store the calculated center for logging in DrawBox
                            OrigCenterY = centerY // Store the calculated center for logging in DrawBox
                        };
                    }
                } // End of loop through boxes

                // --- Draw the closest box if one was found ---
                if (closestBox.HasValue)
                {
                    // Determine font size dynamically or use a fixed value
                    // Example: 5% of UI height, clamped between 14 and 40
                    var fontSize = Mathf.Clamp(displayHeightUI * 0.05f, 14f, 40f);

                    // Pass the BoundingBox struct containing the CORRECT center, width, height
                    // DrawBox will handle mirroring and scaling to the UI coordinates
                    DrawBox(closestBox.Value, 0, fontSize); // Using ID 0 as we only draw one box
                    Debug.Log($"✓ Showing closest object: {closestBox.Value.Label}");
                    PredectionLabel.Raise(closestBox.Value.Label); // Raise the event with the detected label
                }
                else
                {
                    Debug.Log("No valid objects detected or none met the criteria (e.g., closest to center).");
                }
                // --- End Corrected Output Processing ---
            }
            catch (Exception e) // Catch potential errors during inference/processing
            {
                Debug.LogError($"Error during ExecuteML: {e.Message}\n{e.StackTrace}");
            }
            finally // Ensure executionStarted is reset even if an error occurs
            {
                _executionStarted = false; // Reset flag allowing next execution
            }
        }

        public void DrawBox(BoundingBox box, int id, float fontSize)
        {
            var panel = id < _boxPool.Count ? _boxPool[id] : CreateNewBox(Color.yellow);
            panel.SetActive(true);

            var displayRectTransform = displayImage.rectTransform;
            var displaySize = displayRectTransform.rect.size;

            // Original coordinates from the model (0 to imageWidth/imageHeight)
            var origX = box.CenterX;
            var origY = box.CenterY;
            var origW = box.Width;
            var origH = box.Height;

            // --- Coordinate Transformation ---

            // 1. Handle Front Camera Mirroring FIRST
            // Flip the horizontal coordinate if using the front camera.
            if (cameraType == CameraTypes.Front) origX = ImageWidth - origX;

            // 2. Normalize coordinates (0 to 1) relative to the model input size
            var normalizedX = origX / ImageWidth;
            var normalizedY = origY / ImageHeight;
            var normalizedW = origW / ImageWidth;
            var normalizedH = origH / ImageHeight;

            // 3. Calculate the position within the RawImage RectTransform
            // The RawImage is centered (pivot 0.5, 0.5), so (0,0) in anchoredPosition is the center.
            // We map the normalized coordinates to the displaySize.
            // Note: UI Y-coordinates are typically inverted (positive Y is up),
            // while image Y-coordinates are often top-down (positive Y is down).
            // The model output likely follows the image convention (Y=0 is top).
            // So, we convert normalizedY to UI space: (0.5 - normalizedY) maps top (0) to +0.5*height
            // and bottom (1) to -0.5*height.
            var uiPosX = (normalizedX - 0.5f) * displaySize.x;
            var uiPosY = (0.5f - normalizedY) * displaySize.y; // Invert Y-axis

            // 4. Calculate the size in UI units
            var uiWidth = normalizedW * displaySize.x;
            var uiHeight = normalizedH * displaySize.y;

            // --- Apply to UI Elements ---

            var rt = panel.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(uiPosX, uiPosY);
            rt.sizeDelta = new Vector2(uiWidth, uiHeight);

            var label = panel.GetComponentInChildren<TextMeshProUGUI>();
            if (label == null)
            {
                Debug.LogError("TextMeshProUGUI component not found!");
                return;
            }

            // Make sure label is enabled and visible
            label.enabled = true;
            label.gameObject.SetActive(true);

            label.text = box.Label;
            label.fontSize = Mathf.Max((int)fontSize, 14); // Ensure minimum readable size
            label.color = Color.yellow;
            label.alignment = TextAlignmentOptions.Center;

            // Make sure material is set for the text
            if (label.material == null && tmpFont != null) // Check tmpFont isn't null
                // Attempt to get/create material
                try
                {
                    label.fontMaterial = tmpFont.material; // Assign directly if possible or use a standard method
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to assign material: {ex.Message}");
                    // Fallback or default material assignment might be needed here
                    // e.g., label.material = TMP_MaterialManager.GetMaterialForRendering(label, 0);
                }
            else if (tmpFont == null) Debug.LogWarning("TMP Font Asset not assigned in the inspector!");


            // Create background for better visibility (Simplified - consider adjusting if needed)
            var bgImage = label.GetComponentInChildren<Image>(); // Assumes background is child Image
            if (bgImage == null) // Check if background needs creation
            {
                var background = new GameObject("LabelBackground");
                background.transform.SetParent(label.transform, false); // Set parent before adding component
                background.transform.SetAsFirstSibling(); // Put behind text

                bgImage = background.AddComponent<Image>(); // Now add Image component
                bgImage.color = new Color(0, 0, 0, 0.5f); // Semi-transparent black

                var bgRT = background.GetComponent<RectTransform>();
                bgRT.anchorMin = new Vector2(0, 0);
                bgRT.anchorMax = new Vector2(1, 1);
                bgRT.offsetMin = new Vector2(-5, -2); // Padding X-min, Y-min
                bgRT.offsetMax = new Vector2(5, 2); // Padding X-max, Y-max (-ve values pull inwards)
            }


            // Adjust label position to appear at the top of the bounding box
            var labelRT = label.GetComponent<RectTransform>();
            labelRT.anchorMin = new Vector2(0.5f, 1); // Anchor to Top-Center of parent (the box panel)
            labelRT.anchorMax = new Vector2(0.5f, 1);
            labelRT.pivot = new Vector2(0.5f, 0); // Pivot at Bottom-Center of the label itself
            labelRT.anchoredPosition = new Vector2(0, 5); // Position 5 units above the anchor
            // Adjust label size dynamically or set a fixed reasonable size
            labelRT.sizeDelta =
                new Vector2(Mathf.Max(uiWidth, 100),
                    Mathf.Max(uiHeight * 0.2f, 30)); // Example: 20% of box height, min 30

            // Debugging Log
            Debug.Log(
                $"🎯 Drawing Box: {box.Label} | Orig Center: ({box.OrigCenterX:F1}, {box.OrigCenterY:F1}) | Mirrored Center: ({origX:F1}, {origY:F1}) | UI Pos: ({uiPosX:F1}, {uiPosY:F1}) | UI Size: ({uiWidth:F1}x{uiHeight:F1})");
        }

        public GameObject CreateNewBox(Color color)
        {
            var panel = new GameObject("ObjectBox");
            panel.AddComponent<CanvasRenderer>();
            var img = panel.AddComponent<Image>();
            img.color = color;
            img.sprite = _borderSprite;
            img.type = Image.Type.Sliced;
            panel.transform.SetParent(_displayLocation, false);

            var text = new GameObject("ObjectLabel");
            text.AddComponent<CanvasRenderer>();
            text.transform.SetParent(panel.transform, false);

            var txt = text.AddComponent<TextMeshProUGUI>();
            if (tmpFont != null)
                txt.font = tmpFont;
            else
                Debug.LogWarning("TMP Font Asset not assigned in the inspector!");
            txt.color = Color.yellow;
            txt.fontSize = 40;
            txt.alignment = TextAlignmentOptions.Center;
            txt.enableWordWrapping = false;
            txt.overflowMode = TextOverflowModes.Overflow;

            var rt2 = text.GetComponent<RectTransform>();
            rt2.anchorMin = new Vector2(0.5f, 1);
            rt2.anchorMax = new Vector2(0.5f, 1);
            rt2.pivot = new Vector2(0.5f, 0);
            rt2.anchoredPosition = new Vector2(0, 5);
            rt2.sizeDelta = new Vector2(100, 30);

            _boxPool.Add(panel);
            return panel;
        }

        public void ClearAnnotations()
        {
            foreach (var box in _boxPool) box.SetActive(false);
        }

        private void SetupEngine(Model model, BackendType backendType)
        {
            if (_worker != null) _worker.Dispose();
            _worker = new Worker(model, backendType);

            Debug.Log($"Worker initialized with backend: {backendType}");
        }

        private void UpdateDetectionZoneVisual()
        {
            if (detectionZoneOverlay == null || displayImage == null)
            {
                if (detectionZoneOverlay == null) Debug.LogWarning("Detection Zone Overlay UI element not assigned.");
                return;
            }

            var displayRectTransform = displayImage.rectTransform;
            var displayWidthUI = displayRectTransform.rect.width;
            var displayHeightUI = displayRectTransform.rect.height;

            var zoneWidthUI = displayWidthUI * detectionZoneRatio;
            var zoneHeightUI = displayHeightUI * detectionZoneRatio;

            detectionZoneOverlay.rectTransform.sizeDelta = new Vector2(zoneWidthUI, zoneHeightUI);
            // Assuming the overlay is a child of displayImage or another centered element,
            // and its pivot is also centered (0.5, 0.5).
            detectionZoneOverlay.rectTransform.anchoredPosition = Vector2.zero;
            detectionZoneOverlay.gameObject.SetActive(true);
            Debug.Log($"Detection zone visual updated: {zoneWidthUI}x{zoneHeightUI} at center of display.");
        }

        private void HandleCameraDisplay()
        {
            if (!_isCamAvailable || _activeCameraTexture == null || !_activeCameraTexture.isPlaying)
            {
                Debug.Log("No active camera available or camera is not playing.");
                Debug.Log("isCamAvailable: " + _isCamAvailable);
                Debug.Log("activeCameraTexture isPlaying: " + (_activeCameraTexture != null && _activeCameraTexture.isPlaying));
                Debug.Log("activeCameraTexture: " + (_activeCameraTexture != null ? _activeCameraTexture.deviceName : "null"));
                return;
            }

            // Wait for camera to initialize properly
            if (_activeCameraTexture.width < 100) return;

            // Rotate image to show correct orientation
            _rotationVector.z = -_activeCameraTexture.videoRotationAngle;
            displayImage.rectTransform.localEulerAngles = _rotationVector;

            // Handle screen and camera aspect ratio
            var camWidth = (float)_activeCameraTexture.width;
            var camHeight = (float)_activeCameraTexture.height;
            var screenWidth = Screen.width / Screen.dpi * screenSizeRatio * 96f; // Convert to pixels at 96dpi
            var screenHeight = Screen.height / Screen.dpi * screenSizeRatio * 96f;

            // Apply maximum size constraints
            screenWidth = Mathf.Min(screenWidth, maxDisplayWidth);
            screenHeight = Mathf.Min(screenHeight, maxDisplayHeight);

            var camAspect = camWidth / camHeight;
            var screenAspect = screenWidth / screenHeight;

            float finalWidth, finalHeight;

            if (camAspect > screenAspect)
            {
                // Camera is wider than screen
                finalWidth = screenWidth;
                finalHeight = finalWidth / camAspect;
            }
            else
            {
                // Camera is taller than screen
                finalHeight = screenHeight;
                finalWidth = finalHeight * camAspect;
            }

            // Set the final size
            displayImage.rectTransform.sizeDelta = new Vector2(finalWidth, finalHeight);

            // Handle vertical mirroring
            displayImage.uvRect = _activeCameraTexture.videoVerticallyMirrored ? _fixedRect : _defaultRect;

            Debug.Log($"Camera display updated: {finalWidth}x{finalHeight} at rotation {_activeCameraTexture.videoRotationAngle}°");

            UpdateDetectionZoneVisual(); // Update the detection zone visual whenever the displayImage changes
        }

        public struct BoundingBox
        {
            public float CenterX;
            public float CenterY;
            public float Width;
            public float Height;
            public string Label;
            public float OrigCenterX; // Added missing property
            public float OrigCenterY; // Added missing property
        }
    }
}