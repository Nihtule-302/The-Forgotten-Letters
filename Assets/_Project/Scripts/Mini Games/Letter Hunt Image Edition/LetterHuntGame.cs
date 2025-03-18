using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Flexalon;
using Cysharp.Threading.Tasks;

namespace _Project.Scripts.Mini_Games.Letter_Hunt_Image_Edition
{
    public class LetterHuntGame : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI targetLetterText;
        [SerializeField] private FlexalonGridLayout gridLayout;
        [SerializeField] private List<GameObject> buttonVariants = new();
        [SerializeField] private GetValueFromDropdown buttonVariantDropdown;

        [Header("Dissolve Settings")]
        [SerializeField] private float dissolveStart = 1f;
        [SerializeField] private float dissolveEnd = 0f;
        [SerializeField] private float dissolveDuration = 1.5f;
        [SerializeField] private float edgeWidth ;
        [SerializeField] private Color dissolveBaseColor = Color.black;
        [SerializeField] [ColorUsage(true, true)] private Color dissolveEdgeColor = Color.white;

        [Header("Game Settings")]
        [SerializeField] private int totalChoices = 6;
        [SerializeField, Range(0f, 1f)] private float correctChoiceRatio = 0.5f;
        [SerializeField] private float correctChoiceDelay = 0.5f;
        [SerializeField] private float holdDuration = 1f;

        [Header("Letter & Word Data")]
        [SerializeField] private List<LetterData> availableLetters;

        private LetterData targetLetterData;
        private string targetLetter;
        private List<LetterData> distractorLetters = new();
        private List<WordData> wordChoices = new();

        private List<GameObject> choiceButtons = new();
        private GameObject selectedButtonVariant;

        private void Start() => StartGame();

        public void StartGame()
        {
            InitializeGame();
            GenerateWordChoices();
            DisplayChoices();
        }

        private void InitializeGame()
        {
            SelectButtonVariant();
            SelectTargetLetter();
            ConfigureGridLayout();
        }

        public void StartGame(LetterData letterData)
        {
            InitializeGame(letterData);
            GenerateWordChoices();
            DisplayChoices();
        }

        private void InitializeGame(LetterData letterData)
        {
            SelectButtonVariant();
            SelectTargetLetter(letterData);
            ConfigureGridLayout();
        }

        private void SelectTargetLetter(LetterData letterData = null)
        {
            if (letterData == null)
            {
                targetLetterData = availableLetters[Random.Range(0, availableLetters.Count)];
                targetLetter = targetLetterData.letter;
            }
            else
            {
                targetLetter = letterData.letter;
            }
            
            targetLetterText.GetComponent<ArabicFixerTMPRO>().fixedText = targetLetter;
            distractorLetters = availableLetters.Where(letter => letter != targetLetterData).OrderBy(_ => Random.value).Take(3).ToList();
        }

        

        private void ConfigureGridLayout()
        {
            gridLayout.Columns = (uint)Mathf.CeilToInt(Mathf.Sqrt(totalChoices));
            gridLayout.Rows = (uint)Mathf.CeilToInt((float)totalChoices / gridLayout.Columns);
        }

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
        {
            return targetLetterData.words.OrderBy(_ => Random.value).Take(count).ToList();
        }

        private HashSet<WordData> GetIncorrectWords(int count)
        {
            HashSet<WordData> incorrectWords = new();
            
            foreach (var letter in distractorLetters)
            {
                if (letter.words.Count > 0 && incorrectWords.Count < count)
                {
                    var word = letter.words[Random.Range(0, letter.words.Count)];
                    if (IsValidIncorrectWord(word))
                        incorrectWords.Add(word);
                }
            }
            
            while (incorrectWords.Count < count)
            {
                var randomDistractor = distractorLetters[Random.Range(0, distractorLetters.Count)];
                if (randomDistractor.words.Count > 0)
                {
                    var word = randomDistractor.words[Random.Range(0, randomDistractor.words.Count)];
                    if (IsValidIncorrectWord(word))
                        incorrectWords.Add(word);
                }
            }
            
            return incorrectWords;
        }

        private bool IsValidIncorrectWord(WordData word)
        {
            return !ArabicNormalizer.DoesWordContainsTargetLetter(word.arabicWord, targetLetter);
        }

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
            var dissolveData = new DissolveData();

            dissolveData.dissolveStart = dissolveStart;
            dissolveData.dissolveEnd = dissolveEnd;
            dissolveData.dissolveDuration = dissolveDuration;
            dissolveData.baseDissolveColor = dissolveBaseColor;
            dissolveData.edgeColor = dissolveEdgeColor;
            dissolveData.edgeWidth = edgeWidth;

            SetupChoiceButton(button, word, dissolveData);
            choiceButtons.Add(button);
        }

        private void SetupChoiceButton(GameObject buttonObject, WordData word, DissolveData dissolveData)
        {
            var buttonControler = buttonObject.GetComponent<ChoiceButtonController>();

            buttonControler.SetUpChoiceButton(word,holdDuration, dissolveData);
            
            buttonControler.onHoldActionComplete = () => CheckAnswer(word); // Check answer on hold
        }

        public void CheckAnswer(WordData word)
        {
            if (targetLetterData.words.Contains(word))
            {
                HandleCorrectChoiceAsync(word).Forget();
            }
            else
            {
                HandleIncorrectChoiceAsync(word).Forget();
            }
        }

        private async UniTaskVoid HandleCorrectChoiceAsync(WordData word)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(correctChoiceDelay));
            RestartGame();


            var letterHuntDataBuilder = PersistentSOManager.GetSO<LetterHuntData>().GetBuilder();

            letterHuntDataBuilder
            .IncrementCorrectScore()
            .AddRound(targetLetter, word.arabicWord, isCorrect: true);

            PersistentSOManager.GetSO<LetterHuntData>().UpdateData(letterHuntDataBuilder);

            Debug.Log("✅ " + ArabicSupport.Fix("صحيح!", true, true));
            
        }

        private async UniTaskVoid HandleIncorrectChoiceAsync(WordData word)
        {
            var letterHuntDataBuilder = PersistentSOManager.GetSO<LetterHuntData>().GetBuilder();

            letterHuntDataBuilder
            .IncrementIncorrectScore()
            .AddRound(targetLetter, word.arabicWord, isCorrect: false);

            PersistentSOManager.GetSO<LetterHuntData>().UpdateData(letterHuntDataBuilder);

            Debug.Log("❌ " + ArabicSupport.Fix("خطأ! حاول مرة أخرى.", true, true));
            await UniTask.Delay(0);
        }

        public void UpdateButtonVariant()
        {
            SelectButtonVariant();
            RedisplayChoices();
        }

        private void SelectButtonVariant()
        {
            var selectedIndex = buttonVariantDropdown.GetDropdownValue();
            selectedButtonVariant = buttonVariants[selectedIndex];
        }

        [ContextMenu("Restart Game")]
        public void RestartGame() => StartGame();

        [ContextMenu("Redisplay Game")]
        public void RedisplayChoices() => DisplayChoices();
    }
}
