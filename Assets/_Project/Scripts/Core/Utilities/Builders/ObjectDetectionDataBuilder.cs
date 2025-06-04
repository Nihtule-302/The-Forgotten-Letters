using System.Collections.Generic;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Utilities;
using Cysharp.Threading.Tasks;

public class ObjectDetectionDataBuilder
{
    public int CorrectScore { get; private set; }
    public int IncorrectScore { get; private set; }
    public List<ObjectDetectionRound> Rounds { get; }

    private ObjectDetectionData SoRef => PersistentSOManager.GetSO<ObjectDetectionData>();

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

    public ObjectDetectionDataBuilder UpdateLocalData()
    {
        SoRef?.UpdateLocalData(this);
        return this;
    }

    public ObjectDetectionDataSerializable SerializeObjectDetectionData()
    {
        return new ObjectDetectionDataSerializable(SoRef);
    }

    public void SaveDataToFirebase()
    {
        SaveDataToFirebaseAsync().Forget();
    }

    public async UniTask SaveDataToFirebaseAsync()
    {
        var data = SerializeObjectDetectionData();
        if (data != null)
        {
            await FirebaseManager.Instance.SaveObjectDetectionData(data);
        }
        else
        {
            UnityEngine.Debug.LogError("Failed to serialize ObjectDetectionData.");
        }
    }
}