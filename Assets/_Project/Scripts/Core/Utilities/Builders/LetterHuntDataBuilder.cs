using System;
using System.Collections.Generic;
using _Project.Scripts.Core.Managers;
using _Project.Scripts.Core.SaveSystem;
using Cysharp.Threading.Tasks;

namespace _Project.Scripts.Core.Utilities.Builders
{
    public class LetterHuntDataBuilder
    {
        public int CorrectScore { get; private set; }
        public int IncorrectScore { get; private set; }
        public List<LetterHuntRound> Rounds { get; }

        private LetterHuntData SoRef => PersistentSOManager.GetSO<LetterHuntData>();

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
            string cairoTime = TimeHelpers.GetCurrentUtcIsoTimestamp();

            Rounds.Add(new LetterHuntRound
            {
                targetLetter = targetLetter,
                chosenWord = chosenWord,
                isCorrect = isCorrect,
                timestampCairoTime = cairoTime
            });

            return this;
        }

        public LetterHuntDataBuilder UpdateLocalData()
        {
            SoRef?.UpdateLocalData(this);
            return this;
        }

        public LetterHuntDataSerializable SerializeLetterHuntData()
        {
            return new LetterHuntDataSerializable(SoRef);
        }

        public void SaveDataToFirebase()
        {
            SaveDataToFirebaseAsync().Forget();
        }

        public async UniTask SaveDataToFirebaseAsync()
        {
            var data = SerializeLetterHuntData();
            if (data != null)
            {
                await FirebaseManager.Instance.SaveLetterHuntData(data);
            }
            else
            {
                UnityEngine.Debug.LogError("Failed to serialize LetterHuntData.");
            }
        }
    }
}
