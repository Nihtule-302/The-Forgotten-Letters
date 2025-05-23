using System.Collections.Generic;
using _Project.Scripts.Core.DataTypes;
using UnityEngine;

namespace _Project.Scripts.Gameplay.Mini_Games
{
    public class LetterSelectionStrategies
    {
    }

    public class RandomLetterSelectionStrategy : ILetterSelectionStrategy
    {
        public LetterData SelectLetter(List<LetterData> availableLetters)
        {
            return availableLetters[Random.Range(0, availableLetters.Count)];
        }
    }

    public class FixedLetterSelectionStrategy : ILetterSelectionStrategy
    {
        private LetterData _fixedLetter;

        public FixedLetterSelectionStrategy(LetterData providedLetter)
        {
            _fixedLetter = providedLetter;
        }

        public LetterData SelectLetter(List<LetterData> availableLetters)
        {
            return _fixedLetter;
        }

        public void SetLetter(LetterData newLetter)
        {
            _fixedLetter = newLetter;
        }
    }
}