using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Core.DataTypes
{
    [CreateAssetMenu(fileName = "NewLetter", menuName = "LetterGame/Letter")]
    public class LetterData : ScriptableObject
    {
        public string letter;
        public List<WordData> words;
        public List<WordData> objectDetectionWords;

        public AudioClip letterAudio;

        private void OnEnable()
        {
            SetLetterBasedOnSOName();
        }

        [ContextMenu("Set word based on SO name")]
        private void SetLetterBasedOnSOName()
        {
            letter = name;
        }
    }
}