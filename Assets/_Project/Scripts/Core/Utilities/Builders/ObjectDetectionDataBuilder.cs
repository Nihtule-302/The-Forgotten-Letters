using System;
using System.Collections.Generic;
using _Project.Scripts.Core.Utilities;

public class ObjectDetectionDataBuilder
{
    public ObjectDetectionDataBuilder()
    {
        CorrectScore = 0;
        IncorrectScore = 0;
        Rounds = new List<ObjectDetectionRound>();
    }

    public ObjectDetectionDataBuilder(ObjectDetectionData existingData)
    {
        CorrectScore = existingData.correctScore;
        IncorrectScore = existingData.incorrectScore;
        Rounds = new List<ObjectDetectionRound>(existingData.rounds);
    }

    public int CorrectScore { get; private set; }
    public int IncorrectScore { get; private set; }
    public List<ObjectDetectionRound> Rounds { get; }

    public ObjectDetectionDataBuilder SetCorrectScore(int score)
    {
        CorrectScore = score;
        return this;
    }

    public ObjectDetectionDataBuilder SetIncorrectScore(int score)
    {
        IncorrectScore = score;
        return this;
    }

    public ObjectDetectionDataBuilder IncrementCorrectScore(int amount = 1)
    {
        CorrectScore += amount;
        return this;
    }

    public ObjectDetectionDataBuilder IncrementIncorrectScore(int amount = 1)
    {
        IncorrectScore += amount;
        return this;
    }


    public ObjectDetectionDataBuilder AddRound(string targetLetter, bool isCorrect)
    {

        var timeStamp = TimeHelpers.GetCurrentUtcIsoTimestamp();
        Rounds.Add(new ObjectDetectionRound
        {
            targetLetter = targetLetter,
            isCorrect = isCorrect,
            timestampCairoTime = timeStamp
        });
        return this;
    }
}