using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ObjectDetectionUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject SelectLetterMenu;
    [SerializeField] private GameObject GameScreen;
    [SerializeField] private GameObject Yolo;
    // [SerializeField] private GameObject ScoreScreen;


    [Header("Answer Feedback")]
    [SerializeField] private GameObject correctScreen;
    [SerializeField] private GameObject wrongScreen;
    [SerializeField] private float answerFeedbackScreenDelay = 0.2f;
    [SerializeField] private float answerFeedbackScreenDuration = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartGame();
    }

    [ContextMenu("Start Game")]
    private void StartGame()
    {
        SelectLetterMenu.SetActive(true);
        GameScreen.SetActive(false);
        // ScoreScreen.SetActive(false);
        Yolo.SetActive(false);

        correctScreen.SetActive(false);
        wrongScreen.SetActive(false);
    }

    [ContextMenu("Start Object Detection")]
    public void StartObjectDetection()
    {
        RequestCameraPermissionAsync().Forget();
    }

    private async UniTaskVoid RequestCameraPermissionAsync()
    {
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            await Application.RequestUserAuthorization(UserAuthorization.WebCam);
            bool granted = Application.HasUserAuthorization(UserAuthorization.WebCam);

            if (!granted)
            {
                Debug.LogWarning("Camera permission denied.");
                return;
            }
        }

        await UniTask.Delay(0);

        ProceedWithObjectDetection();
    }

    private void ProceedWithObjectDetection()
    {
        SelectLetterMenu.SetActive(false);
        GameScreen.SetActive(true);
        // ScoreScreen.SetActive(true);
        EnableYoloWithDelayAsync().Forget();
    }
    [ContextMenu("Activate Letter Selection Screen")]
    public void ActivateLetterSelectionScreen()
    {
        StartGame();
    }

    private async UniTaskVoid EnableYoloWithDelayAsync()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(.5f));
        Yolo.SetActive(true);
    }
    [ContextMenu("Call Correct Screen")]
    public void CallCorrectScreen()
    {
        ShowWrongScreenWithDelayAsync().Forget();
    }
    [ContextMenu("Call Wrong Screen")]
    public void CallWrongScreen()
    {
        ShowCorrectScreenWithDelayAsync().Forget();
    }



    private async UniTaskVoid ShowWrongScreenWithDelayAsync()
    {
        wrongScreen.SetActive(false);
        correctScreen.SetActive(false);

        await UniTask.Delay(TimeSpan.FromSeconds(answerFeedbackScreenDelay));
        wrongScreen.SetActive(true);
        await UniTask.Delay(TimeSpan.FromSeconds(answerFeedbackScreenDuration));
        wrongScreen.SetActive(false);
    }
    private async UniTaskVoid ShowCorrectScreenWithDelayAsync()
    {
        wrongScreen.SetActive(false);
        correctScreen.SetActive(false);

        await UniTask.Delay(TimeSpan.FromSeconds(answerFeedbackScreenDelay));
        correctScreen.SetActive(true);
        await UniTask.Delay(TimeSpan.FromSeconds(answerFeedbackScreenDuration));
        correctScreen.SetActive(false);
    }
}
