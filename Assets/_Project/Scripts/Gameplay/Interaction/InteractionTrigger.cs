using _Project.Scripts.UI.Buttons;
using UnityEngine;
using UnityEngine.UI;

public class InteractionTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;
    [SerializeField] private float visualCueYPosition = 0.5f; // The Y position to move the visual cue to
    [SerializeField] private float visualCueMoveDuration = 1f; // The duration of the move animation
    [SerializeField] private IInteractable interactable; // Reference to the interactable object
    [SerializeField] private GameObject interactionButton; // Reference to the button touch handler
    private Transform visualCueTransform; // Reference to the visual cue's transform
    private bool isPlayerInRange = false;

    void Awake()
    {
        isPlayerInRange = false;
        visualCue.SetActive(false);
        interactionButton.SetActive(false); // Hide the interaction button at the start

        visualCueTransform = visualCue.transform; // Cache the transform of the visual cue
        LeanTween.moveY(visualCue, visualCue.transform.position.y + visualCueYPosition, visualCueMoveDuration).setLoopPingPong().setEaseInOutSine(); // Restart the bobbing animation
    }

    [ContextMenu("Reset Visual Cue Animation")]
    public void ResetVisualCueAnimation()
    {
        visualCue.transform.localPosition = visualCueTransform.localPosition; // Reset the position of the visual cue
        LeanTween.cancel(visualCue); // Cancel any existing animations on the visual cue
        LeanTween.moveY(visualCue, visualCue.transform.position.y + visualCueYPosition, visualCueMoveDuration).setLoopPingPong().setEaseInOutSine(); // Restart the bobbing animation
    }

    void Update()
    {
        if (isPlayerInRange)
        {
            visualCue.SetActive(true); // Show the visual cue when player is in range
        }
        else
        {
            visualCue.SetActive(false); // Show the visual cue when player is in range
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            interactionButton.SetActive(true); // Show the interaction button when player enters the trigger area
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}
