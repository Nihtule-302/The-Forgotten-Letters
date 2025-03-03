using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWord", menuName = "LetterGame/Word")]
public class WordData : ScriptableObject
{
    public string arabicWord;
    public Sprite wordImage;

    void OnEnable()
    {
        SetWordBasedOnSOName();
    }

    [ContextMenu("Set word based on SO name")]
    private void SetWordBasedOnSOName()
    {
        arabicWord = this.name;
    }
}