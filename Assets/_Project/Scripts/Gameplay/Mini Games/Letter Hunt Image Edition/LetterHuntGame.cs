using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using _Project.Scripts.Core.DataTypes;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.SaveSystem;
using _Project.Scripts.Core.Utilities;
using _Project.Scripts.Core.Utilities.UI;
using _Project.Scripts.UI;
using _Project.Scripts.UI.Buttons.ChoiceButton;
using Flexalon;

namespace _Project.Scripts.Mini_Games.Letter_Hunt_Image_Edition
{
    public class LetterHuntGame : MonoBehaviour
    {
        #region UI
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI targetLetterText;
        [SerializeField] private FlexalonGridLayout gridLayout;
        [SerializeField] private List<GameObject> buttonVariants = new();
        [SerializeField] private GetValueFromDropdown buttonVariantDropdown;
        #endregion

        #region Dissolve Settings
        [Header("Dissolve Settings")]
        [SerializeField] private float dissolveStart = 1f;
        [SerializeField] private float dissolveEnd = 0f;
        [SerializeField] private float dissolveDuration = 1.5f;
        [SerializeField] private float edgeWidth;
        [SerializeField] private Color dissolveBaseColor = Color.black;
        [SerializeField, ColorUsage(true, true)] private Color dissolveEdgeColor = Color.white;
        #endregion

        #region Game Settings
        [Header("Game Settings")]
        [SerializeField] private int totalChoices = 6;
        [SerializeField, Range(0f, 1f)] private float correctChoiceRatio = 0.5f;
        [SerializeField] private float correctChoiceDelay = 0.5f;
        [SerializeField] private float holdDuration = 1f;
        #endregion

        #region Data
        [Header("Letter & Word Data")]
        [SerializeField] private List<LetterData> availableLetters;

        private LetterData targetLetterData;
        private string targetLetter;
        private List<LetterData> distractorLetters = new();
        private List<WordData> wordChoices = new();
        private List<GameObject> choiceButtons = new();
        private GameObject selectedButtonVariant;

        private ILetterSelectionStrategy letterSelectionStrategy;
        #endregion

        #region Game Entry
        private void Start() => StartGame();

        public void SetLetterSelectionStrategy(ILetterSelectionStrategy strategy)
            => letterSelectionStrategy = strategy;

        [ContextMenu("Restart Game")]
        public void RestartGame() => StartGame();

        [ContextMenu("Redisplay Game")]
        public void RedisplayChoices() => DisplayChoices();
        #endregion

        #region Game Initialization
        public void StartGame()
        {
            InitializeGame();
            GenerateWordChoices();
            DisplayChoices();
        }

        public void StartGame(LetterData letterData)
        {
            InitializeGame(letterData);
            GenerateWordChoices();
            DisplayChoices();
        }

        private void InitializeGame()
        {
            SelectButtonVariant();
            SetLetterSelectionStrategy(new RandomLetterSelectionStrategy());
            SelectTargetLetter();
            ConfigureGridLayout();
        }

        private void InitializeGame(LetterData letterData)
        {
            SelectButtonVariant();
            SetLetterSelectionStrategy(new FixedLetterSelectionStrategy(letterData));
            SelectTargetLetter();
            ConfigureGridLayout();
        }

        private void SelectButtonVariant()
        {
            int selectedIndex = buttonVariantDropdown.GetDropdownValue();
            selectedButtonVariant = buttonVariants[selectedIndex];
        }

        private void SelectTargetLetter()
        {
            targetLetterData = letterSelectionStrategy?.SelectLetter(availableLetters) 
                            ?? availableLetters[Random.Range(0, availableLetters.Count)];
            targetLetter = targetLetterData.letter;

            targetLetterText.GetComponent<ArabicFixerTMPRO>().fixedText = targetLetter;
            distractorLetters = availableLetters.Where(letter => letter != targetLetterData).OrderBy(_ => Random.value).Take(3).ToList();
        }

        private void ConfigureGridLayout()
        {
            gridLayout.Columns = (uint)Mathf.CeilToInt(Mathf.Sqrt(totalChoices));
            gridLayout.Rows = (uint)Mathf.CeilToInt((float)totalChoices / gridLayout.Columns);
        }
        #endregion

        [ContextMenu("Next Level")]
        public void NextLevel()
        {
            SelectTargetLetter();
            GenerateWordChoices();
            DisplayChoices();
        }


        #region Word Generation
        private void GenerateWordChoices()
        {
            int correctCount = Mathf.CeilToInt(totalChoices * correctChoiceRatio);
            int incorrectCount = totalChoices - correctCount;

            wordChoices.Clear();
            AddWordsToChoices(GetCorrectWords(correctCount));
            AddWordsToChoices(GetIncorrectWords(incorrectCount));
            wordChoices.Shuffle();
        }

        private void AddWordsToChoices(IEnumerable<WordData> words) => wordChoices.AddRange(words);

        private List<WordData> GetCorrectWords(int count)
            => targetLetterData.words.OrderBy(_ => Random.value).Take(count).ToList();

        private HashSet<WordData> GetIncorrectWords(int count)
        {
            HashSet<WordData> incorrectWords = new();

            foreach (var letter in distractorLetters)
            {
                if (letter.words.Count > 0 && incorrectWords.Count < count)
                {
                    var word = letter.words[Random.Range(0, letter.words.Count)];
                    if (IsValidIncorrectWord(word)) incorrectWords.Add(word);
                }
            }

            while (incorrectWords.Count < count)
            {
                var randomDistractor = distractorLetters[Random.Range(0, distractorLetters.Count)];
                if (randomDistractor.words.Count == 0) continue;

                var word = randomDistractor.words[Random.Range(0, randomDistractor.words.Count)];
                if (IsValidIncorrectWord(word)) incorrectWords.Add(word);
            }

            return incorrectWords;
        }

        private bool IsValidIncorrectWord(WordData word)
            => !ArabicNormalizer.DoesWordContainsTargetLetter(word.arabicWord, targetLetter);
        #endregion

        #region UI Display
        private void DisplayChoices()
        {
            ClearExistingButtons();
            wordChoices.ForEach(CreateChoiceButton);
        }

        private void ClearExistingButtons()
        {
            choiceButtons.ForEach(Destroy);
            choiceButtons.Clear();
        }

        private void CreateChoiceButton(WordData word)
        {
            var button = Instantiate(selectedButtonVariant, gridLayout.transform);
            var dissolveData = new DissolveData
            {
                dissolveStart = dissolveStart,
                dissolveEnd = dissolveEnd,
                dissolveDuration = dissolveDuration,
                baseDissolveColor = dissolveBaseColor,
                edgeColor = dissolveEdgeColor,
                edgeWidth = edgeWidth
            };

            SetupChoiceButton(button, word, dissolveData);
            choiceButtons.Add(button);
        }

        private void SetupChoiceButton(GameObject buttonObject, WordData word, DissolveData dissolveData)
        {
            var controller = buttonObject.GetComponent<ChoiceButtonController>();

            controller.SetUpChoiceButton(word, holdDuration, dissolveData);
            controller.onHoldActionComplete = () => CheckAnswer(word, controller);
        }
        #endregion

        #region Answer Checking
        public void CheckAnswer(WordData word, ChoiceButtonController button)
        {
            if (targetLetterData.words.Contains(word))
            {
                button.updateDissolveColors(Color.green, Color.green);
                HandleCorrectChoiceAsync(word).Forget();
            }
            else
            {
                button.updateDissolveColors(Color.red, Color.red);
                HandleIncorrectChoiceAsync(word).Forget();
            }
        }

        private async UniTask HandleCorrectChoiceAsync(WordData word)
        {
            var dataBuilder = PersistentSOManager.GetSO<LetterHuntData>().GetBuilder();

            dataBuilder
                .IncrementCorrectScore()
                .AddRound(targetLetter, word.arabicWord, isCorrect: true);

            PersistentSOManager.GetSO<LetterHuntData>().UpdateData(dataBuilder);
            await FirebaseManager.Instance.SaveLetterHuntData(PersistentSOManager.GetSO<LetterHuntData>());

            Debug.Log("✅ " + ArabicSupport.Fix("صحيح!", true, true));
            await UniTask.Delay(System.TimeSpan.FromSeconds(correctChoiceDelay));
            NextLevel();
        }

        private async UniTaskVoid HandleIncorrectChoiceAsync(WordData word)
        {
            var dataBuilder = PersistentSOManager.GetSO<LetterHuntData>().GetBuilder();

            dataBuilder
                .IncrementIncorrectScore()
                .AddRound(targetLetter, word.arabicWord, isCorrect: false);

            PersistentSOManager.GetSO<LetterHuntData>().UpdateData(dataBuilder);
            await FirebaseManager.Instance.SaveLetterHuntData(PersistentSOManager.GetSO<LetterHuntData>());

            Debug.Log("❌ " + ArabicSupport.Fix("خطأ! حاول مرة أخرى.", true, true));
            await UniTask.Delay(0);
        }
        #endregion

        #region Button Variant
        public void UpdateButtonVariant()
        {
            SelectButtonVariant();
            RedisplayChoices();
        }
        #endregion
    }
}
