using UnityEngine.Events;

namespace _Project.Scripts.Core.Scriptable_Events.EventTypes.LetterData
{
    public class LetterDataEventListener : GameEventListener<DataTypes.LetterData>{}

    [System.Serializable]
    public class LetterDataUnityEvent : UnityEvent<DataTypes.LetterData> { }
}