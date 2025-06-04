using System;
using System.Collections.Generic;
using _Project.Scripts.Core.Managers;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

[CreateAssetMenu(fileName = "DrawLetterData", menuName = "GameData/DrawLetterData")]
public class DrawLetterData : ScriptableObject
{
    public int correctScore;
    public int incorrectScore;
    public List<DrawLetterRound> rounds = new();

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
            Debug.LogError("DrawLetterDataBuilder is null.");
            return;
        }

        await builder.SaveDataToFirebaseAsync();
    }

    public void UpdateLocalData(DrawLetterDataBuilder builder)
    {
        correctScore = builder.CorrectScore;
        incorrectScore = builder.IncorrectScore;
        rounds = new List<DrawLetterRound>(builder.Rounds);
    }

    public void UpdateLocalData(DrawLetterDataSerializable dataSerializable)
    {
        correctScore = dataSerializable.correctScore;
        incorrectScore = dataSerializable.incorrectScore;
        rounds = new List<DrawLetterRound>(dataSerializable.rounds);
    }

    public DrawLetterDataBuilder GetBuilder()
    {
        return new DrawLetterDataBuilder(this);
    }
}

[FirestoreData]
public class DrawLetterDataSerializable
{
    [FirestoreProperty] public int correctScore { get; set; }
    [FirestoreProperty] public int incorrectScore { get; set; }
    [FirestoreProperty] public List<DrawLetterRound> rounds { get; set; }


    // Required for Firestore serialization
    public DrawLetterDataSerializable()
    {
        correctScore = 0;
        incorrectScore = 0;
        rounds = new List<DrawLetterRound>();
    }

    // Conversion constructor
    public DrawLetterDataSerializable(DrawLetterData data)
    {
        correctScore = data.correctScore;
        incorrectScore = data.incorrectScore;
        rounds = new List<DrawLetterRound>(data.rounds);
    }
}

[Serializable]
[FirestoreData]
public class DrawLetterRound
{
    public string TargetLetter;
    public bool IsCorrect;
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

    [FirestoreProperty]
    public string timestampCairoTime
    {
        get => TimestampCairoTime;
        set => TimestampCairoTime = value;
    }

    // You can bring this back if you later track task duration
    // [FirestoreProperty]
    // public string duration { get; set; }
}