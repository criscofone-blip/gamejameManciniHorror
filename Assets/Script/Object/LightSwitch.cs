using UnityEngine;

public class LightSwitch : MonoBehaviour, IInteractable
{
    [SerializeField] private Light targetLight;

    private bool isOn = false;

    public void Interact()
    {
        isOn = !isOn;
        targetLight.enabled = isOn;
    }
}