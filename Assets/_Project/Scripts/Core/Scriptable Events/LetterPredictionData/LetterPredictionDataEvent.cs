using UnityEngine;

namespace _Project.Scripts.Core.Scriptable_Events.EventTypes.LetterData
{
    [CreateAssetMenu(fileName = "LetterPredictionDataEvent", menuName = "Events/LetterPredictionDataEvent")]
    public class LetterPredictionDataEvent : GameEvent<DataTypes.LetterPredictionData>
    {
    }
}