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

    // 🔧 AGGIORNATO
    public string GetInteractionText(PlayerItemHolder itemHolder)
    {
        return isOn ? turnOffText : turnOnText;
    }

    // 🔧 AGGIORNATO
    public void Interact(PlayerItemHolder itemHolder)
    {
        isOn = !isOn;

        if (targetLight != null)
            targetLight.enabled = isOn;
    }
}