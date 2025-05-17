using _Project.Scripts.Core.DataTypes;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Utilities;
using Cysharp.Threading.Tasks;
using TheForgottenLetters;
using TMPro;
using UnityEngine;

public class InGameDrawLetterPuzzle : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private InGameDrawLetterUIManager uiManager;
    [SerializeField] private TextMeshProUGUI targetLetterGuideText;

    [Header("Letter Data")]
    [SerializeField] private LetterData currentTargetLetterData;
    [SerializeField] private string currentTargetLetter;

    [Header("Hidden Door")]
    [SerializeField] private HiddenDoor hiddenDoor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTargetLetter = currentTargetLetterData.letter;
        targetLetterGuideText.text = currentTargetLetter;
    }

    // ------------------ Answer Checking ------------------
    public void CheckAnswer(LetterPredictionData letterPredictionData)
    {
        if (IsAnswerCorrect(letterPredictionData))
        {
            HandleCorrectChoiceAsync().Forget();
        }
        else
        {
            HandleIncorrectChoiceAsync().Forget();
        }
    }

    private bool IsAnswerCorrect(LetterPredictionData letterPredictionData)
    {
        bool correctLetter = ArabicNormalizer.NormalizeArabicLetter(currentTargetLetter) == letterPredictionData.letter.ToString();
        bool probability = letterPredictionData.probability >= 75f;

        return correctLetter && probability;
    }

    private async UniTask HandleCorrectChoiceAsync()
    {
        Debug.Log("Correct answer");
        var dataBuilder = PersistentSOManager.GetSO<DrawLetterData>().GetBuilder();
        dataBuilder
            .IncrementCorrectScore()
            .AddRound(currentTargetLetter, isCorrect: true);

        PersistentSOManager.GetSO<DrawLetterData>().UpdateData(dataBuilder);
        await FirebaseManager.Instance.SaveDrawLetterData(PersistentSOManager.GetSO<DrawLetterData>());

        PersistentSOManager.GetSO<PlayerAbilityStats>().AddEnergyPoint();
            
        hiddenDoor.UnlockDoor(); 
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
}
