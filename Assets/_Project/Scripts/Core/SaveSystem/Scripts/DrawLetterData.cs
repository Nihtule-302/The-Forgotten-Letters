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

    [ContextMenu("Reset Data")]
    public void ResetData()
    {
        correctScore = 0;
        incorrectScore = 0;
        rounds.Clear();
        SaveDataAfterReset().Forget();
    }

    private async UniTask SaveDataAfterReset()
    {
        await FirebaseManager.Instance.SaveDrawLetterData(this);
    }

    public void UpdateData(DrawLetterDataBuilder builder)
    {
        correctScore = builder.CorrectScore;
        incorrectScore = builder.IncorrectScore;
        rounds = new List<DrawLetterRound>(builder.Rounds);
    }

    public void UpdateData(DrawLetterDataSerializable drawLetterDataSerializable)
    {
        correctScore = drawLetterDataSerializable.correctScore;
        incorrectScore = drawLetterDataSerializable.incorrectScore;
        rounds = new List<DrawLetterRound>(drawLetterDataSerializable.rounds);
    }

    public DrawLetterDataBuilder GetBuilder()
    {
        return new DrawLetterDataBuilder(this);
    }
}

[FirestoreData]
public class DrawLetterDataSerializable
{
    // ✅ Default constructor required for Firestore
    public DrawLetterDataSerializable()
    {
        correctScore = 0;
        incorrectScore = 0;
        rounds = new List<DrawLetterRound>();
    }

    // ✅ Constructor for conversion from LetterHuntData
    public DrawLetterDataSerializable(DrawLetterData data)
    {
        correctScore = data.correctScore;
        incorrectScore = data.incorrectScore;
        rounds = new List<DrawLetterRound>(data.rounds);
    }

    [FirestoreProperty] public int correctScore { get; set; }

    [FirestoreProperty] public int incorrectScore { get; set; }

    [FirestoreProperty] public List<DrawLetterRound> rounds { get; set; }
}

[Serializable]
[FirestoreData]
public class DrawLetterRound
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