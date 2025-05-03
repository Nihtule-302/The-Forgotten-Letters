using _Project.Scripts.Core.Scriptable_Events;
using UnityEngine;

public class Bonfire : IInteractable
{

    [SerializeField] private GameEvent bonfireRestEvent;
    [SerializeField] private GameEvent bonfireStopRestEvent;

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
