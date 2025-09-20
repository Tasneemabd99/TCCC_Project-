using UnityEngine;

public class HideUI : MonoBehaviour
{
    [Header("UI element to hide")]
    public GameObject uiElement;

    // Call this method on button click
    public void Hide()
    {
        if (uiElement != null)
            uiElement.SetActive(false);
    }
}
