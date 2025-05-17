using _Project.Scripts.Core.Scriptable_Events;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class Bonfire : IInteractable
{

    [SerializeField] private AssetReference bonfireRestEventRef;
    [SerializeField] private AssetReference bonfireStopRestEventRef;

    [SerializeField] private GameEvent bonfireRestEvent => EventLoader.Instance.GetEvent<GameEvent>(bonfireRestEventRef);
    [SerializeField] private GameEvent bonfireStopRestEvent => EventLoader.Instance.GetEvent<GameEvent>(bonfireStopRestEventRef);

    public bool IsPlayerResting {get; private set;}

    public override bool IsInteractable()
    {
        return !IsPlayerResting;
    }

    public override void Interact()
    {
        if (!IsInteractable()) return;

        RestAtBonfire();
    }

    public void RestAtBonfire()
    {
        IsPlayerResting = true;
        bonfireRestEvent.Raise(); // Notify that the player is resting at the bonfire
    }

    public void StopResting()
    {
        IsPlayerResting = false;
        bonfireStopRestEvent.Raise(); // Notify that the player has stopped resting at the bonfire
    }

}
