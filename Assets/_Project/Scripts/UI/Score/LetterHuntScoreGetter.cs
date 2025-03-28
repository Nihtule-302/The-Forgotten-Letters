using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.SaveSystem;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI.Score
{
    public class LetterHuntScoreGetter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI correctTmp;
        [SerializeField] private TextMeshProUGUI incorrectTmp;
        private LetterHuntData letterHuntData;
        void Start()
        {
            letterHuntData = PersistentSOManager.GetSO<LetterHuntData>();
            FirebaseManager.Instance.OnLetterHuntDataUpdated += SetScore;
            SetScore();
        }

        void Update()
        {
            SetScore();
        }

        private void SetScore()
        {
            if(letterHuntData == null) return;

            SetCorrectScore();
            SetIncorrectScore();
        }

        private void SetCorrectScore()
        {
            if (correctTmp == null) return;
            correctTmp.SetText(letterHuntData.correctScore.ToString());
        }

        private void SetIncorrectScore()
        {
            if (incorrectTmp == null) return;
            incorrectTmp.SetText(letterHuntData.incorrectScore.ToString());
        }
    }
}
