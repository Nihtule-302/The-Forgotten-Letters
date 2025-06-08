using System;
using _Project.Scripts.Core.Scriptable_Events;
using _Project.Scripts.Core.Scriptable_Events.EventTypes.Float;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AddresableSceneContainer : MonoBehaviour
{
    [Header("Addresable Scene Container Settings")]
    [SerializeField] private GameObject OrginalSceneContainer;
    [SerializeField] private AddresableLoaderAndUnloader addresableLoaderAndUnloader;

    [Header("Event References")]
    [SerializeField] private AssetReference ShowLoadingScreenNoBarRef;
    [SerializeField] private AssetReference HideLoadingScreenRef;
    [SerializeField] private AssetReference updateProgressBarRef;
    [SerializeField] private AssetReference ActivateSceneContainerRef;
    [SerializeField] private AssetReference DeactivateSceneContainerRef;

    [Header("Listeners")]
    [SerializeField] private GameEventListener triggerNextSetupStepListener;

    FloatEvent updateProgressBarEvent => EventLoader.Instance.GetEvent<FloatEvent>(updateProgressBarRef);
    GameEvent showLoadingScreenNoBarEvent => EventLoader.Instance.GetEvent<GameEvent>(ShowLoadingScreenNoBarRef);
    GameEvent hideLoadingScreenEvent => EventLoader.Instance.GetEvent<GameEvent>(HideLoadingScreenRef);
    GameEvent activateSceneContainerEvent => EventLoader.Instance.GetEvent<GameEvent>(ActivateSceneContainerRef);
    GameEvent deactivateSceneContainerEvent => EventLoader.Instance.GetEvent<GameEvent>(DeactivateSceneContainerRef);

    void Awake()
    {
        showLoadingScreenNoBarEvent?.Raise();
        triggerNextSetupStepListener?.RegisterListener();
    }
    public void StartAdressing()
    {
        if (OrginalSceneContainer != null)
        {
            Destroy(OrginalSceneContainer);
            OrginalSceneContainer = null;
        }

        LoadSceneContainer().Forget();
        triggerNextSetupStepListener?.DeregisterListener();
    }

    public async UniTaskVoid LoadSceneContainer()
    {
        if (addresableLoaderAndUnloader != null)
        {
            await addresableLoaderAndUnloader.LoadAssetAsync();
            await UniTask.Delay(TimeSpan.FromSeconds(1)); // Ensure the scene is fully loaded before raising the event
            hideLoadingScreenEvent?.Raise();
            activateSceneContainerEvent?.Raise();
        }
        else
        {
            Debug.LogError("AddresableLoaderAndUnloader is not assigned in AddresableSceneContainer.");
        }
    }

    public void UnloadSceneContainer()
    {
        if (addresableLoaderAndUnloader != null)
        {
            deactivateSceneContainerEvent?.Raise();
            addresableLoaderAndUnloader.UnloadAsset();
        }
        else
        {
            Debug.LogError("AddresableLoaderAndUnloader is not assigned in AddresableSceneContainer.");
        }
        
    }
}
