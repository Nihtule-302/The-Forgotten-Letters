using System;
using _Project.Scripts.Core.Utilities.Scene_Management;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Mini_Games.Object_Detection
{
    public class ObjectDetectionUIManager : MonoBehaviour
    {
        [FormerlySerializedAs("SelectLetterMenu")] [Header("UI Elements")] [SerializeField]
        private GameObject selectLetterMenu;

        [FormerlySerializedAs("GameScreen")] [SerializeField] private GameObject gameScreen;

        [FormerlySerializedAs("Yolo")] [SerializeField] private GameObject yolo;
        // [SerializeField] private GameObject ScoreScreen;


        [Header("Answer Feedback")] [SerializeField]
        private GameObject correctScreen;

        [SerializeField] private GameObject wrongScreen;
        [SerializeField] private float answerFeedbackScreenDelay = 0.2f;
        [SerializeField] private float answerFeedbackScreenDuration = 1f;

        [Header("Scene Transitioner")]
        [SerializeField] private SceneTransitioner sceneTransitioner;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            StartGame();
        }

        [ContextMenu("Start Game")]
        private void StartGame()
        {
            selectLetterMenu.SetActive(true);
            gameScreen.SetActive(false);
            // ScoreScreen.SetActive(false);
            yolo.SetActive(false);

            correctScreen.SetActive(false);
            wrongScreen.SetActive(false);
        }

        [ContextMenu("Start Object Detection")]
        public void StartObjectDetection()
        {
            RequestCameraPermissionAsync().Forget();
        }

        public async UniTask RequestCameraPermissionAsync()
        {
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                await Application.RequestUserAuthorization(UserAuthorization.WebCam);
                var granted = Application.HasUserAuthorization(UserAuthorization.WebCam);

                if (!granted)
                {
                    Debug.LogWarning("Camera permission denied.");
                    sceneTransitioner.TranstionToSceneAsync().Forget();
                    return;
                }
            }

            await UniTask.Delay(0);

            ProceedWithObjectDetection();
        }

        private void ProceedWithObjectDetection()
        {
            selectLetterMenu.SetActive(false);
            gameScreen.SetActive(true);
            // ScoreScreen.SetActive(true);
            EnableYoloWithDelayAsync().Forget();
        }

        [ContextMenu("Activate Letter Selection Screen")]
        public void ActivateLetterSelectionScreen()
        {
            StartGame();
        }

        private async UniTaskVoid EnableYoloWithDelayAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(.5f));
            yolo.SetActive(true);
        }

        [ContextMenu("Call Correct Screen")]
        public void CallCorrectScreen()
        {
            ShowWrongScreenWithDelayAsync().Forget();
        }

        [ContextMenu("Call Wrong Screen")]
        public void CallWrongScreen()
        {
            ShowCorrectScreenWithDelayAsync().Forget();
        }


        private async UniTaskVoid ShowWrongScreenWithDelayAsync()
        {
            wrongScreen.SetActive(false);
            correctScreen.SetActive(false);

            await UniTask.Delay(TimeSpan.FromSeconds(answerFeedbackScreenDelay));
            wrongScreen.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(answerFeedbackScreenDuration));
            wrongScreen.SetActive(false);
        }

        private async UniTaskVoid ShowCorrectScreenWithDelayAsync()
        {
            wrongScreen.SetActive(false);
            correctScreen.SetActive(false);

            await UniTask.Delay(TimeSpan.FromSeconds(answerFeedbackScreenDelay));
            correctScreen.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(answerFeedbackScreenDuration));
            correctScreen.SetActive(false);
        }
    }
}