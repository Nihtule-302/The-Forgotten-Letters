using System.Collections.Generic;
using _Project.Scripts.Core.DataTypes;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Utilities;
using Cysharp.Threading.Tasks;
using TheForgottenLetters;
using TMPro;
using UnityEngine;

/// <summary>
/// 🕹️ Gameplay Flow:
/// 1) Player selects a target letter.
/// 2) Game runs progressive drawing rounds with different fonts.
///    - Rounds 1–3: Show a guide (target letter in a font).
///    - Round 4: Guide is hidden (memory challenge).
///    - return to select letter menu.
/// ✅ Scoring:
///    - +1 Correct if match
///    - +1 Incorrect if mismatch
/// </summary>
public class DrawLetterGame : MonoBehaviour
{
    [SerializeField] private LetterDrawingUIManager uiManager;
    // ------------------ UI References ------------------
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI targetLetterGuideText;

    // ------------------ Font Management ------------------
    [Header("Fonts")]
    [SerializeField] private List<TMP_FontAsset> fontVariants = new();
    [SerializeField] private List<TMP_FontAsset> shuffledFontVariants = new();

    // ------------------ Game Settings ------------------
    [Header("Game Settings")]
    [SerializeField] private int guideRoundsCount = 3;

    // ------------------ Letter Data ------------------
    [Header("Letter & Word Data")]
    [SerializeField] private List<LetterData> availableLetters;
    private LetterData currentTargetLetterData;
    [SerializeField] private string currentTargetLetter;

    // ------------------ Game State ------------------
    [SerializeField] private int currentRound = 0;
    private ILetterSelectionStrategy letterSelectionStrategy;

    // ------------------ Unity Lifecycle ------------------
    private void Start() => StartGame();

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
        currentTargetLetterData = letterSelectionStrategy?.SelectLetter(availableLetters)
                                    ?? GetRandomLetter();

        currentTargetLetter = currentTargetLetterData.letter;
        UpdateGuideVisibility();

        // Fix target letter so it doesn't change each round
        SetLetterSelectionStrategy(new FixedLetterSelectionStrategy(currentTargetLetterData));
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
        bool showGuide = ShouldShowGuide();
        targetLetterGuideText.gameObject.SetActive(showGuide);

        if (showGuide)
        {
            UpdateGuideText();
        }
    }

    private bool ShouldShowGuide()
    {
        return currentRound < guideRoundsCount - 1;
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
        {
            HandleCorrectChoiceAsync().Forget();
        }
        else
        {
            HandleIncorrectChoiceAsync().Forget();
        }
    }

    private bool IsAnswerCorrect(string drawnLetter)
    {
        return ArabicNormalizer.NormalizeArabicLetter(currentTargetLetter) == drawnLetter;
    }

    private async UniTask HandleCorrectChoiceAsync()
    {
        Debug.Log("Correct answer");

        if (IsLastRound())
        {
            EndGame();
        }
        else
        {
            var dataBuilder = PersistentSOManager.GetSO<DrawLetterData>().GetBuilder();
            dataBuilder
                .IncrementCorrectScore()
                .AddRound(currentTargetLetter, isCorrect: true);

            PersistentSOManager.GetSO<DrawLetterData>().UpdateData(dataBuilder);
            await FirebaseManager.Instance.SaveDrawLetterData(PersistentSOManager.GetSO<DrawLetterData>());

            PersistentSOManager.GetSO<PlayerAbilityStats>().AddEnergyPoint();
            
            ProceedToNextRound();
        }
    }

    private async UniTaskVoid HandleIncorrectChoiceAsync()
    {
        Debug.Log("Wrong answer");
        var dataBuilder = PersistentSOManager.GetSO<DrawLetterData>().GetBuilder();
        dataBuilder
            .IncrementIncorrectScore()
            .AddRound(currentTargetLetter, isCorrect: false);

        PersistentSOManager.GetSO<DrawLetterData>().UpdateData(dataBuilder);
        
        await FirebaseManager.Instance.SaveDrawLetterData(PersistentSOManager.GetSO<DrawLetterData>());
    }

    private bool IsLastRound()
    {
        return currentRound >= guideRoundsCount - 1;
    }

    private void ProceedToNextRound()
    {
        currentRound++;
        UpdateGuideVisibility();
    }

    private void EndGame()
    {
        Debug.Log("Game Over");
        uiManager.ActivateLetterSelectionScreen();
    }

    // ------------------ Strategy Pattern ------------------
    public void SetLetterSelectionStrategy(ILetterSelectionStrategy strategy)
    {
        letterSelectionStrategy = strategy;
    }

    // ------------------ Debugging ------------------
    [ContextMenu("Restart Game")]
    private void DebugRestartGame()
    {
        RestartGame();
    }
}
