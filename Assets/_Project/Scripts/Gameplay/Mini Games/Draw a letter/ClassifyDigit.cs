using Unity.Sentis;
using UnityEngine;

public class Classifier : HandwrittenClassifier
{
    [SerializeField] private Texture2D inputTexture;
    [SerializeField] private ModelAsset modelAsset;
    [SerializeField] private float[] results;
    [SerializeField] private FingerDrawing fingerDrawing;

    [SerializeField] private int predictedDigit;
    [SerializeField] private float probability;

    Worker engine;
    Tensor<float> inputTensor = null;

    [SerializeField] BackendType backendType = BackendType.GPUCompute;
    const int imageWidth = 28;

    void Start()
    {
        Model model = ModelLoader.Load(modelAsset);

        var graph = new FunctionalGraph();
        var input = graph.AddInput(DataType.Float, new TensorShape(1, 1, imageWidth, imageWidth));
        var outputs = Functional.Forward(model, input);
        var result = outputs[0];

        var probabilititys = Functional.Softmax(result);
        var indexOfMaxProba = Functional.ArgMax(result, -1, false);
        model = graph.Compile(probabilititys, indexOfMaxProba);

        engine = new Worker(model, backendType);

        // ExecuteModel();
    }

    [ContextMenu("ExecuteModel")]
    public override void ExecuteModel(Texture2D tex)
    {
        inputTensor?.Dispose();
        inputTensor = new Tensor<float>(new TensorShape(1, 1, imageWidth, imageWidth));
        // inputTensor = PreprocessImage(inputTexture,imageWidth);
        // Prepare input tensor for the model
        var transform = new TextureTransform().SetDimensions(imageWidth,imageWidth,1);
        TextureConverter.ToTensor(tex, inputTensor, transform);

        // Schedule the input tensor for execution
        engine.Schedule(inputTensor);

        // Read back the result from the GPU
        using var probabilities = (engine.PeekOutput(0) as Tensor<float>).ReadbackAndClone();
        using var indexOfMaxProba = (engine.PeekOutput(1) as Tensor<int>).ReadbackAndClone();

        results = probabilities.DownloadToArray();

        // Get the predicted letter and probability
        probability = probabilities[indexOfMaxProba[0]];
        predictedDigit = indexOfMaxProba[0];

        Debug.Log($"predictedNumber {indexOfMaxProba[0]}");
        Debug.Log($"probability {probability}");
        fingerDrawing.ClearTexture();
    }

    void OnDisable()
    {
        // Clean up Sentis resources.
        engine.Dispose();
        inputTensor?.Dispose();
    }

    Tensor<float> PreprocessImage(Texture2D sourceTexture, int imageWidth)
    {

        // Rotate 90 degrees counter-clockwise
        // Texture2D transpoedTexture = transpose(sourceTexture);

        // Process the pixels of the rotated and flipped image
        Color32[] pixels = sourceTexture.GetPixels32();
        float[] grayscalePixels = new float[imageWidth * imageWidth];

        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 pixel = pixels[i];

            // Convert to grayscale using luminance formula
            float gray = 0.299f * pixel.r + 0.587f * pixel.g + 0.114f * pixel.b;

            // Normalize to [0, 1]
            grayscalePixels[i] = gray / 255f;
        }

        // Shape: [batch, height, width, channels] → [1, imageWidth, imageWidth, 1]
        TensorShape shape = new TensorShape(1, imageWidth, imageWidth, 1);
        return new Tensor<float>(shape, grayscalePixels);
    }
}
