using System.Collections.Generic;
using System.Threading.Tasks;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Utilities;
using Cysharp.Threading.Tasks;

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

    public DrawLetterData SoRef => PersistentSOManager.GetSO<DrawLetterData>();

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

    public DrawLetterDataBuilder UpdateLocalData()
    {
        SoRef.UpdateLocalData(this);
        return this;
    }

    public DrawLetterDataSerializable SerializeDrawLetterData()
    {
        return new DrawLetterDataSerializable(SoRef);
    }

    public void SaveDataToFirebase()
    {
        SaveDataToFirebaseAsync().Forget();
    }

    public async UniTask SaveDataToFirebaseAsync()
    {
        await FirebaseManager.Instance.SaveDrawLetterData(SerializeDrawLetterData());
    }

}