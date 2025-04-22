using UnityEngine;
using Unity.Sentis;
using UnityEngine.UI;
using _Project.Scripts.Core.Scriptable_Events.EventTypes.String;
using _Project.Scripts.Core.Utilities.UI;
using TMPro;
using System;
public class ClassifyHandwrittenArabicLetters : HandwrittenClassifier
{
    [SerializeField] private Texture2D inputTexture;
    [SerializeField] private ModelAsset modelAsset;
    [SerializeField] private letterProbability[] letterProbability = new letterProbability[28]; // Initialized size here
    [SerializeField] private char predictedLetter;
    [SerializeField] private float probability;
    [SerializeField] private FingerDrawing fingerDrawing;

    [SerializeField] private RawImage inputTexturePreSendModelScreen;

    [SerializeField] private StringEvent confidenceEvent;
    [SerializeField] private StringEvent predictedLetterEvent;

    Worker engine;
    Model model;
    [SerializeField] BackendType backendType = BackendType.GPUCompute;
    [SerializeField] TMP_Dropdown backendTypeDropdown;
    const int imageWidth = 32;
    Tensor<float> inputTensor = null;

    char[] arabic_chars = new char[]{
        'ا', 'ب', 'ت', 'ث', 'ج', 'ح', 'خ', 'د', 'ذ', 'ر',
        'ز', 'س', 'ش', 'ص', 'ض', 'ط', 'ظ', 'ع', 'غ', 'ف',
        'ق', 'ك', 'ل', 'م', 'ن', 'ه', 'و', 'ي'
    };

    void Start()
    {
        model = ModelLoader.Load(modelAsset);

        var graph = new FunctionalGraph();
        inputTensor = new Tensor<float>(new TensorShape(1, imageWidth, imageWidth, 1));
        var input = graph.AddInput(DataType.Float, new TensorShape(1, imageWidth, imageWidth, 1));
        var outputs = Functional.Forward(model, input);
        var result = outputs[0];

        var indexOfMaxProba = Functional.ArgMax(result, -1, false);
        model = graph.Compile(result, indexOfMaxProba);

        SetupEngine(model, backendType);

        // Make sure letterProbability is initialized
        letterProbability = new letterProbability[28]; // Reset array size in case of re-initialization

    }

    [ContextMenu("ExecuteModel")]
    public void ExecuteModel()
    {
        inputTensor?.Dispose();
        var transform = new TextureTransform().SetTensorLayout(TensorLayout.NHWC).SetDimensions(imageWidth,imageWidth,1);
        TextureConverter.ToTensor(inputTexture, inputTensor, transform);

        // Schedule the input tensor for execution
        engine.Schedule(inputTensor);

        // Read back the result from the GPU
        using var probabilities = (engine.PeekOutput(0) as Tensor<float>).ReadbackAndClone();
        using var indexOfMaxProba = (engine.PeekOutput(1) as Tensor<int>).ReadbackAndClone();

        // Get the predicted letter and probability
        predictedLetter = arabic_chars[indexOfMaxProba[0]];
        probability = probabilities[indexOfMaxProba[0]];

        // Populate the letterProbability array with letters and their corresponding probabilities
        for (int i = 0; i < probabilities.count; i++)
        {
            letterProbability[i] = new letterProbability();
            letterProbability[i].letter = arabic_chars[i];
            letterProbability[i].probability = probabilities[i];
        }

        Debug.Log($"predictedLetter {predictedLetter}");
        Debug.Log($"probability {probability}");

        predictedLetterEvent.Raise($"{predictedLetter}");
        confidenceEvent.Raise($"{probability * 100:F2}%");
        
        fingerDrawing.ClearTexture();
    }

    public override void ExecuteModel(Texture2D tex)
    {
        inputTensor?.Dispose();

        var downsampeldTex = TextureDownsampling.DownsampleTexture(tex,imageWidth, imageWidth);
        TextureDownsampling.DisplayDownsampledTexture(downsampeldTex, inputTexturePreSendModelScreen);

        var transform = new TextureTransform().SetTensorLayout(TensorLayout.NHWC).SetDimensions(imageWidth,imageWidth,1);
        TextureConverter.ToTensor(downsampeldTex, inputTensor, transform);


        engine.Schedule(inputTensor);

        // Read back the result from the GPU
        using var probabilities = (engine.PeekOutput(0) as Tensor<float>).ReadbackAndClone();
        using var indexOfMaxProba = (engine.PeekOutput(1) as Tensor<int>).ReadbackAndClone();

        // Get the predicted letter and probability
        predictedLetter = arabic_chars[indexOfMaxProba[0]];
        probability = probabilities[indexOfMaxProba[0]];

        // Populate the letterProbability array with letters and their corresponding probabilities
        for (int i = 0; i < probabilities.count; i++)
        {
            letterProbability[i] = new letterProbability();
            letterProbability[i].letter = arabic_chars[i];
            letterProbability[i].probability = probabilities[i];
        }

        Debug.Log($"predictedLetter {predictedLetter}");
        Debug.Log($"probability {probability}");

        predictedLetterEvent.Raise($"{predictedLetter}");
        confidenceEvent.Raise($"{probability * 100:F2}%");

        fingerDrawing.ClearTexture();
    }

    void OnDisable()
    {
        // Clean up Sentis resources.
        engine.Dispose();
        inputTensor?.Dispose();
    }

    public void OnDropdownValueChanged(int index)
    {
        string selectedOption = backendTypeDropdown.options[index].text;
        backendType = (BackendType)Enum.Parse(typeof(BackendType), selectedOption);
        SetupEngine(model, backendType);
    }

    private void SetupEngine(Model model, BackendType backendType)
    {
        if (engine != null)
        {
            engine.Dispose();
        }

        engine = new Worker(model, backendType);
    }
}



[System.Serializable]
public class letterProbability
{
    public char letter;
    public float probability;
}

public static class TextureDownsampling
{
    // Static method to downsample a given texture
    public static Texture2D DownsampleTexture(Texture2D original, int targetWidth, int targetHeight)
    {
        // Create a RenderTexture with the target width and height
        RenderTexture renderTexture = new RenderTexture(targetWidth, targetHeight, 0);
        renderTexture.filterMode = FilterMode.Bilinear;
        renderTexture.Create();

        // Copy original texture to RenderTexture, resizing it in the process
        Graphics.Blit(original, renderTexture);

        // Create a new Texture2D to hold the downsampled image
        Texture2D downsampledTexture = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
        
        // Set active RenderTexture and read pixels back into the new Texture2D
        RenderTexture.active = renderTexture;
        downsampledTexture.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        downsampledTexture.Apply();

        // Clean up
        RenderTexture.active = null;
        renderTexture.Release();

        return downsampledTexture;
    }

    // Static method to display the downsampled texture on a material
    public static void DisplayDownsampledTexture(Texture2D downsampledTexture, Material displayMaterial)
    {
        if (displayMaterial != null)
        {
            displayMaterial.mainTexture = downsampledTexture;
        }
    }

    public static void DisplayDownsampledTexture(Texture2D downsampledTexture, RawImage image)
    {
        if (image != null)
        {
            image.texture = downsampledTexture;
        }
    }
}