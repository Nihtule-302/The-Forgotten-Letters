using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectDetectionData", menuName = "GameData/ObjectDetectionData")]
public class ObjectDetectionData : ScriptableObject
{
    public int correctScore;
    public int incorrectScore;
    public List<ObjectDetectionRound> rounds = new();

    [ContextMenu("Reset and Save Data To Firebase")]
    public void ResetAndSaveDataToFirebase()
    {
        ResetData();
        SaveData();
    }

    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        correctScore = 0;
        incorrectScore = 0;
        rounds.Clear();
    }

    [ContextMenu("Save Data")]
    private void SaveData()
    {
        SaveDataAsync().Forget();
    }

    private async UniTask SaveDataAsync()
    {
        var builder = GetBuilder();
        if (builder == null)
        {
            Debug.LogError("ObjectDetectionDataBuilder is null.");
            return;
        }

        await builder.SaveDataToFirebaseAsync();
    }

    public void UpdateLocalData(ObjectDetectionDataBuilder builder)
    {
        correctScore = builder.CorrectScore;
        incorrectScore = builder.IncorrectScore;
        rounds = new List<ObjectDetectionRound>(builder.Rounds);
    }

    public void UpdateLocalData(ObjectDetectionDataSerializable objectDetectionDataSerializable)
    {
        correctScore = objectDetectionDataSerializable.correctScore;
        incorrectScore = objectDetectionDataSerializable.incorrectScore;
        rounds = new List<ObjectDetectionRound>(objectDetectionDataSerializable.rounds);
    }

    public ObjectDetectionDataBuilder GetBuilder()
    {
        return new ObjectDetectionDataBuilder(this);
    }
}

[FirestoreData]
public class ObjectDetectionDataSerializable
{
    [FirestoreProperty] public int correctScore { get; set; }

    [FirestoreProperty] public int incorrectScore { get; set; }

    [FirestoreProperty] public List<ObjectDetectionRound> rounds { get; set; }


    // ✅ Default constructor required for Firestore
    public ObjectDetectionDataSerializable()
    {
        correctScore = 0;
        incorrectScore = 0;
        rounds = new List<ObjectDetectionRound>();
    }

    // ✅ Constructor for conversion from LetterHuntData
    public ObjectDetectionDataSerializable(ObjectDetectionData data)
    {
        correctScore = data.correctScore;
        incorrectScore = data.incorrectScore;
        rounds = new List<ObjectDetectionRound>(data.rounds);
    }
}

[Serializable]
[FirestoreData]
public class ObjectDetectionRound
{
    public string TargetLetter;

    public bool IsCorrect;

    // public string Duration;
    public string TimestampCairoTime;

    [FirestoreProperty]
    public string targetLetter
    {
        get => TargetLetter;
        set => TargetLetter = value;
    }

    [FirestoreProperty]
    public bool isCorrect
    {
        get => IsCorrect;
        set => IsCorrect = value;
    }

    // [FirestoreProperty]
    // public string duration
    // {
    //     get
    //     {
    //         return Duration;
    //     }
    //     set
    //     {
    //         Duration = value;
    //     }
    // }

    [FirestoreProperty]
    public string timestampCairoTime
    {
        get => TimestampCairoTime;
        set => TimestampCairoTime = value;
    }
}