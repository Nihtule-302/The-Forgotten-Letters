using System;
using UnityEngine.Events;

namespace _Project.Scripts.Core.Scriptable_Events.EventTypes.LetterPredictionData
{
    public class LetterPredictionDataEventListener : GameEventListener<DataTypes.LetterPredictionData>
    {
    }

    [Serializable]
    public class LetterPredictionDataUnityEvent : UnityEvent<DataTypes.LetterPredictionData>
    {
    }
}