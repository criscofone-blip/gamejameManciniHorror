using UnityEngine;

public class LightSwitch : MonoBehaviour, IInteractable
{
    [Header("Light")]
    [SerializeField] private Light targetLight;
    [SerializeField] private bool startOn = false;

    [Header("Prompt")]
    [SerializeField] private string turnOnText = "Premi E per accendere la luce";
    [SerializeField] private string turnOffText = "Premi E per spegnere la luce";

    private bool isOn;

    private void Start()
    {
        isOn = startOn;

        if (targetLight != null)
            targetLight.enabled = isOn;
    }

    public string GetInteractionText()
    {
        return isOn ? turnOffText : turnOnText;
    }

    public void Interact()
    {
        isOn = !isOn;

        if (targetLight != null)
            targetLight.enabled = isOn;
    }
}