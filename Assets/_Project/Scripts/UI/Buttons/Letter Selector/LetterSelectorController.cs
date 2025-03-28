using _Project.Scripts.Core.Scriptable_Events.EventTypes.LetterData;
using _Project.Scripts.Mini_Games.Letter_Hunt_Image_Edition;
using UnityEngine;

namespace _Project.Scripts.UI.Buttons.Letter_Selector
{
    public class LetterSelectorController : MonoBehaviour
    {
        [SerializeField] private ButtonTouchHandler clickHandler;
        [SerializeField] private LetterData targetLetter;
        [SerializeField] private DissolveControl dissolveControl;

        [SerializeField] private LetterDataEvent onLetterSelected; 
        [SerializeField] private float holdTime = 0.2f;

        void Start() => SetUpSelectorButton();

        public void SetUpSelectorButton()
        {
            InitializeComponents();
            ConfigureClickHandler(holdTime);
        }

        private void InitializeComponents()
        {
            if(clickHandler == null)
                clickHandler = GetComponent<ButtonTouchHandler>() ?? gameObject.AddComponent<ButtonTouchHandler>();
        }

        private void ConfigureClickHandler(float holdDuration)
        {
            clickHandler.SetHoldTime(holdDuration);
            // clickHandler.onTap = () => PlayAudio();
            clickHandler.onHold = () => dissolveControl.StartDissolveEffect(HandleHoldActionComplete);
            clickHandler.onHoldRelease = () => dissolveControl.ResetDissolveEffect();
        }

        // private void PlayAudio()
        // {
        //     Debug.Log($"{word.arabicWord}");
        //     if (word.wordAudio != null)
        //     {
        //         AudioSource.PlayClipAtPoint(word.wordAudio, Camera.main.transform.position);
        //     }
        // }

        private void HandleHoldActionComplete()
        {
            onLetterSelected.Raise(targetLetter);     
        }
    }
}
