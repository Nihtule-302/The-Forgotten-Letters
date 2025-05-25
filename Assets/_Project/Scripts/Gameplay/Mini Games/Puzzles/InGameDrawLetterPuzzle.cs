using _Project.Scripts.Core.DataTypes;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Utilities;
using _Project.Scripts.Gameplay._2D.HiddenDoor;
using Cysharp.Threading.Tasks;
using TheForgottenLetters;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Mini_Games.Puzzles
{
    public class InGameDrawLetterPuzzle : MonoBehaviour
    {
        [Header("UI")] [SerializeField] private InGameDrawLetterUIManager uiManager;

        [SerializeField] private TextMeshProUGUI targetLetterGuideText;

        [Header("Letter Data")] [SerializeField]
        private LetterData currentTargetLetterData;

        [SerializeField] private string currentTargetLetter;

        [Header("Hidden Door")] [SerializeField]
        private HiddenDoor hiddenDoor;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            currentTargetLetter = currentTargetLetterData.letter;
            targetLetterGuideText.text = currentTargetLetter;
        }

        // ------------------ Answer Checking ------------------
        public void CheckAnswer(LetterPredictionData letterPredictionData)
        {
            if (IsAnswerCorrect(letterPredictionData))
                HandleCorrectChoiceAsync().Forget();
            else
                HandleIncorrectChoiceAsync().Forget();
        }

        private bool IsAnswerCorrect(LetterPredictionData letterPredictionData)
        {
            var correctLetter = ArabicNormalizer.NormalizeArabicLetter(currentTargetLetter) ==
                                letterPredictionData.letter.ToString();
            var probability = letterPredictionData.probability >= 75f;

            return correctLetter && probability;
        }

        private async UniTask HandleCorrectChoiceAsync()
        {
            Debug.Log("Correct answer");
            var dataBuilder = PersistentSOManager.GetSO<DrawLetterData>().GetBuilder();
            dataBuilder
                .IncrementCorrectScore()
                .AddRound(currentTargetLetter, true);

            PersistentSOManager.GetSO<DrawLetterData>().UpdateData(dataBuilder);
            await FirebaseManager.Instance.SaveDrawLetterData(PersistentSOManager.GetSO<DrawLetterData>());

            var playerDataBuilder = PersistentSOManager.GetSO<PlayerAbilityStats>().GetBuilder();
            playerDataBuilder
                .IncrementEnergyPoints()
                .SetlastTimeEnergyIncreased();

            PersistentSOManager.GetSO<PlayerAbilityStats>().UpdateData(playerDataBuilder);


            hiddenDoor.UnlockDoor();
        }

        private async UniTaskVoid HandleIncorrectChoiceAsync()
        {
            Debug.Log("Wrong answer");
            var dataBuilder = PersistentSOManager.GetSO<DrawLetterData>().GetBuilder();
            dataBuilder
                .IncrementIncorrectScore()
                .AddRound(currentTargetLetter, false);

            PersistentSOManager.GetSO<DrawLetterData>().UpdateData(dataBuilder);

            await FirebaseManager.Instance.SaveDrawLetterData(PersistentSOManager.GetSO<DrawLetterData>());
        }
    }
}