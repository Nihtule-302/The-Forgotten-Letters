using UnityEngine;
using Unity.Sentis;
public class ClassifyHandwrittenArabicLetters : HandwrittenClassifier
{
    [SerializeField] private Texture2D inputTexture;
    [SerializeField] private ModelAsset modelAsset;
    [SerializeField] private letterProbability[] letterProbability = new letterProbability[28]; // Initialized size here
    [SerializeField] private char predictedLetter;
    [SerializeField] private float probability;
    [SerializeField] private FingerDrawing fingerDrawing;

    Worker engine;
    [SerializeField] BackendType backendType = BackendType.GPUCompute;
    const int imageWidth = 32;
    Tensor<float> inputTensor = null;

    char[] arabic_chars = new char[]{
        'ا', 'ب', 'ت', 'ث', 'ج', 'ح', 'خ', 'د', 'ذ', 'ر',
        'ز', 'س', 'ش', 'ص', 'ض', 'ط', 'ظ', 'ع', 'غ', 'ف',
        'ق', 'ك', 'ل', 'م', 'ن', 'ه', 'و', 'ي'
    };

    void Start()
    {
        Model model = ModelLoader.Load(modelAsset);

        var graph = new FunctionalGraph();
        inputTensor = new Tensor<float>(new TensorShape(1, imageWidth, imageWidth, 1));
        var input = graph.AddInput(DataType.Float, new TensorShape(1, imageWidth, imageWidth, 1));
        var outputs = Functional.Forward(model, input);
        var result = outputs[0];

        var indexOfMaxProba = Functional.ArgMax(result, -1, false);
        model = graph.Compile(result, indexOfMaxProba);

        engine = new Worker(model, backendType);

        // Make sure letterProbability is initialized
        letterProbability = new letterProbability[28]; // Reset array size in case of re-initialization

        // ExecuteModel();
    }

    [ContextMenu("ExecuteModel")]
    public override void ExecuteModel(Texture2D tex)
    {
        //inputTensor = PreprocessImage(inputTexture,imageWidth);
        // Prepare input tensor for the model
        var transform = new TextureTransform().SetTensorLayout(TensorLayout.NHWC).SetDimensions(imageWidth,imageWidth,1);
        TextureConverter.ToTensor(tex, inputTensor, transform);

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

    // Helper function to rotate the image 90 degrees counter-clockwise
    Texture2D transpose(Texture2D texture)
    {
        Texture2D transpoedTexture = new Texture2D(texture.height, texture.width);

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                transpoedTexture.SetPixel(y, x, texture.GetPixel(x, y));
            }
        }

        transpoedTexture.Apply();
        return transpoedTexture;
    }


}



[System.Serializable]
public class letterProbability
{
    public char letter;
    public float probability;
}