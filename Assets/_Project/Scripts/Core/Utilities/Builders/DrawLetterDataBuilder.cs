using System;
using System.Collections.Generic;
using _Project.Scripts.Core.Utilities;

public class DrawLetterDataBuilder
{
    public DrawLetterDataBuilder()
    {
        CorrectScore = 0;
        IncorrectScore = 0;
        Rounds = new List<DrawLetterRound>();
    }

    public DrawLetterDataBuilder(DrawLetterData existingData)
    {
        CorrectScore = existingData.correctScore;
        IncorrectScore = existingData.incorrectScore;
        Rounds = new List<DrawLetterRound>(existingData.rounds);
    }

    public int CorrectScore { get; private set; }
    public int IncorrectScore { get; private set; }
    public List<DrawLetterRound> Rounds { get; }

    public DrawLetterDataBuilder SetCorrectScore(int score)
    {
        CorrectScore = score;
        return this;
    }

    public DrawLetterDataBuilder SetIncorrectScore(int score)
    {
        IncorrectScore = score;
        return this;
    }

    public DrawLetterDataBuilder IncrementCorrectScore(int amount = 1)
    {
        CorrectScore += amount;
        return this;
    }

    public DrawLetterDataBuilder IncrementIncorrectScore(int amount = 1)
    {
        IncorrectScore += amount;
        return this;
    }


    public DrawLetterDataBuilder AddRound(string targetLetter, bool isCorrect)
    {
        var timeStamp = TimeHelpers.GetCurrentUtcIsoTimestamp();
        Rounds.Add(new DrawLetterRound
        {
            targetLetter = targetLetter,
            isCorrect = isCorrect,
            timestampCairoTime = timeStamp
        });
        return this;
    }
}