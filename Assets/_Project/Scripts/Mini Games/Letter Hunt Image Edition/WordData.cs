using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Mini_Games.Letter_Hunt_Image_Edition
{
    [CreateAssetMenu(fileName = "NewWord", menuName = "LetterGame/Word")]
    public class WordData : ScriptableObject
    {
        public string arabicWord;
        public List<Sprite> wordImage = new();
        public AudioClip wordAudio;

        void OnEnable()
        {
            SetWordBasedOnSOName();
        }

        [ContextMenu("Set word based on SO name")]
        private void SetWordBasedOnSOName()
        {
            arabicWord = this.name;
        }
    }
}