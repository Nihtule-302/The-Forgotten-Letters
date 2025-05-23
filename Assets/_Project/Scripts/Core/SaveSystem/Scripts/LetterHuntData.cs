using System;
using System.Collections.Generic;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.Utilities;
using _Project.Scripts.Core.Utilities.Builders;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

namespace _Project.Scripts.Core.SaveSystem
{
    [CreateAssetMenu(fileName = "LetterHuntData", menuName = "GameData/LetterHuntData")]
    public class LetterHuntData : ScriptableObject
    {
        public int correctScore;
        public int incorrectScore;
        public List<LetterHuntRound> rounds = new();

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
            await FirebaseManager.Instance.SaveLetterHuntData(this);
        }

        public void UpdateData(LetterHuntDataBuilder builder)
        {
            correctScore = builder.CorrectScore;
            incorrectScore = builder.IncorrectScore;
            rounds = new List<LetterHuntRound>(builder.Rounds);
        }

        public void UpdateData(LetterHuntDataSerializable letterHuntDataSerializable)
        {
            correctScore = letterHuntDataSerializable.correctScore;
            incorrectScore = letterHuntDataSerializable.incorrectScore;
            rounds = new List<LetterHuntRound>(letterHuntDataSerializable.rounds);
        }

        public LetterHuntDataBuilder GetBuilder()
        {
            return new LetterHuntDataBuilder(this);
        }
    }

    [FirestoreData]
    public class LetterHuntDataSerializable
    {
        // ✅ Default constructor required for Firestore
        public LetterHuntDataSerializable()
        {
            correctScore = 0;
            incorrectScore = 0;
            rounds = new List<LetterHuntRound>();
        }

        // ✅ Constructor for conversion from LetterHuntData
        public LetterHuntDataSerializable(LetterHuntData data)
        {
            correctScore = data.correctScore;
            incorrectScore = data.incorrectScore;
            rounds = new List<LetterHuntRound>(data.rounds);
        }

        [FirestoreProperty] public int correctScore { get; set; }

        [FirestoreProperty] public int incorrectScore { get; set; }

        [FirestoreProperty] public List<LetterHuntRound> rounds { get; set; }
    }

    [Serializable]
    [FirestoreData]
    public class LetterHuntRound
    {
        public string TargetLetter;
        public string chosenWord;
        public bool IsCorrect;
        public string TimestampCairoTime;

        [FirestoreProperty]
        public string targetLetter
        {
            get => TargetLetter;
            set => TargetLetter = value;
        }

        [FirestoreProperty]
        public string chosenWordCorrectFormate
        {
            get => chosenWord.ReverseText();

            set => chosenWord = value.ReverseText();
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
    }
}