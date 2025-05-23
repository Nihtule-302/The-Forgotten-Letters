using _Project.Scripts.Core.DataTypes;
using _Project.Scripts.Core.Scriptable_Events.EventTypes.LetterData;
using _Project.Scripts.Core.Scriptable_Events.EventTypes.String;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Mini_Games.Draw_a_letter
{
    public class ClassifyHandwrittenArabicLetters : HandwrittenClassifier
    {
        private const int ImageWidth = 32;

        [Header("Refearences")] [SerializeField]
        private FingerDrawing fingerDrawing;

        [Header("Letter Prediction Data")] [SerializeField]
        private LetterPredictionData[] letterProbability = new LetterPredictionData[28]; // Initialized size here

        [Header("Model")] [SerializeField] private ModelAsset modelAsset;

        [SerializeField] private BackendType backendType = BackendType.GPUCompute;

        [Header("Events")] [SerializeField] private AssetReference letterPredictionDataEventRef;

        [SerializeField] private AssetReference confidenceEventRef;
        [SerializeField] private AssetReference predictedLetterEventRef;

        private readonly char[] _arabicChars =
        {
            'ا', 'ب', 'ت', 'ث', 'ج', 'ح', 'خ', 'د', 'ذ', 'ر',
            'ز', 'س', 'ش', 'ص', 'ض', 'ط', 'ظ', 'ع', 'غ', 'ف',
            'ق', 'ك', 'ل', 'م', 'ن', 'ه', 'و', 'ي'
        };

        private Worker _engine;
        private Tensor<float> _inputTensor;
        private Texture2D _inputTexture;
        private RawImage _inputTexturePreSendModelScreen;
        private Model _model;
        private char _predictedLetter;
        private float _probability;

        private LetterPredictionDataEvent LetterPredictionDataEvent =>
            EventLoader.Instance.GetEvent<LetterPredictionDataEvent>(letterPredictionDataEventRef);

        private StringEvent ConfidenceEvent => EventLoader.Instance.GetEvent<StringEvent>(confidenceEventRef);
        private StringEvent PredictedLetterEvent => EventLoader.Instance.GetEvent<StringEvent>(predictedLetterEventRef);

        private void Awake()
        {
            _model = ModelLoader.Load(modelAsset);

            var graph = new FunctionalGraph();
            _inputTensor = new Tensor<float>(new TensorShape(1, ImageWidth, ImageWidth, 1));
            var input = graph.AddInput(DataType.Float, new TensorShape(1, ImageWidth, ImageWidth, 1));
            var outputs = Functional.Forward(_model, input);
            var result = outputs[0];

            var indexOfMaxProba = Functional.ArgMax(result, -1);
            _model = graph.Compile(result, indexOfMaxProba);

            SetupEngine(_model, backendType);

            // Make sure letterProbability is initialized
            letterProbability = new LetterPredictionData[28]; // Reset array size in case of re-initialization
        }

        private void OnEnable()
        {
            // Reinitialize the engine when the object is enabled.
            if (_engine == null) SetupEngine(_model, backendType);
        }

        private void OnDestroy()
        {
            // Clean up Sentis resources.
            _engine.Dispose();
            _inputTensor?.Dispose();
        }

        [ContextMenu("ExecuteModel")]
        public void ExecuteModel()
        {
            _inputTensor?.Dispose();
            var transform = new TextureTransform().SetTensorLayout(TensorLayout.NHWC)
                .SetDimensions(ImageWidth, ImageWidth, 1);
            TextureConverter.ToTensor(_inputTexture, _inputTensor, transform);

            // Schedule the input tensor for execution
            _engine.Schedule(_inputTensor);

            // Read back the result from the GPU
            using var probabilities = (_engine.PeekOutput(0) as Tensor<float>).ReadbackAndClone();
            using var indexOfMaxProba = (_engine.PeekOutput(1) as Tensor<int>).ReadbackAndClone();

            // Get the predicted letter and probability

            var letterPredictionData = new LetterPredictionData();
            letterPredictionData.letter = _arabicChars[indexOfMaxProba[0]];
            letterPredictionData.probability = probabilities[indexOfMaxProba[0]];

            _predictedLetter = letterPredictionData.letter;
            _probability = letterPredictionData.probability;


            // Populate the letterProbability array with letters and their corresponding probabilities
            for (var i = 0; i < probabilities.count; i++)
            {
                letterProbability[i] = new LetterPredictionData();
                letterProbability[i].letter = _arabicChars[i];
                letterProbability[i].probability = probabilities[i];
            }

            Debug.Log(letterPredictionData.ToString());

            PredictedLetterEvent.Raise($"{_predictedLetter}");
            ConfidenceEvent.Raise($"{_probability * 100:F2}%");
            LetterPredictionDataEvent.Raise(letterPredictionData);

            fingerDrawing.ClearTexture();
        }

        public override void ExecuteModel(Texture2D tex)
        {
            _inputTensor?.Dispose();

            var downsampeldTex = TextureDownsampling.DownsampleTexture(tex, ImageWidth, ImageWidth);
            TextureDownsampling.DisplayDownsampledTexture(downsampeldTex, _inputTexturePreSendModelScreen);

            var transform = new TextureTransform().SetTensorLayout(TensorLayout.NHWC)
                .SetDimensions(ImageWidth, ImageWidth, 1);
            TextureConverter.ToTensor(downsampeldTex, _inputTensor, transform);


            _engine.Schedule(_inputTensor);

            // Read back the result from the GPU
            using var probabilities = (_engine.PeekOutput(0) as Tensor<float>).ReadbackAndClone();
            using var indexOfMaxProba = (_engine.PeekOutput(1) as Tensor<int>).ReadbackAndClone();

            // Get the predicted letter and probability

            var letterPredictionData = new LetterPredictionData();
            letterPredictionData.letter = _arabicChars[indexOfMaxProba[0]];
            letterPredictionData.probability = probabilities[indexOfMaxProba[0]];

            _predictedLetter = letterPredictionData.letter;
            _probability = letterPredictionData.probability;

            // Populate the letterProbability array with letters and their corresponding probabilities
            for (var i = 0; i < probabilities.count; i++)
            {
                letterProbability[i] = new LetterPredictionData();
                letterProbability[i].letter = _arabicChars[i];
                letterProbability[i].probability = probabilities[i];
            }

            Debug.Log(letterPredictionData.ToString());

            PredictedLetterEvent.Raise($"{_predictedLetter}");
            ConfidenceEvent.Raise($"{_probability:F2}%");
            LetterPredictionDataEvent.Raise(letterPredictionData);

            fingerDrawing.ClearTexture();
        }

        private void SetupEngine(Model model, BackendType backendType)
        {
            if (_engine != null) _engine.Dispose();

            _engine = new Worker(model, backendType);
        }
    }

    public static class TextureDownsampling
    {
        // Static method to downsample a given texture
        public static Texture2D DownsampleTexture(Texture2D original, int targetWidth, int targetHeight)
        {
            // Create a RenderTexture with the target width and height
            var renderTexture = new RenderTexture(targetWidth, targetHeight, 0);
            renderTexture.filterMode = FilterMode.Bilinear;
            renderTexture.Create();

            // Copy original texture to RenderTexture, resizing it in the process
            Graphics.Blit(original, renderTexture);

            // Create a new Texture2D to hold the downsampled image
            var downsampledTexture = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);

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
            if (displayMaterial != null) displayMaterial.mainTexture = downsampledTexture;
        }

        public static void DisplayDownsampledTexture(Texture2D downsampledTexture, RawImage image)
        {
            if (image != null) image.texture = downsampledTexture;
        }
    }
}