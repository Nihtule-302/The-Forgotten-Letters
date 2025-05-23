using UnityEngine;

public abstract class IInteractable : MonoBehaviour
{
    public abstract void Interact(); // Method to be called when the object is interacted with
    public abstract bool IsInteractable(); // Method to check if the object can be interacted with
}