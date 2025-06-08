using UnityEngine;

public class handelSetupStep : MonoBehaviour
{
    [SerializeField] private GameObject UI;
    [SerializeField] private GameObject[] elements;

    void Awake()
    {
        if (elements == null || elements.Length == 0)
        {
            Debug.LogWarning("No elements assigned to handelSetupStep.");
            return;
        }

        foreach (var element in elements)
        {
            if (element != null)
            {
                element.SetActive(false);
            }
            else
            {
                Debug.LogWarning("One of the elements is null in handelSetupStep.");
            }
        }
    }
    public void ActivateElements()
    {
        if (elements == null || elements.Length == 0)
        {
            Debug.LogWarning("No elements assigned to handelSetupStep.");
            return;
        }

        foreach (var element in elements)
        {
            if (element != null)
            {
                element.SetActive(true);
            }
            else
            {
                Debug.LogWarning("One of the elements is null in handelSetupStep.");
            }
        }
    }
    public void DeactivateElements()
    {
        if (elements == null || elements.Length == 0)
        {
            Debug.LogWarning("No elements assigned to handelSetupStep.");
            return;
        }

        foreach (var element in elements)
        {
            if (element != null)
            {
                element.SetActive(false);
            }
            else
            {
                Debug.LogWarning("One of the elements is null in handelSetupStep.");
            }
        }
    }
    public void EnableUI()
    {
        if (UI != null)
        {
            UI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("UI GameObject is not assigned in handelSetupStep.");
        }
    }
    public void DisableUI()
    {
        if (UI != null)
        {
            UI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("UI GameObject is not assigned in handelSetupStep.");
        }
    }
}
