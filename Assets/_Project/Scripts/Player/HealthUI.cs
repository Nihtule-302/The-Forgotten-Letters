using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheForgottenLetters
{
    public class HealthUI : MonoBehaviour
    {
        public Image heartPrefab;
        public Sprite fullHeartSprite;
        public Sprite emptyHeartSprite;

        [SerializeField] private List<Image> hearts = new List<Image>();

        [SerializeField] private PlayerHealth playerHealth;
        
        private void Start()
        {
            SetMaxHearts(playerHealth.maxHealth);
        }
        public void SetMaxHearts(int maxHearts)
        {
            foreach (var heart in hearts)
            {
                Destroy(heart.gameObject);
            }

            hearts.Clear();

            for (int i = 0; i < maxHearts; i++)
            {
                Image newHeart = Instantiate(heartPrefab, transform);
                hearts.Add(newHeart);
            }

            UpdateHearts();
        }

        public void UpdateHearts()
        {
            for (int i = 0; i < hearts.Count; i++)
            {
                if (i < playerHealth.CurrentHealth)
                {
                    hearts[i].sprite = fullHeartSprite;
                }
                else
                {
                    hearts[i].sprite = emptyHeartSprite;
                }
            }
        }

    }
}
