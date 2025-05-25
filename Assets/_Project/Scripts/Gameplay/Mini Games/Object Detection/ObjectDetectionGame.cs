using System;
using System.Collections.Generic;
using _Project.Scripts.Core.DataTypes;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Utilities.Scene_Management;
using Cysharp.Threading.Tasks;
using TheForgottenLetters;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Gameplay.Mini_Games.Object_Detection
{
    public class ObjectDetectionGame : MonoBehaviour
    {
        [Header("Dependencies")] [SerializeField]
        private SceneTransitioner sceneTransitioner;

        [SerializeField] private ObjectDetectionUIManager uiManager;
        [SerializeField] private List<LetterData> availableLetters;

        [Header("Debug")] [SerializeField] private LetterData targetLetterData;

        [SerializeField] private string targetLetter;
        [SerializeField] private List<WordData> validObjectWords = new();

        private ILetterSelectionStrategy _selectionStrategy;

        private void Awake()
        {
            RequestCameraPermissionAsync().Forget();
        }

        public void StartGame(LetterData customLetter = null)
        {
            uiManager.StartObjectDetection();
            InitializeGame(customLetter);
        }

        public void RestartGame()
        {
            StartGame();
        }

        private void InitializeGame(LetterData customLetter = null)
        {
            SetSelectionStrategy(customLetter != null
                ? new FixedLetterSelectionStrategy(customLetter)
                : new RandomLetterSelectionStrategy());

            SelectTargetLetter();
            LoadValidObjectWords();
        }

        private void SelectTargetLetter()
        {
            targetLetterData = _selectionStrategy?.SelectLetter(availableLetters) ?? GetRandomLetter();
            targetLetter = targetLetterData.letter;

            SetSelectionStrategy(new FixedLetterSelectionStrategy(targetLetterData));
        }

        private void LoadValidObjectWords()
        {
            validObjectWords.Clear();
            validObjectWords.AddRange(targetLetterData.objectDetectionWords);
        }

        private LetterData GetRandomLetter()
        {
            return availableLetters[Random.Range(0, availableLetters.Count)];
        }

        public void CheckAnswer(string detectedLabel)
        {
            if (IsCorrectAnswer(detectedLabel))
                HandleCorrectAnswerAsync().Forget();
            else
                HandleIncorrectAnswerAsync().Forget();
        }

        private bool IsCorrectAnswer(string label)
        {
            return validObjectWords.Exists(word =>
                word.englishName.Equals(label, StringComparison.OrdinalIgnoreCase));
        }

        private async UniTask HandleCorrectAnswerAsync()
        {
            var dataBuilder = PersistentSOManager.GetSO<ObjectDetectionData>().GetBuilder();
            dataBuilder
                .IncrementCorrectScore()
                .AddRound(targetLetter, true);

            PersistentSOManager.GetSO<ObjectDetectionData>().UpdateData(dataBuilder);
            await FirebaseManager.Instance.SaveObjectDetectionData(PersistentSOManager.GetSO<ObjectDetectionData>());

            var playerDataBuilder = PersistentSOManager.GetSO<PlayerAbilityStats>().GetBuilder();
            playerDataBuilder
                .IncrementEnergyPoints()
                .SetlastTimeEnergyIncreased();

            PersistentSOManager.GetSO<PlayerAbilityStats>().UpdateData(playerDataBuilder);


            Debug.Log("Correct answer");
            uiManager.CallCorrectScreen();
            NextRound();
        }

        private async UniTaskVoid HandleIncorrectAnswerAsync()
        {
            var dataBuilder = PersistentSOManager.GetSO<ObjectDetectionData>().GetBuilder();
            dataBuilder
                .IncrementIncorrectScore()
                .AddRound(targetLetter, false);

            PersistentSOManager.GetSO<ObjectDetectionData>().UpdateData(dataBuilder);
            await FirebaseManager.Instance.SaveObjectDetectionData(PersistentSOManager.GetSO<ObjectDetectionData>());

            Debug.Log("Wrong answer");
            uiManager.CallWrongScreen();
        }

        private void NextRound()
        {
            SelectTargetLetter();
            LoadValidObjectWords();
        }

        public void SetSelectionStrategy(ILetterSelectionStrategy strategy)
        {
            _selectionStrategy = strategy;
        }

        [ContextMenu("Restart Game")]
        private void DebugRestartGame()
        {
            RestartGame();
        }

        private async UniTaskVoid RequestCameraPermissionAsync()
        {
            if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                await Application.RequestUserAuthorization(UserAuthorization.WebCam);
                var granted = Application.HasUserAuthorization(UserAuthorization.WebCam);

                if (!granted)
                {
                    Debug.LogWarning("Camera permission denied.");
                    sceneTransitioner.TranstionToScene();
                }
            }

            await UniTask.Delay(0);
        }
    }
}