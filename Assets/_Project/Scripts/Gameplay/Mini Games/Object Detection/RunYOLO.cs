using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using FF = Unity.Sentis.Functional;

/*
 *  YOLO Inference Script
 *  ========================
 *
 * Place this script on the Main Camera and set the script parameters according to the tooltips.
 *
 */

namespace _Project.Scripts.Gameplay.Mini_Games.Object_Detection
{
    public class RunYolo : MonoBehaviour
    {
        //Image size for the model
        private const int ImageWidth = 640;
        private const int ImageHeight = 640;

        [Tooltip("Drag a YOLO model .onnx file here")]
        public ModelAsset modelAsset;

        [Tooltip("Drag the classes.txt here")] public TextAsset classesAsset;

        [Tooltip("Create a Raw Image in the scene and link it here")]
        public RawImage displayImage;

        [Tooltip("Drag a border box texture here")]
        public Texture2D borderTexture;

        [Tooltip("Select an appropriate font for the labels")]
        public Font font;

        [Tooltip("Change this to the name of the video you put in the Assets/StreamingAssets folder")]
        public string videoFilename = "giraffes.mp4";

        public int framesToExectute = 2;

        [SerializeField] private BackendType backend = BackendType.CPU;
        [SerializeField] private TMP_Dropdown backendTypeDropdown;

        [Tooltip("Intersection over union threshold used for non-maximum suppression")] [SerializeField] [Range(0, 1)]
        private float iouThreshold = 0.5f;

        [Tooltip("Confidence score threshold used for non-maximum suppression")] [SerializeField] [Range(0, 1)]
        private float scoreThreshold = 0.5f;

        [SerializeField] private int timeSlicingFrameFactor = 1;

        private readonly List<GameObject> _boxPool = new();
        private Sprite _borderSprite;

        private Tensor<float> _centersToCorners;

        private Transform _displayLocation;
        private IEnumerator _executionSchedule;

        private bool _executionStarted;


        private Tensor<float> _inputTensor;
        private string[] _labels;
        private Model _model;

        private int _modelLayerCount;
        private RenderTexture _targetRT;

        private VideoPlayer _video;
        private Worker _worker;


        private void Start()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            //Parse neural net labels
            _labels = classesAsset.text.Split('\n');

            LoadModel();

            _targetRT = new RenderTexture(ImageWidth, ImageHeight, 0);

            //Create image to display video
            _displayLocation = displayImage.transform;

            SetupInput();

            _borderSprite = Sprite.Create(borderTexture, new Rect(0, 0, borderTexture.width, borderTexture.height),
                new Vector2(borderTexture.width / 2, borderTexture.height / 2));
        }

        private void Update()
        {
            if (_video && _video.texture)
            {
                var aspect = _video.width * 1f / _video.height;
                Graphics.Blit(_video.texture, _targetRT, new Vector2(1f / aspect, 1), new Vector2(0, 0));
                displayImage.texture = _targetRT;
            }
            else
            {
                return;
            }

            if (Time.frameCount % timeSlicingFrameFactor == 0) ExecuteMl();
        }

        private void OnDestroy()
        {
            _centersToCorners?.Dispose();
            _worker?.Dispose();
        }

        private void LoadModel()
        {
            //Load model
            var model1 = ModelLoader.Load(modelAsset);

            _centersToCorners = new Tensor<float>(new TensorShape(4, 4),
                new[]
                {
                    1, 0, 1, 0,
                    0, 1, 0, 1,
                    -0.5f, 0, 0.5f, 0,
                    0, -0.5f, 0, 0.5f
                });

            //Here we transform the output of the model1 by feeding it through a Non-Max-Suppression layer.
            var graph = new FunctionalGraph();
            var inputs = graph.AddInputs(model1);
            var modelOutput = FF.Forward(model1, inputs)[0]; //shape=(1,84,8400)
            var boxCoords = modelOutput[0, ..4, ..].Transpose(0, 1); //shape=(8400,4)
            var allScores = modelOutput[0, 4.., ..]; //shape=(80,8400)
            var scores = FF.ReduceMax(allScores, 0); //shape=(8400)
            var classIDs = FF.ArgMax(allScores); //shape=(8400)
            var boxCorners = FF.MatMul(boxCoords, FF.Constant(_centersToCorners)); //shape=(8400,4)
            var indices = FF.NMS(boxCorners, scores, iouThreshold, scoreThreshold); //shape=(N)
            var coords = boxCoords.IndexSelect(0, indices); //shape=(N,4)
            var labelIDs = classIDs.IndexSelect(0, indices); //shape=(N)

            _model = graph.Compile(coords, labelIDs);
            _modelLayerCount = _model.layers.Count;

            //Create worker to run model
            SetupEngine(_model, backend);

            _inputTensor = new Tensor<float>(new TensorShape(1, 3, ImageHeight, ImageWidth));
        }

        private void SetupInput()
        {
            _video = gameObject.GetComponent<VideoPlayer>();
            _video.renderMode = VideoRenderMode.APIOnly;
            _video.source = VideoSource.Url;
            _video.url = Path.Combine(Application.streamingAssetsPath, videoFilename);
            _video.isLooping = true;
            _video.Prepare();
            StartCoroutine(WaitForVideoPreparation());
        }

        private IEnumerator WaitForVideoPreparation()
        {
            while (!_video.isPrepared) yield return null;
            _video.Play();
        }

        public void ExecuteMl()
        {
            if (!_executionStarted)
            {
                TextureConverter.ToTensor(_targetRT, _inputTensor, default);
                _executionSchedule = _worker.ScheduleIterable(_inputTensor);
                _executionStarted = true;
            }

            var hasMoreWork = false;
            var layersToRun = (_modelLayerCount + framesToExectute - 1) / framesToExectute; // round up
            for (var i = 0; i < layersToRun; i++)
            {
                hasMoreWork = _executionSchedule.MoveNext();
                if (!hasMoreWork)
                    break;
            }

            if (hasMoreWork)
                return;

            using var output = (_worker.PeekOutput("output_0") as Tensor<float>).ReadbackAndClone();
            using var labelIDs = (_worker.PeekOutput("output_1") as Tensor<int>).ReadbackAndClone();

            var displayWidth = displayImage.rectTransform.rect.width;
            var displayHeight = displayImage.rectTransform.rect.height;

            var scaleX = displayWidth / ImageWidth;
            var scaleY = displayHeight / ImageHeight;

            var boxesFound = output.shape[0];
            ClearAnnotations();
            //Draw the bounding boxes
            for (var n = 0; n < Mathf.Min(boxesFound, 200); n++)
            {
                var box = new BoundingBox
                {
                    CenterX = output[n, 0] * scaleX - displayWidth / 2,
                    CenterY = output[n, 1] * scaleY - displayHeight / 2,
                    Width = output[n, 2] * scaleX,
                    Height = output[n, 3] * scaleY,
                    Label = _labels[labelIDs[n]]
                };
                DrawBox(box, n, displayHeight * 0.05f);
            }

            _executionStarted = false;
        }

        public void DrawBox(BoundingBox box, int id, float fontSize)
        {
            //Create the bounding box graphic or get from pool
            GameObject panel;
            if (id < _boxPool.Count)
            {
                panel = _boxPool[id];
                panel.SetActive(true);
            }
            else
            {
                panel = CreateNewBox(Color.yellow);
            }

            //Set box position
            panel.transform.localPosition = new Vector3(box.CenterX, -box.CenterY);

            //Set box size
            var rt = panel.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(box.Width, box.Height);

            //Set label text
            var label = panel.GetComponentInChildren<Text>();
            label.text = box.Label;
            label.fontSize = (int)fontSize;
        }

        public GameObject CreateNewBox(Color color)
        {
            //Create the box and set image

            var panel = new GameObject("ObjectBox");
            panel.AddComponent<CanvasRenderer>();
            var img = panel.AddComponent<Image>();
            img.color = color;
            img.sprite = _borderSprite;
            img.type = Image.Type.Sliced;
            panel.transform.SetParent(_displayLocation, false);

            //Create the label

            var text = new GameObject("ObjectLabel");
            text.AddComponent<CanvasRenderer>();
            text.transform.SetParent(panel.transform, false);
            var txt = text.AddComponent<Text>();
            txt.font = font;
            txt.color = color;
            txt.fontSize = 40;
            txt.horizontalOverflow = HorizontalWrapMode.Overflow;

            var rt2 = text.GetComponent<RectTransform>();
            rt2.offsetMin = new Vector2(20, rt2.offsetMin.y);
            rt2.offsetMax = new Vector2(0, rt2.offsetMax.y);
            rt2.offsetMin = new Vector2(rt2.offsetMin.x, 0);
            rt2.offsetMax = new Vector2(rt2.offsetMax.x, 30);
            rt2.anchorMin = new Vector2(0, 0);
            rt2.anchorMax = new Vector2(1, 1);

            _boxPool.Add(panel);
            return panel;
        }

        public void ClearAnnotations()
        {
            foreach (var box in _boxPool) box.SetActive(false);
        }

        public void OnDropdownValueChanged(int index)
        {
            var selectedOption = backendTypeDropdown.options[index].text;
            backend = (BackendType)Enum.Parse(typeof(BackendType), selectedOption);
            SetupEngine(_model, backend);
        }

        private void SetupEngine(Model model, BackendType backendType)
        {
            if (_worker != null) _worker.Dispose();

            _worker = new Worker(model, backendType);
        }

        //bounding box data
        public struct BoundingBox
        {
            public float CenterX;
            public float CenterY;
            public float Width;
            public float Height;
            public string Label;
        }
    }
}