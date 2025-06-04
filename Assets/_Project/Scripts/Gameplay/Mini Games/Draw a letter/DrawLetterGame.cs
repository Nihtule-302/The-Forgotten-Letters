using System.Collections.Generic;
using _Project.Scripts.Core.DataTypes;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Utilities;
using Cysharp.Threading.Tasks;
using TheForgottenLetters;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Mini_Games.Draw_a_letter
{
    /// <summary>
    ///     🕹️ Gameplay Flow:
    ///     1) Player selects a target letter.
    ///     2) Game runs progressive drawing rounds with different fonts.
    ///     - Rounds 1–3: Show a guide (target letter in a font).
    ///     - Round 4: Guide is hidden (memory challenge).
    ///     - return to select letter menu.
    ///     ✅ Scoring:
    ///     - +1 Correct if match
    ///     - +1 Incorrect if mismatch
    /// </summary>
    public class DrawLetterGame : MonoBehaviour
    {
        [SerializeField] private LetterDrawingUIManager uiManager;

        // ------------------ UI References ------------------
        [Header("UI Components")] [SerializeField]
        private TextMeshProUGUI targetLetterGuideText;

        [SerializeField] private DrawLetterRoundUI drawLetterRoundUI;


        // ------------------ Font Management ------------------
        [Header("Fonts")] [SerializeField] private List<TMP_FontAsset> fontVariants = new();

        [SerializeField] private List<TMP_FontAsset> shuffledFontVariants = new();

        // ------------------ Game Settings ------------------
        [Header("Game Settings")] [SerializeField]
        private int guideRoundsCount = 3;

        // ------------------ Letter Data ------------------
        [Header("Letter & Word Data")] [SerializeField]
        private List<LetterData> availableLetters;

        [SerializeField] private string currentTargetLetter;

        // ------------------ Game State ------------------
        [SerializeField] private int currentRound;
        private LetterData _currentTargetLetterData;
        private ILetterSelectionStrategy _letterSelectionStrategy;
        public int MaxRounds => guideRoundsCount + 1;
        public int CurrentRound => currentRound;

        // ------------------ Unity Lifecycle ------------------
        private void Start()
        {
            StartGame();
        }

        // ------------------ Game Flow ------------------
        public void StartGame(LetterData letterData = null)
        {
            InitializeGame(letterData);
        }

        public void RestartGame()
        {
            StartGame();
        }

        private void InitializeGame(LetterData letterData = null)
        {
            SetLetterSelectionStrategy(letterData != null
                ? new FixedLetterSelectionStrategy(letterData)
                : new RandomLetterSelectionStrategy());

            ShuffleFontVariants();
            ResetGameState();
            SelectTargetLetter();
        }

        private void ResetGameState()
        {
            currentRound = 0;
        }

        private void SelectTargetLetter()
        {
            _currentTargetLetterData = _letterSelectionStrategy?.SelectLetter(availableLetters)
                                      ?? GetRandomLetter();

            currentTargetLetter = _currentTargetLetterData.letter;
            UpdateGuideVisibility();

            // Fix target letter so it doesn't change each round
            SetLetterSelectionStrategy(new FixedLetterSelectionStrategy(_currentTargetLetterData));
        }

        private LetterData GetRandomLetter()
        {
            return availableLetters[Random.Range(0, availableLetters.Count)];
        }

        // ------------------ Font Handling ------------------
        private void ShuffleFontVariants()
        {
            shuffledFontVariants.Clear();
            shuffledFontVariants.AddRange(fontVariants);
            shuffledFontVariants.Shuffle();
        }

        // ------------------ Guide Display ------------------
        private void UpdateGuideVisibility()
        {
            var showGuide = ShouldShowGuide();
            targetLetterGuideText.gameObject.SetActive(showGuide);

            if (showGuide) UpdateGuideText();
        }

        private bool ShouldShowGuide()
        {
            return currentRound < guideRoundsCount;
        }

        private void UpdateGuideText()
        {
            targetLetterGuideText.GetComponent<ArabicFixerTMPRO>().fixedText = currentTargetLetter;
            targetLetterGuideText.font = shuffledFontVariants[currentRound];
        }

        // ------------------ Answer Checking ------------------
        public void CheckAnswer(string drawnLetter)
        {
            if (IsAnswerCorrect(drawnLetter))
                HandleCorrectChoiceAsync().Forget();
            else
                HandleIncorrectChoiceAsync().Forget();
        }

        private bool IsAnswerCorrect(string drawnLetter)
        {
            return ArabicNormalizer.NormalizeArabicLetter(currentTargetLetter) == drawnLetter;
        }

        private async UniTask HandleCorrectChoiceAsync()
        {
            Debug.Log("Correct answer");
            
            var drawLetterDataBuilder = PersistentSOManager.GetSO<DrawLetterData>().GetBuilder();
            await drawLetterDataBuilder
                    .IncrementCorrectScore()
                    .AddRound(currentTargetLetter, true)
                    .UpdateLocalData()
                    .SaveDataToFirebaseAsync();
            

            var playerDataBuilder = PersistentSOManager.GetSO<PlayerAbilityStats>().GetBuilder();
            await playerDataBuilder
                .IncrementEnergyPoints()
                .SetlastTimeEnergyIncreased()
                .UpdateLocalData()
                .SaveDataToFirebaseAsync();

            if (IsLastRound())
            {
                EndGame();
            }
            else
            {
                ProceedToNextRound();
            }
        }

        private async UniTaskVoid HandleIncorrectChoiceAsync()
        {
            Debug.Log("Wrong answer");
            var dataBuilder = PersistentSOManager.GetSO<DrawLetterData>().GetBuilder();
            await dataBuilder
                    .IncrementIncorrectScore()
                    .AddRound(currentTargetLetter, false)
                    .UpdateLocalData()
                    .SaveDataToFirebaseAsync();
        }

        private bool IsLastRound()
        {
            return currentRound >= MaxRounds - 1;
        }

        private void ProceedToNextRound()
        {
            currentRound++;
            drawLetterRoundUI.UpdateStars();
            UpdateGuideVisibility();
        }

        private void EndGame()
        {
            Debug.Log("Game Over");
            ResetGameState();
            drawLetterRoundUI.UpdateStars();
            uiManager.ActivateLetterSelectionScreen();
        }

        // ------------------ Strategy Pattern ------------------
        public void SetLetterSelectionStrategy(ILetterSelectionStrategy strategy)
        {
            _letterSelectionStrategy = strategy;
        }

        // ------------------ Debugging ------------------
        [ContextMenu("Restart Game")]
        private void DebugRestartGame()
        {
            RestartGame();
        }
    }
}