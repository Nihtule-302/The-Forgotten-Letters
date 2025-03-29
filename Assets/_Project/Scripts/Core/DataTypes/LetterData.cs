using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Mini_Games.Letter_Hunt_Image_Edition
{
    [CreateAssetMenu(fileName = "NewLetter", menuName = "LetterGame/Letter")]
    public class LetterData : ScriptableObject
    {
        public string letter;
        public List<WordData> words;

        public AudioClip letterAudio;

        void OnEnable()
        {
            SetLetterBasedOnSOName();
        }

        [ContextMenu("Set word based on SO name")]
        private void SetLetterBasedOnSOName()
        {
            letter = this.name;
        }
    }
}