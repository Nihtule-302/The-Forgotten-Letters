using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LetterHuntData", menuName = "GameData/LetterHuntData")]
public class LetterHuntData : ScriptableObject
{
    public int correctScore;
    public int incorrectScore;
    public List<LetterHuntRound> rounds = new List<LetterHuntRound>();

    public void ResetData()
    {
        correctScore = 0;
        incorrectScore = 0;
        rounds.Clear();
    }

    public void UpdateData(LetterHuntDataBuilder builder)
    {
        correctScore = builder.CorrectScore;
        incorrectScore = builder.IncorrectScore;
        rounds = new List<LetterHuntRound>(builder.Rounds);
    }

    public LetterHuntDataBuilder GetBuilder()
    {
        return new LetterHuntDataBuilder(this);
    }
}

[System.Serializable]
public class LetterHuntDataSerializable
{
    public int correctScore;
    public int incorrectScore;
    public List<LetterHuntRound> rounds;

    public LetterHuntDataSerializable(LetterHuntData data)
    {
        correctScore = data.correctScore;
        incorrectScore = data.incorrectScore;
        rounds = data.rounds;
    }
}

[System.Serializable]
public class LetterHuntRound
{
    public string targetLetter;
    public string chosenWord;
    public bool isCorrect;
    public string timestampCairoTime; // Store as string
}
