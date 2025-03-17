using System;
using System.Collections.Generic;

public class LetterHuntDataBuilder
{
    public int CorrectScore { get; private set; }
    public int IncorrectScore { get; private set; }
    public List<LetterHuntRound> Rounds { get; private set; }

    public LetterHuntDataBuilder()
    {
        CorrectScore = 0;
        IncorrectScore = 0;
        Rounds = new List<LetterHuntRound>();
    }

    public LetterHuntDataBuilder(LetterHuntData existingData)
    {
        CorrectScore = existingData.correctScore;
        IncorrectScore = existingData.incorrectScore;
        Rounds = new List<LetterHuntRound>(existingData.rounds);
    }

    public LetterHuntDataBuilder SetCorrectScore(int score)
    {
        CorrectScore = score;
        return this;
    }

    public LetterHuntDataBuilder SetIncorrectScore(int score)
    {
        IncorrectScore = score;
        return this;
    }

    public LetterHuntDataBuilder IncrementCorrectScore(int amount = 1)
    {
        CorrectScore += amount;
        return this;
    }

    public LetterHuntDataBuilder IncrementIncorrectScore(int amount = 1)
    {
        IncorrectScore += amount;
        return this;
    }

    public LetterHuntDataBuilder AddRound(string targetLetter, string chosenWord, bool isCorrect)
    {
        TimeZoneInfo cairoTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
        DateTime cairoTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cairoTimeZone);
        Rounds.Add(new LetterHuntRound
        {
            targetLetter = targetLetter,
            chosenWord = chosenWord,
            isCorrect = isCorrect,
            timestampCairoTime = cairoTime.ToString("yyyy-MM-dd hh:mm:ss tt") 
        });
        return this;
    }
}
