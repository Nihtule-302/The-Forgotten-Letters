using UnityEngine.Events;

namespace _Project.Scripts.Core.Scriptable_Events.EventTypes.LetterData
{
    public class LetterDataEventListener : GameEventListener<Mini_Games.Letter_Hunt_Image_Edition.LetterData>{}

    [System.Serializable]
    public class LetterDataUnityEvent : UnityEvent<Mini_Games.Letter_Hunt_Image_Edition.LetterData> { }
}