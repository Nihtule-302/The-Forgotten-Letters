using System.Collections.Generic;
using _Project.Scripts.Core.DataTypes;
using UnityEngine;

public class LetterSelectionStrategies{}

public class RandomLetterSelectionStrategy : ILetterSelectionStrategy
{
    public LetterData SelectLetter(List<LetterData> availableLetters)
    {
        return availableLetters[Random.Range(0, availableLetters.Count)];
    }
}

public class FixedLetterSelectionStrategy : ILetterSelectionStrategy
{
    private LetterData fixedLetter;

    public FixedLetterSelectionStrategy(LetterData providedLetter)
    {
        fixedLetter = providedLetter;
    }

    public LetterData SelectLetter(List<LetterData> availableLetters)
    {
        return fixedLetter;
    }

    public void SetLetter(LetterData newLetter)
    {
        fixedLetter = newLetter;
    }
}
