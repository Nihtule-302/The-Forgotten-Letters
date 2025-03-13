using System;
using UnityEngine;

public class ChoiceButtonController : MonoBehaviour
{
    [SerializeField] private ButtonInfoAccess buttonInfo;
    [SerializeField] private ClickAndHoldHandler clickHandler;
    [SerializeField] private WordData word;
    [SerializeField] private DissolveControl dissolveControl;

    public Action onHoldActionComplete;

    public void SetUpChoiceButton(WordData wordData, float holdDuration)
    {
        InitializeComponents();
        AssignWordData(wordData);
        SetButtonImage();
        ConfigureClickHandler(holdDuration);
    }

    private void InitializeComponents()
    {
        if(buttonInfo == null)
            buttonInfo = GetComponent<ButtonInfoAccess>();
            
        if(clickHandler == null)
        clickHandler = GetComponent<ClickAndHoldHandler>() ?? gameObject.AddComponent<ClickAndHoldHandler>();
    }

    private void AssignWordData(WordData wordData)
    {
        word = wordData;
        buttonInfo.Text.text = word.arabicWord;
        buttonInfo.Text.GetComponent<ArabicFixerTMPRO>().fixedText = word.arabicWord;
    }

    private void SetButtonImage()
    {
        if (word.wordImage != null && word.wordImage.Count > 0 && buttonInfo.Image != null)
        {
            buttonInfo.Image.sprite = word.wordImage[UnityEngine.Random.Range(0, word.wordImage.Count)];
        }
    }

    private void ConfigureClickHandler(float holdDuration)
    {
        clickHandler.SetHoldTime(holdDuration);
        clickHandler.onTap = () => PlayAudio();
        clickHandler.onHold = () => onHoldActionComplete?.Invoke();
    }

    private void PlayAudio()
    {
        Debug.Log($"{word.arabicWord}");
        if (word.wordAudio != null)
        {
            AudioSource.PlayClipAtPoint(word.wordAudio, Camera.main.transform.position);
        }
    }
}
