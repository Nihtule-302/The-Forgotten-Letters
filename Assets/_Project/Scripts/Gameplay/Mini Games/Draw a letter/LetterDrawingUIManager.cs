using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TheForgottenLetters
{
    public class LetterDrawingUIManager : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject SelectLetterMenu;
        [SerializeField] private GameObject LetterDrawingScreen;
        [SerializeField] private FingerDrawing FingerDrawing;
        [SerializeField] private GameObject BackgroundScreen;
        [SerializeField] private GameObject RoundStarsScreen;


        [Header("Answer Feedback")]
        [SerializeField] private GameObject correctScreen;
        [SerializeField] private GameObject wrongScreen;
        [SerializeField] private float answerFeedbackScreenDelay = 0.2f;
        [SerializeField] private float answerFeedbackScreenDuration = 1f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            StartGame();
        }

        private void StartGame()
        {
            SelectLetterMenu.SetActive(true);
            LetterDrawingScreen.SetActive(false);
            BackgroundScreen.SetActive(false); 
            RoundStarsScreen.SetActive(false);
            FingerDrawing.enabled = false;

            correctScreen.SetActive(false);
            wrongScreen.SetActive(false);
        }


        public void StartLetterDrawing()
        {
            SelectLetterMenu.SetActive(false);
            LetterDrawingScreen.SetActive(true);
            BackgroundScreen.SetActive(true);
            RoundStarsScreen.SetActive(true);
            EnableFingerDrawingWithDelayAsync().Forget();
        }

        public void ActivateLetterSelectionScreen()
        {
            StartGame();
        }

        private async UniTaskVoid EnableFingerDrawingWithDelayAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(.5f));
            FingerDrawing.enabled = true;
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
