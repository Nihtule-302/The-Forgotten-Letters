using System;
using UnityEngine.Events;

namespace _Project.Scripts.Core.Scriptable_Events.EventTypes.LetterData
{
    public class LetterDataEventListener : GameEventListener<DataTypes.LetterData>
    {
    }

    [Serializable]
    public class LetterDataUnityEvent : UnityEvent<DataTypes.LetterData>
    {
    }
}