using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Gameplay.Mini_Games.Draw_a_letter
{
    public class DrawLetterRoundUI : MonoBehaviour
    {
        public Image roundStarPrefab;
        public Sprite fullSprite;
        public Sprite emptySprite;

        [SerializeField] private List<Image> stars = new();

        [SerializeField] private DrawLetterGame drawLetterGame;

        private void Start()
        {
            SetMaxRounds(drawLetterGame.MaxRounds);
        }

        public void SetMaxRounds(int maxRounds)
        {
            foreach (var star in stars) Destroy(star.gameObject);

            stars.Clear();

            for (var i = 0; i < maxRounds; i++)
            {
                var newStar = Instantiate(roundStarPrefab, transform);
                stars.Add(newStar);
            }

            UpdateStars();
        }

        public void UpdateStars()
        {
            for (var i = 0; i < stars.Count; i++)
                if (i < drawLetterGame.CurrentRound)
                    stars[i].sprite = fullSprite;
                else
                    stars[i].sprite = emptySprite;
        }
    }
}