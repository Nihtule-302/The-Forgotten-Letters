using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Project.Scripts.Gameplay.Mini_Games.Draw_a_letter
{
    public class LetterDrawingUIManager : MonoBehaviour
    {
        [FormerlySerializedAs("SelectLetterMenu")] [Header("UI Elements")] [SerializeField]
        private GameObject selectLetterMenu;

        [FormerlySerializedAs("LetterDrawingScreen")] [SerializeField] private GameObject letterDrawingScreen;
        [FormerlySerializedAs("FingerDrawing")] [SerializeField] private FingerDrawing fingerDrawing;
        [FormerlySerializedAs("BackgroundScreen")] [SerializeField] private GameObject backgroundScreen;
        [FormerlySerializedAs("RoundStarsScreen")] [SerializeField] private GameObject roundStarsScreen;


        [Header("Answer Feedback")] [SerializeField]
        private GameObject correctScreen;

        [SerializeField] private GameObject wrongScreen;
        [SerializeField] private float answerFeedbackScreenDelay = 0.2f;
        [SerializeField] private float answerFeedbackScreenDuration = 1f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            StartGame();
        }

        private void StartGame()
        {
            selectLetterMenu.SetActive(true);
            letterDrawingScreen.SetActive(false);
            backgroundScreen.SetActive(false);
            roundStarsScreen.SetActive(false);
            fingerDrawing.enabled = false;

            correctScreen.SetActive(false);
            wrongScreen.SetActive(false);
        }


        public void StartLetterDrawing()
        {
            selectLetterMenu.SetActive(false);
            letterDrawingScreen.SetActive(true);
            backgroundScreen.SetActive(true);
            roundStarsScreen.SetActive(true);
            EnableFingerDrawingWithDelayAsync().Forget();
        }

        public void ActivateLetterSelectionScreen()
        {
            StartGame();
        }

        private async UniTaskVoid EnableFingerDrawingWithDelayAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(.5f));
            fingerDrawing.enabled = true;
        }

        public void CallCorrectScreen()
        {
            ShowWrongScreenWithDelayAsync().Forget();
        }

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