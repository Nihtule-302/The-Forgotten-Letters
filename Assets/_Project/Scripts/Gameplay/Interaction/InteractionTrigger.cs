using _Project.Scripts.UI.Buttons;
using UnityEngine;

public class InteractionTrigger : MonoBehaviour
{
    public enum VisualCueMode { YAxisOnly, XAxisOnly, CustomDirection }

    [Header("Visual Cue")]
    [SerializeField] private GameObject visualCue;
    [SerializeField] private VisualCueMode animationMode = VisualCueMode.YAxisOnly;

    [SerializeField] private float visualCueYPosition = 0.1f;     // Used in Y-axis mode
    [SerializeField] private float visualCueXPosition = 0.1f;     // Used in X-axis mode
    [SerializeField] private Vector3 visualCueOffset = new Vector3(0.1f, 0.1f, 0f); // Used in custom mode
    [SerializeField] private float visualCueMoveDuration = .7f;

    [SerializeField] private IInteractable interactable;
    [SerializeField] private GameObject interactionButton;

    private Vector3 initialWorldPosition;
    private bool isPlayerInRange = false;

    void Awake()
    {
        isPlayerInRange = false;
        visualCue.SetActive(false);
        interactionButton.SetActive(false);

        initialWorldPosition = visualCue.transform.position;

        StartVisualCueAnimation();
    }

    private void StartVisualCueAnimation()
    {
        LeanTween.cancel(visualCue);

        switch (animationMode)
        {
            case VisualCueMode.YAxisOnly:
                AnimateYAxis();
                break;
            case VisualCueMode.XAxisOnly:
                AnimateXAxis();
                break;
            case VisualCueMode.CustomDirection:
                AnimateCustomDirection();
                break;
        }
    }

    private void AnimateYAxis()
    {
        LeanTween.moveY(visualCue, visualCue.transform.position.y + visualCueYPosition, visualCueMoveDuration)
                 .setLoopPingPong()
                 .setEaseInOutSine();
    }

    private void AnimateXAxis()
    {
        LeanTween.moveX(visualCue, visualCue.transform.position.x + visualCueXPosition, visualCueMoveDuration)
                 .setLoopPingPong()
                 .setEaseInOutSine();
    }

    private void AnimateCustomDirection()
    {
        LeanTween.move(visualCue, visualCue.transform.position + visualCueOffset, visualCueMoveDuration)
                 .setLoopPingPong()
                 .setEaseInOutSine();
    }

    [ContextMenu("Reset Visual Cue Animation")]
    public void ResetVisualCueAnimation()
    {
        LeanTween.cancel(visualCue);
        visualCue.transform.position = initialWorldPosition;
        StartVisualCueAnimation();
    }

    void Update()
    {
        visualCue.SetActive(isPlayerInRange);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            interactionButton.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            interactionButton.SetActive(false);
        }
    }
}
