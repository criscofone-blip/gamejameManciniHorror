using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private TextMeshProUGUI promptText;

    private bool isVisible;
    private string currentText;

    private void Awake()
    {
        Hide();
    }

    public void Show(string text)
    {
        if (isVisible && currentText == text)
            return;

        currentText = text;
        promptText.text = text;
        container.SetActive(true);
        isVisible = true;
    }

    public void Hide()
    {
        if (!isVisible && !container.activeSelf)
            return;

        currentText = string.Empty;
        container.SetActive(false);
        isVisible = false;
    }
}