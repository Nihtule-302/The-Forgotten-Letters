using UnityEngine;
using UnityEngine.Events;

public class LetterDataEventListener : GameEventListener<LetterData>{}

[System.Serializable]
public class LetterDataUnityEvent : UnityEvent<LetterData> { }