using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private TextMeshProUGUI promptText;

    private void Awake()
    {
        Hide();
    }

    public void Show(string text)
    {
        promptText.text = text;
        container.SetActive(true);
    }

    public void Hide()
    {
        container.SetActive(false);
    }
}