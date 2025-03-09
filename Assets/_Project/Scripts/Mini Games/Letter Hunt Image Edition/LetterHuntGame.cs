using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Flexalon;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.Events;

namespace _Project.Scripts.Mini_Games.Letter_Hunt_Image_Edition
{
    public class LetterHuntGame : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private TextMeshProUGUI letterText;
        [SerializeField] private FlexalonGridLayout grid;
        [SerializeField] private GameObject choiceButtonPrefab;

        [Header("Game Settings")]
        [SerializeField] private List<LetterData> allLetters;
        [SerializeField] private int totalButtons = 6;
        [SerializeField] private float correctRatio = 0.5f;
        [SerializeField] private float correctAnswerDelaySec = 0.5f;
        [SerializeField] private float buttonHoldTime = 1f;

        [Header("Debugging")]
        [SerializeField] private LetterData targetLetterSO;
        [SerializeField] private string targetLetter;
        [SerializeField] private List<LetterData> distractorLetters;
        [SerializeField] private List<WordData> allChoices = new();
        [SerializeField] private List<GameObject> choiceButtons = new();


        private void Start() => StartGame();

        public void StartGame()
        {
            InitializeGame();
            SelectWords();
            DisplayWords();
        }

        private void InitializeGame()
        {
            SelectTargetLetter();
            SetupGrid();
        }

        private void SelectTargetLetter()
        {
            targetLetterSO = allLetters[Random.Range(0, allLetters.Count)];
            targetLetter = targetLetterSO.letter;
            letterText.GetComponent<ArabicFixerTMPRO>().fixedText = targetLetter;
            distractorLetters = allLetters.Where(letter => letter != targetLetterSO).OrderBy(_ => Random.value).Take(3).ToList();
        }

        private void SetupGrid()
        {
            grid.Columns = (uint)Mathf.CeilToInt(Mathf.Sqrt(totalButtons));
            grid.Rows = (uint)Mathf.CeilToInt((float)totalButtons / grid.Columns);
        }


        private void SelectWords()
        {
            int numCorrect = Mathf.CeilToInt(totalButtons * correctRatio);
            int numIncorrect = totalButtons - numCorrect;
            
            allChoices.Clear();
            AddWordsToChoices(GetCorrectWords(numCorrect));
            AddWordsToChoices(GetIncorrectWords(numIncorrect));
            allChoices.Shuffle();
        }

        private void AddWordsToChoices(IEnumerable<WordData> words)
        {
            allChoices.AddRange(words);
        }


        private List<WordData> GetCorrectWords(int count)
        {
            return targetLetterSO.words.OrderBy(_ => Random.value).Take(count).ToList();
        }

        private HashSet<WordData> GetIncorrectWords(int requiredCount)
        {
            HashSet<WordData> incorrectWords = new();
            
            foreach (var letter in distractorLetters)
            {
                if (letter.words.Count > 0 && incorrectWords.Count < requiredCount)
                {
                    var word = letter.words[Random.Range(0, letter.words.Count)];
                    if (IsValidIncorrectWord(word))
                        incorrectWords.Add(word);
                }
            }
            
            while (incorrectWords.Count < requiredCount)
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


        private void DisplayWords()
        {
            ClearExistingButtons();
            allChoices.ForEach(CreateChoiceButton);
        }

        private void ClearExistingButtons()
        {
            choiceButtons.ForEach(Destroy);
            choiceButtons.Clear();
        }

        private void CreateChoiceButton(WordData word)
        {
            var newButton = Instantiate(choiceButtonPrefab, grid.transform);
            SetupChoiceButton(newButton, word);
            choiceButtons.Add(newButton);
        }

        private void SetupChoiceButton(GameObject buttonObject, WordData word)
        {
            var buttonInfo = buttonObject.GetComponent<ButtonInfoAccess>();

            buttonInfo.Text.text = word.arabicWord;
            buttonInfo.Text.GetComponent<ArabicFixerTMPRO>().fixedText = word.arabicWord;

            if (word.wordImage != null && word.wordImage.Count > 0)
            {
                buttonInfo.Image.sprite = word.wordImage[Random.Range(0, word.wordImage.Count)];
            }

            ClickAndHoldHandler clickHandler = buttonObject.GetComponent<ClickAndHoldHandler>() 
                                      ?? buttonObject.AddComponent<ClickAndHoldHandler>();
            
            clickHandler.SetHoldTime(buttonHoldTime);

            clickHandler.onTap = () => PlayAudio(word);  // Play audio on tap
            clickHandler.onHold = () => CheckAnswer(word); // Check answer on hold
        }

        private void PlayAudio(WordData word)
        {
            Debug.Log($"{word.arabicWord}");
            if (word.wordAudio != null)
            {
                AudioSource.PlayClipAtPoint(word.wordAudio, Camera.main.transform.position);
            }
        }

        public void CheckAnswer(WordData word)
        {
            if (targetLetterSO.words.Contains(word))
            {
                Debug.Log("✅ " + ArabicSupport.Fix("صحيح!", true, true));
                HandleCorrectAnswerAsync().Forget();
            }
            else
            {
                Debug.Log("❌ " + ArabicSupport.Fix("خطأ! حاول مرة أخرى.", true, true));
            }
        }

        private async UniTaskVoid HandleCorrectAnswerAsync()
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(correctAnswerDelaySec));
            RestartGame();
        }

        [ContextMenu("Restart Game")]
        public void RestartGame() => StartGame();

        [ContextMenu("Redisplay Game")]
        public void RedisplayWords() => DisplayWords();
    }
}
