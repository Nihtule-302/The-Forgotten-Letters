using System;

namespace _Project.Scripts.Core.DataTypes
{
    [Serializable]
    public class LetterPredictionData
    {
        public char letter;

        private float _probability;

        public float probability
        {
            get => _probability;
            set => _probability = value * 100f;
        }

        public override string ToString()
        {
            return $"Letter: {letter}, Probability: {probability}";
        }
    }
}