using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheForgottenLetters
{
    public class DrawLetterRoundUI : MonoBehaviour
    {
        public Image roundStarPrefab;
        public Sprite fullSprite;
        public Sprite emptySprite;

        [SerializeField] private List<Image> stars = new List<Image>();

        [SerializeField] private DrawLetterGame drawLetterGame;
        
        private void Start()
        {
            SetMaxRounds(drawLetterGame.maxRounds);
        }
        public void SetMaxRounds(int maxRounds)
        {
            foreach (var star in stars)
            {
                Destroy(star.gameObject);
            }

            stars.Clear();

            for (int i = 0; i < maxRounds; i++)
            {
                Image newStar = Instantiate(roundStarPrefab, transform);
                stars.Add(newStar);
            }

            UpdateStars();
        }

        public void UpdateStars()
        {
            for (int i = 0; i < stars.Count; i++)
            {
                if (i < drawLetterGame.CurrentRound)
                {
                    stars[i].sprite = fullSprite;
                }
                else
                {
                    stars[i].sprite = emptySprite;
                }
            }
        }

    }
}
