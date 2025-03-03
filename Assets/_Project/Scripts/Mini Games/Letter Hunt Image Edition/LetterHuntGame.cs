using UnityEngine;        
using TMPro;                   
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Flexalon;

namespace _Project.Scripts.Mini_Games.Letter_Hunt_Image_Edition
{
    public class LetterHuntGame : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI letterText;
        [SerializeField] FlexalonGridLayout grid;
        [SerializeField] GameObject choiceButtonPrefab;
        [SerializeField] List<LetterData> allLetters; 

        [SerializeField] private int totalButtons = 6; // 🔹 Total choices shown (adjustable)
        [SerializeField] private float correctRatio = 0.5f; // 🔹 % of correct answers (0.5 = 50%)

        [Header("Debugging")]
        [SerializeField] private LetterData targetLetterSO;
        [SerializeField] private string targetLetter;
        [SerializeField] private List<LetterData> distractorLetters;
        [SerializeField]private List<WordData> allChoices = new List<WordData>();
        [SerializeField] private List<GameObject> choiceButtons = new();

        void Start()
        {
            StartGame();
        }

        public void StartGame()
        {
            InitializeGame();
            SelectWords();
            DisplayWords();
        }

        private void InitializeGame()
        {
            // Select a random letter
            targetLetterSO = allLetters[Random.Range(0, allLetters.Count)];
            targetLetter = targetLetterSO.letter; // ✅ Fix: Assign target letter
            letterText.GetComponent<ArabicFixerTMPRO>().fixedText = targetLetter;

             // 2. Select 3 distractor letters (ي, ل, ت)
            distractorLetters = allLetters.Where(letter => letter != targetLetterSO).ToList();
            distractorLetters.Shuffle();
            distractorLetters = distractorLetters.Take(3).ToList();

            grid.Columns = (uint)Mathf.CeilToInt(Mathf.Sqrt(totalButtons));
            grid.Rows = (uint)Mathf.CeilToInt((float)totalButtons / grid.Columns);
        }
    
        private void SelectWords()
        {
            // Determine number of correct/incorrect words dynamically
            int numCorrect = Mathf.CeilToInt(totalButtons * correctRatio);
            int numIncorrect = totalButtons - numCorrect;


            // Select 2 correct words
            List<WordData> correctWords = new List<WordData>(targetLetterSO.words);
            correctWords.Shuffle();
            correctWords = correctWords.Take(numCorrect).ToList(); // ✅ Fix: Ensure selection is stored

            // Select 2 incorrect words
            HashSet<WordData> wrongWords = new HashSet<WordData>();
            foreach (var letter in distractorLetters)
            {
                if (letter.words.Count > 0 && wrongWords.Count < numIncorrect)
                {
                    wrongWords.Add(letter.words[Random.Range(0, letter.words.Count)]);
                }
            }

            // Ensure we have enough incorrect words (fallback)
            while (wrongWords.Count < numIncorrect)
            {
                LetterData randomDistractor = distractorLetters[Random.Range(0, distractorLetters.Count)];
                if (randomDistractor.words.Count > 0)
                {
                    wrongWords.Add(randomDistractor.words[Random.Range(0, randomDistractor.words.Count)]);
                }
            }

            // Combine correct and incorrect words
            allChoices.Clear();
            allChoices.AddRange(correctWords);
            allChoices.AddRange(wrongWords);
            allChoices.Shuffle();
        }

        private void DisplayWords()
        {
            choiceButtons.Clear();
            // Clear previous buttons
            foreach (Transform child in grid.transform)
            {
                Destroy(child.gameObject);
            }
            
            // Create buttons
            foreach (WordData word in allChoices)
            {
                var newButton = Instantiate(choiceButtonPrefab, grid.transform);
                ButtonInfoAccess buttonInfo = newButton.GetComponent<ButtonInfoAccess>();
                buttonInfo.Text.text = word.arabicWord;
                buttonInfo.Text.GetComponent<ArabicFixerTMPRO>().fixedText = word.arabicWord;
                newButton.GetComponent<Button>().onClick.AddListener(() => CheckAnswer(word));
                choiceButtons.Add(newButton);
            }
        }

        public void CheckAnswer(WordData word)
        {
            if (targetLetterSO.words.Contains(word))
            {
                
                Debug.Log("✅ " +ArabicSupport.Fix("صحيح!", true, true));
                RestartGame();
            }
            else
            {
                Debug.Log("❌ " + ArabicSupport.Fix("خطأ! حاول مرة أخرى.", true, true));
            }
        }

        [ContextMenu("Restart Game")]
        public void RestartGame()
        {
            StartGame();
        }
    }
}
