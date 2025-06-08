using _Project.Scripts.Core.Scriptable_Events;
using _Project.Scripts.Core.Scriptable_Events.EventTypes.Float;
using _Project.Scripts.Core.Utilities.Scene_Management;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject loadingScreen;
    public GameObject progressBar;
    public Image progressBarFill;
    public GameObject loadingScreenCamera;

    [Header("Event Ref")]
    public AssetReference onProgressUpdateRef;
    public AssetReference onShowLoadingScreenRef;
    public AssetReference onHideLoadingScreenRef;

    [Header("Events Fallback")]
    public FloatEvent onProgressUpdate;
    public GameEvent onShowLoadingScreen;
    public GameEvent onHideLoadingScreen;

    [Header("sceneAdder")]
    public SceneAdder sceneAdder;

    private float currentProgress = 0f;
    private bool isHiding = false;

    // === Properties to safely retrieve events ===
    public FloatEvent OnProgressUpdateTrue => GetEventSafe(onProgressUpdateRef, onProgressUpdate);
    public GameEvent onShowLoadingScreenTrue => GetEventSafe(onShowLoadingScreenRef, onShowLoadingScreen);
    public GameEvent onHideLoadingScreenTrue => GetEventSafe(onHideLoadingScreenRef, onHideLoadingScreen);


    void Awake()
    {
        ShowScreen();
        sceneAdder.AddSceneAsync();
    }


    // === Public Methods ===
    public void ShowScreen()
    {
        loadingScreen.SetActive(true);
        loadingScreenCamera.SetActive(true);
        progressBar.SetActive(false);
        isHiding = false;
    }

    void ShowScreenWithProgress()
    {
        loadingScreen.SetActive(true);
        loadingScreenCamera.SetActive(true);
        progressBar.SetActive(true);
        isHiding = false;
    }

    public void HideScreenImmediate()
    {
        loadingScreen.SetActive(false);
        loadingScreenCamera.SetActive(false);
        progressBarFill.fillAmount = 0f;
        isHiding = true;
    }

    public void UpdateProgressBar(float targetProgress)
    {
        if (!loadingScreen.activeSelf || !loadingScreenCamera.activeSelf || !progressBar.activeSelf)
            ShowScreenWithProgress();

        currentProgress = Mathf.Clamp01(targetProgress);

        SetProgressFill(currentProgress);

        if (currentProgress >= 1f && !isHiding)
        {
            currentProgress = 1;
            ShowScreen();
            // SwitchToNoBarAfterDelay().Forget();
        }
    }

    // === Private Methods ===
    private void SetProgressFill(float value)
    {
        progressBarFill.fillAmount = value;
    }

    private async UniTaskVoid SwitchToNoBarAfterDelay()
    {
        await UniTask.Delay(1500); 
        ShowScreen();
    }

    

    private T GetEventSafe<T>(AssetReference reference, T fallback) where T : ScriptableObject
    {
        try
        {
            return EventLoader.Instance.GetEvent<T>(reference);
        }
        catch
        {
            return fallback;
        }
    }
}
