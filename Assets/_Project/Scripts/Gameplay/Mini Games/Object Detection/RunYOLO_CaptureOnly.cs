using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using FF = Unity.Sentis.Functional;
using System;
using TMPro;
using System.Linq;

public enum CameraTypes
{
    BACK,
    FRONT
}

public class RunYOLO_CaptureOnly : MonoBehaviour
{
    [Tooltip("Drag a YOLO model .onnx file here")]
    public ModelAsset modelAsset;

    [Tooltip("Drag the classes.txt here")]
    public TextAsset classesAsset;

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

    [SerializeField] BackendType backend = BackendType.CPU;
    [SerializeField] TMP_Dropdown backendTypeDropdown;
    [SerializeField] CameraTypes cameraType = CameraTypes.BACK;
    [SerializeField] AspectRatioFitter imageFitter;

    // Camera display size limits
    [SerializeField] float maxDisplayWidth = 1280f;
    [SerializeField] float maxDisplayHeight = 720f;
    [SerializeField, Range(0.1f, 2f)] float screenSizeRatio = 1.3f; // How much of the screen to use (80% by default)

    // Device cameras
    private WebCamDevice frontCameraDevice;
    private WebCamDevice backCameraDevice;
    private WebCamDevice activeCameraDevice;
    private bool isCamAvailable = false;

    private WebCamTexture frontCameraTexture;
    private WebCamTexture backCameraTexture;
    private WebCamTexture activeCameraTexture;
    private WebCamTexture webcamTexture;
    
    // Image rotation
    private Vector3 rotationVector = new Vector3(0f, 0f, 0f);

    // Image uvRect
    private Rect defaultRect = new Rect(0f, 0f, 1f, 1f);
    private Rect fixedRect = new Rect(0f, 1f, 1f, -1f);

    private Texture2D capturedImage;
    private Transform displayLocation;
    private Worker worker;
    private string[] labels;
    private RenderTexture targetRT;
    private Sprite borderSprite;

    private const int imageWidth = 640;
    private const int imageHeight = 640;

    List<GameObject> boxPool = new();

    [SerializeField, Range(0, 1)] float iouThreshold = 0.5f;
    [SerializeField, Range(0, 1)] float scoreThreshold = 0.5f;
    [SerializeField, Range(0.1f, 1f)] float detectionZoneRatio = 0.5f; // Percentage of image center to consider

    Tensor<float> centersToCorners;
    Tensor<float> inputTensor;
    Model model;
    bool executionStarted = false;
    System.Collections.IEnumerator executionSchedule;

    public struct BoundingBox
    {
        public float centerX;
        public float centerY;
        public float width;
        public float height;
        public string label;
        public float origCenterX;  // Added missing property
        public float origCenterY;  // Added missing property
    }

    void Start()
    {
        displayImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        displayImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        displayImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        Application.targetFrameRate = 60;
        // Removed forced orientation to support different device orientations

        labels = classesAsset.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        LoadModel();

        targetRT = new RenderTexture(imageWidth, imageHeight, 0);
        displayLocation = displayImage.transform;
        SetupInput();

        borderSprite = Sprite.Create(
            borderTexture,
            new Rect(0, 0, borderTexture.width, borderTexture.height),
            new Vector2(borderTexture.width / 2f, borderTexture.height / 2f)
        );
        
        // Hide the try again button at start
        if (tryAgainButton != null)
        {
            tryAgainButton.gameObject.SetActive(false);
        }

        UpdateDetectionZoneVisual(); // Initial update for the detection zone visual
    }

    void SetupInput()
    {
        InitializeCam();
    }

    private void InitializeCam()
    {
        // Check for device cameras
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.Log("No device cameras found");
            isCamAvailable = false;
            return;
        }
        isCamAvailable = true;

        FindFrontAndBackCam(devices);
        CreateCameraTextures();
        SetCameraFilterModes();
        SetCameraToUse();
    }

    private void FindFrontAndBackCam(WebCamDevice[] devices)
    {
        for (int i = 0; i < devices.Length; i++)
        {
            if (devices[i].isFrontFacing)
            {
                frontCameraDevice = devices[i];
                Debug.Log($"Found front camera: {devices[i].name}");
            }
            else
            {
                backCameraDevice = devices[i];
                Debug.Log($"Found back camera: {devices[i].name}");
            }
        }
    }

    private void CreateCameraTextures()
    {
        // Default to YOLO dimensions
        int width = 640, height = 640;
        
        // Try to find the front camera device with available resolutions
        var frontDevice = WebCamTexture.devices.FirstOrDefault(d => d.name == frontCameraDevice.name);
        var backDevice = WebCamTexture.devices.FirstOrDefault(d => d.name == backCameraDevice.name);

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

        frontCameraTexture = new WebCamTexture(frontCameraDevice.name, width, height);
        backCameraTexture = new WebCamTexture(backCameraDevice.name, width, height);
    }
    
    private void SetCameraFilterModes()
    {
        // Set camera filter modes for a smoother looking image
        frontCameraTexture.filterMode = FilterMode.Trilinear;
        backCameraTexture.filterMode = FilterMode.Trilinear;
    }

    private void SetCameraToUse()
    {
        // Set the camera to use by default based on selected camera type
        if (cameraType == CameraTypes.FRONT)
            SetActiveCamera(frontCameraTexture);
        else // default to back camera
            SetActiveCamera(backCameraTexture);
    }

    public void SetActiveCamera(WebCamTexture cameraToUse)
    {
        if (activeCameraTexture != null)
        {
            activeCameraTexture.Stop();
        }

        activeCameraTexture = cameraToUse;
        webcamTexture = activeCameraTexture; // Keep reference for compatibility with existing code
        activeCameraDevice = WebCamTexture.devices.FirstOrDefault(device => 
            device.name == cameraToUse.deviceName);

        displayImage.texture = activeCameraTexture;
        activeCameraTexture.Play();
    }

    /// <summary>
    /// Switch between front and back camera
    /// </summary>
    public void SwitchCamera()
    {
        if (!isCamAvailable) return;
        
        if (activeCameraTexture == frontCameraTexture)
        {
            cameraType = CameraTypes.BACK;
            SetActiveCamera(backCameraTexture);
        }
        else
        {
            cameraType = CameraTypes.FRONT;
            SetActiveCamera(frontCameraTexture);
        }
    }

    public void CaptureFrame()
    {
        webcamTexture.Pause();

        capturedImage = new Texture2D(webcamTexture.width, webcamTexture.height);
        capturedImage.SetPixels(webcamTexture.GetPixels());
        capturedImage.Apply();

        // Add these debug logs
        Debug.Log($"Webcam: {webcamTexture.width}x{webcamTexture.height}");
        Debug.Log($"Captured: {capturedImage.width}x{capturedImage.height}");
        Debug.Log($"Display: {displayImage.rectTransform.rect.size}");

        Texture2D resized = ResizeTexture(capturedImage, imageWidth, imageHeight);
        displayImage.texture = resized;
        Graphics.Blit(resized, targetRT);
        
        // Show the try again button when we've captured a frame
        if (tryAgainButton != null)
        {
            tryAgainButton.gameObject.SetActive(true);
        }

        if (capturedImage != null)
        {
            Destroy(capturedImage);
        }
        
        ExecuteML();
        UpdateDetectionZoneVisual(); // Update visual after capture (displayImage texture changes)
    }

    /// <summary>
    /// Resume camera feed after capturing an image
    /// </summary>
    public void ResumeCamera()
    {
        // Clear any previous annotations
        ClearAnnotations();
        
        // Hide the try again button
        if (tryAgainButton != null)
        {
            tryAgainButton.gameObject.SetActive(false);
        }
        
        // Resume the webcam
        if (webcamTexture != null)
        {
            // Set the display texture back to the webcam texture
            displayImage.texture = webcamTexture;
            // Start playing the webcam again
            webcamTexture.Play();
        }
        UpdateDetectionZoneVisual(); // Update visual when resuming camera
    }

    Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        RenderTexture rt = RenderTexture.GetTemporary(width, height);
        Graphics.Blit(source, rt);
        RenderTexture.active = rt;
        Texture2D result = new Texture2D(width, height);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        result.Apply();
        RenderTexture.ReleaseTemporary(rt);
        return result;
    }

    void LoadModel()
    {
        var model1 = ModelLoader.Load(modelAsset);

        centersToCorners = new Tensor<float>(new TensorShape(4, 4),
        new float[]
        {
            1, 0, 1, 0,
            0, 1, 0, 1,
            -0.5f, 0, 0.5f, 0,
            0, -0.5f, 0, 0.5f
        });

        var graph = new FunctionalGraph();
        var inputs = graph.AddInputs(model1);
        var modelOutput = FF.Forward(model1, inputs)[0];
        var boxCoords = modelOutput[0, 0..4, ..].Transpose(0, 1);
        var allScores = modelOutput[0, 4.., ..];
        var scores = FF.ReduceMax(allScores, 0);
        var classIDs = FF.ArgMax(allScores, 0);
        var boxCorners = FF.MatMul(boxCoords, FF.Constant(centersToCorners));
        var indices = FF.NMS(boxCorners, scores, iouThreshold, scoreThreshold);
        var coords = FF.IndexSelect(boxCorners, 0, indices);
        var labelIDs = FF.IndexSelect(classIDs, 0, indices);

        model = graph.Compile(coords, labelIDs);
        SetupEngine(model, backend);
        inputTensor = new Tensor<float>(new TensorShape(1, 3, imageHeight, imageWidth));
    }

   public void ExecuteML()
    {
        Debug.Log("🚀 YOLO started");

        // Check if inference is already running or if worker is not set up
        if (executionStarted || worker == null)
        {
             Debug.LogWarning("YOLO execution skipped: Already running or worker not initialized.");
             return;
        }
        if (inputTensor == null)
        {
            Debug.LogError("YOLO execution skipped: inputTensor is null.");
            return;
        }

        try // Add try-catch block for robustness during inference and tensor operations
        {
            // Ensure the input texture (targetRT) is valid before converting
            if (targetRT == null || !targetRT.IsCreated())
            {
                 Debug.LogError("YOLO execution skipped: targetRT is null or not created.");
                 return;
            }
TextureConverter.ToTensor(targetRT, inputTensor, new TextureTransform());
            executionSchedule = worker.ScheduleIterable(inputTensor);
            executionStarted = true;

            // --- Ensure the schedule runs to completion ---
            // This loop iterates through the schedule steps. For simple models it might be quick,
            // but it's the correct way to ensure the worker finishes processing.
            while (executionSchedule.MoveNext()) { }
            // --- Execution Complete ---

            Debug.Log("✅ YOLO inference finished");

            // --- Corrected Output Processing ---
            // Peek and read back the output tensors. Use ReadbackAndClone to get data on CPU.
            // Output 0 should be the bounding box coordinates after NMS
            // Output 1 should be the corresponding class IDs after NMS
            using var outputCoordsTensor = (worker.PeekOutput("output_0") as Tensor<float>);
            using var labelIDsTensor = (worker.PeekOutput("output_1") as Tensor<int>);

            // Check if tensors are null before attempting to read
             if (outputCoordsTensor == null || labelIDsTensor == null)
             {
                 Debug.LogError("Failed to retrieve output tensors from the model worker.");
                 executionStarted = false; // Reset flag
                 return;
             }

            // Clone tensors to CPU memory
            using var outputCoords = outputCoordsTensor.ReadbackAndClone();
            using var labelIDs = labelIDsTensor.ReadbackAndClone();


            Debug.Log($"📦 Boxes detected after NMS: {outputCoords.shape[0]}");
             // Optional: Log shape for debugging, check if it's as expected (e.g., [num_boxes, 4])
             if (outputCoords.shape.rank > 1)
             {
                  Debug.Log($"Tensor shape outputCoords: {outputCoords.shape}");
             }
             if (labelIDs.shape.rank > 0)
             {
                  Debug.Log($"Tensor shape labelIDs: {labelIDs.shape}");
             }


            // Get the actual display size from the RawImage RectTransform
            // This is crucial for positioning calculations in DrawBox
            float displayWidthUI = displayImage.rectTransform.rect.width;   // UI element width
            float displayHeightUI = displayImage.rectTransform.rect.height; // UI element height

            // Track the closest box to the center (of the original 640x640 image space)
            float closestDistance = float.MaxValue;
            BoundingBox? closestBox = null; // Use nullable struct to track if a valid box is found

            // Define the central detection zone boundaries based on imageWidth, imageHeight and detectionZoneRatio
            float zoneHalfWidth = (imageWidth * detectionZoneRatio) / 2f;
            float zoneHalfHeight = (imageHeight * detectionZoneRatio) / 2f;
            float imageCenterX = imageWidth / 2f;
            float imageCenterY = imageHeight / 2f;

            float zoneMinX = imageCenterX - zoneHalfWidth;
            float zoneMaxX = imageCenterX + zoneHalfWidth;
            float zoneMinY = imageCenterY - zoneHalfHeight;
            float zoneMaxY = imageCenterY + zoneHalfHeight;

            int boxesFound = outputCoords.shape[0];
            ClearAnnotations(); // Clear any boxes drawn previously

            for (int n = 0; n < Mathf.Min(boxesFound, 200); n++) // Limit processed boxes if needed
            {
                // Read corner coordinates [x_min, y_min, x_max, y_max] from NMS output tensor
                float x_min = outputCoords[n, 0];
                float y_min = outputCoords[n, 1];
                float x_max = outputCoords[n, 2];
                float y_max = outputCoords[n, 3];

                // --- Calculate center, width, and height FROM corners ---
                float width = x_max - x_min;
                float height = y_max - y_min;
                float centerX = x_min + width * 0.5f;
                float centerY = y_min + height * 0.5f;

                // --- Basic sanity check for coordinates ---
                // Ensure dimensions are positive and coords are roughly within image bounds
                // Adjust tolerance (e.g., > 0.1f) if tiny boxes are problematic
                if (width <= 0f || height <= 0f ||
                    x_min < 0f || y_min < 0f || x_max < 0f || y_max < 0f || // Basic check
                    x_max > imageWidth || y_max > imageHeight || x_min > imageWidth || y_min > imageHeight) // Bounds check
                {
                    Debug.LogWarning($"Invalid or out-of-bounds box coordinates skipped at index {n}: xmin={x_min:F1}, ymin={y_min:F1}, xmax={x_max:F1}, ymax={y_max:F1}, w={width:F1}, h={height:F1}");
                    continue; // Skip this invalid or out-of-bounds box
                }

                // --- NEW: Check if the box center is within the defined detection zone ---
                bool isInDetectionZone = centerX >= zoneMinX && centerX <= zoneMaxX &&
                                           centerY >= zoneMinY && centerY <= zoneMaxY;

                if (!isInDetectionZone)
                {
                    // Optional: Log that the box was skipped
                    // Debug.Log($"Box {n} ({labels[labelIDs[n]]}) at ({centerX:F1}, {centerY:F1}) skipped: outside detection zone [{zoneMinX:F1}-{zoneMaxX:F1}, {zoneMinY:F1}-{zoneMaxY:F1}].");
                    continue; // Skip this box if it's not in the detection zone
                }
                // --- End of new check ---

                // Calculate distance from the center of the *original 640x640 image space*
                // This helps pick the object perceived as most central in the capture
                float deltaX = Mathf.Abs(centerX - (imageWidth * 0.5f));
                float deltaY = Mathf.Abs(centerY - (imageHeight * 0.5f));
                float distanceFromCenter = Mathf.Sqrt((deltaX * deltaX) + (deltaY * deltaY));

                // If this box is closer to the center than the previous closest valid box
                if (distanceFromCenter < closestDistance)
                {
                    closestDistance = distanceFromCenter;
                    int labelIndex = labelIDs[n]; // Get the class ID for this box

                    // Ensure labelIndex is valid
                    if (labelIndex < 0 || labelIndex >= labels.Length)
                    {
                         Debug.LogWarning($"Invalid label index {labelIndex} detected for box {n}. Skipping.");
                         continue; // Skip if label index is bad
                    }


                    closestBox = new BoundingBox
                    {
                        // Store the CALCULATED center, width, height
                        centerX = centerX,
                        centerY = centerY,
                        width = width,
                        height = height,
                        label = labels[labelIndex],
                        origCenterX = centerX, // Store the calculated center for logging in DrawBox
                        origCenterY = centerY  // Store the calculated center for logging in DrawBox
                    };
                }
            } // End of loop through boxes

            // --- Draw the closest box if one was found ---
            if (closestBox.HasValue)
            {
                // Determine font size dynamically or use a fixed value
                // Example: 5% of UI height, clamped between 14 and 40
                float fontSize = Mathf.Clamp(displayHeightUI * 0.05f, 14f, 40f);

                // Pass the BoundingBox struct containing the CORRECT center, width, height
                // DrawBox will handle mirroring and scaling to the UI coordinates
                DrawBox(closestBox.Value, 0, fontSize); // Using ID 0 as we only draw one box
                Debug.Log($"✓ Showing closest object: {closestBox.Value.label}");
            }
            else
            {
                Debug.Log("No valid objects detected or none met the criteria (e.g., closest to center).");
            }
             // --- End Corrected Output Processing ---

        }
        catch (System.Exception e) // Catch potential errors during inference/processing
        {
            Debug.LogError($"Error during ExecuteML: {e.Message}\n{e.StackTrace}");
        }
        finally // Ensure executionStarted is reset even if an error occurs
        {
             executionStarted = false; // Reset flag allowing next execution
        }
    }

    public void DrawBox(BoundingBox box, int id, float fontSize)
{
    GameObject panel = (id < boxPool.Count) ? boxPool[id] : CreateNewBox(Color.yellow);
    panel.SetActive(true);

    RectTransform displayRectTransform = displayImage.rectTransform;
    Vector2 displaySize = displayRectTransform.rect.size;

    // Original coordinates from the model (0 to imageWidth/imageHeight)
    float origX = box.centerX;
    float origY = box.centerY;
    float origW = box.width;
    float origH = box.height;

    // --- Coordinate Transformation ---

    // 1. Handle Front Camera Mirroring FIRST
    // Flip the horizontal coordinate if using the front camera.
    if (cameraType == CameraTypes.FRONT)
    {
        origX = imageWidth - origX;
    }

    // 2. Normalize coordinates (0 to 1) relative to the model input size
    float normalizedX = origX / imageWidth;
    float normalizedY = origY / imageHeight;
    float normalizedW = origW / imageWidth;
    float normalizedH = origH / imageHeight;

    // 3. Calculate the position within the RawImage RectTransform
    // The RawImage is centered (pivot 0.5, 0.5), so (0,0) in anchoredPosition is the center.
    // We map the normalized coordinates to the displaySize.
    // Note: UI Y-coordinates are typically inverted (positive Y is up),
    // while image Y-coordinates are often top-down (positive Y is down).
    // The model output likely follows the image convention (Y=0 is top).
    // So, we convert normalizedY to UI space: (0.5 - normalizedY) maps top (0) to +0.5*height
    // and bottom (1) to -0.5*height.
    float uiPosX = (normalizedX - 0.5f) * displaySize.x;
    float uiPosY = (0.5f - normalizedY) * displaySize.y; // Invert Y-axis

    // 4. Calculate the size in UI units
    float uiWidth = normalizedW * displaySize.x;
    float uiHeight = normalizedH * displaySize.y;

    // --- Apply to UI Elements ---

    RectTransform rt = panel.GetComponent<RectTransform>();
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

    label.text = box.label;
    label.fontSize = Mathf.Max((int)fontSize, 14); // Ensure minimum readable size
    label.color = Color.yellow;
    label.alignment = TextAlignmentOptions.Center;

    // Make sure material is set for the text
    if (label.material == null && tmpFont != null) // Check tmpFont isn't null
    {
        // Attempt to get/create material
        try
        {
             label.fontMaterial = tmpFont.material; // Assign directly if possible or use a standard method
        }
        catch (System.Exception ex)
        {
             Debug.LogError($"Failed to assign material: {ex.Message}");
             // Fallback or default material assignment might be needed here
             // e.g., label.material = TMP_MaterialManager.GetMaterialForRendering(label, 0);
        }

    } else if (tmpFont == null) {
         Debug.LogWarning("TMP Font Asset not assigned in the inspector!");
    }


    // Create background for better visibility (Simplified - consider adjusting if needed)
    Image bgImage = label.GetComponentInChildren<Image>(); // Assumes background is child Image
     if (bgImage == null) // Check if background needs creation
     {
        GameObject background = new GameObject("LabelBackground");
        background.transform.SetParent(label.transform, false); // Set parent before adding component
        background.transform.SetAsFirstSibling(); // Put behind text

        bgImage = background.AddComponent<Image>(); // Now add Image component
        bgImage.color = new Color(0, 0, 0, 0.5f); // Semi-transparent black

        RectTransform bgRT = background.GetComponent<RectTransform>();
        bgRT.anchorMin = new Vector2(0, 0);
        bgRT.anchorMax = new Vector2(1, 1);
        bgRT.offsetMin = new Vector2(-5, -2); // Padding X-min, Y-min
        bgRT.offsetMax = new Vector2(5, 2);  // Padding X-max, Y-max (-ve values pull inwards)
     }


    // Adjust label position to appear at the top of the bounding box
    RectTransform labelRT = label.GetComponent<RectTransform>();
    labelRT.anchorMin = new Vector2(0.5f, 1); // Anchor to Top-Center of parent (the box panel)
    labelRT.anchorMax = new Vector2(0.5f, 1);
    labelRT.pivot = new Vector2(0.5f, 0);    // Pivot at Bottom-Center of the label itself
    labelRT.anchoredPosition = new Vector2(0, 5); // Position 5 units above the anchor
    // Adjust label size dynamically or set a fixed reasonable size
    labelRT.sizeDelta = new Vector2(Mathf.Max(uiWidth, 100), Mathf.Max(uiHeight * 0.2f, 30)); // Example: 20% of box height, min 30

    // Debugging Log
    Debug.Log($"🎯 Drawing Box: {box.label} | Orig Center: ({box.origCenterX:F1}, {box.origCenterY:F1}) | Mirrored Center: ({origX:F1}, {origY:F1}) | UI Pos: ({uiPosX:F1}, {uiPosY:F1}) | UI Size: ({uiWidth:F1}x{uiHeight:F1})");
}

    public GameObject CreateNewBox(Color color)
    {
        var panel = new GameObject("ObjectBox");
        panel.AddComponent<CanvasRenderer>();
        Image img = panel.AddComponent<Image>();
        img.color = color;
        img.sprite = borderSprite;
        img.type = Image.Type.Sliced;
        panel.transform.SetParent(displayLocation, false);

        var text = new GameObject("ObjectLabel");
        text.AddComponent<CanvasRenderer>();
        text.transform.SetParent(panel.transform, false);

        TextMeshProUGUI txt = text.AddComponent<TextMeshProUGUI>();
        if (tmpFont != null)
        {
            txt.font = tmpFont;
        }
        else
        {
            Debug.LogWarning("TMP Font Asset not assigned in the inspector!");
        }
        txt.color = Color.yellow;
        txt.fontSize = 40;
        txt.alignment = TextAlignmentOptions.Center;
        txt.enableWordWrapping = false;
        txt.overflowMode = TextOverflowModes.Overflow;

        RectTransform rt2 = text.GetComponent<RectTransform>();
        rt2.anchorMin = new Vector2(0.5f, 1);
        rt2.anchorMax = new Vector2(0.5f, 1);
        rt2.pivot = new Vector2(0.5f, 0);
        rt2.anchoredPosition = new Vector2(0, 5);
        rt2.sizeDelta = new Vector2(100, 30);

        boxPool.Add(panel);
        return panel;
    }

    public void ClearAnnotations()
    {
        foreach (var box in boxPool)
        {
            box.SetActive(false);
        }
    }

    private void SetupEngine(Model model, BackendType backendType)
    {
        if (worker != null)
        {
            worker.Dispose();
        }
        worker = new Worker(model, backendType);
    }

    void Update()
    {
        HandleCameraDisplay();
    }

    private void UpdateDetectionZoneVisual()
    {
        if (detectionZoneOverlay == null || displayImage == null)
        {
            if (detectionZoneOverlay == null) Debug.LogWarning("Detection Zone Overlay UI element not assigned.");
            return;
        }

        RectTransform displayRectTransform = displayImage.rectTransform;
        float displayWidthUI = displayRectTransform.rect.width;
        float displayHeightUI = displayRectTransform.rect.height;

        float zoneWidthUI = displayWidthUI * detectionZoneRatio;
        float zoneHeightUI = displayHeightUI * detectionZoneRatio;

        detectionZoneOverlay.rectTransform.sizeDelta = new Vector2(zoneWidthUI, zoneHeightUI);
        // Assuming the overlay is a child of displayImage or another centered element,
        // and its pivot is also centered (0.5, 0.5).
        detectionZoneOverlay.rectTransform.anchoredPosition = Vector2.zero; 
        detectionZoneOverlay.gameObject.SetActive(true);
    }

    private void HandleCameraDisplay()
    {
        if (!isCamAvailable || activeCameraTexture == null || !activeCameraTexture.isPlaying) 
            return;

        // Wait for camera to initialize properly
        if (activeCameraTexture.width < 100)
        {
            return;
        }

        // Rotate image to show correct orientation
        rotationVector.z = -activeCameraTexture.videoRotationAngle;
        displayImage.rectTransform.localEulerAngles = rotationVector;

        // Handle screen and camera aspect ratio
        float camWidth = (float)activeCameraTexture.width;
        float camHeight = (float)activeCameraTexture.height;
        float screenWidth = (Screen.width / Screen.dpi) * screenSizeRatio * 96f; // Convert to pixels at 96dpi
        float screenHeight = (Screen.height / Screen.dpi) * screenSizeRatio * 96f;

        // Apply maximum size constraints
        screenWidth = Mathf.Min(screenWidth, maxDisplayWidth);
        screenHeight = Mathf.Min(screenHeight, maxDisplayHeight);

        float camAspect = camWidth / camHeight;
        float screenAspect = screenWidth / screenHeight;

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
        displayImage.uvRect = activeCameraTexture.videoVerticallyMirrored ? fixedRect : defaultRect;

        UpdateDetectionZoneVisual(); // Update the detection zone visual whenever the displayImage changes
    }
}