using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public Transform ClosedDoor;
    public Transform OpenDoor;

    bool isDoorClosed;

    [Header("Prompt")]
    [SerializeField] private string turnOnText = "Premi E per aprire la porta";
    [SerializeField] private string turnOffText = "Premi E per chiudere la porta";

    public void Open()
    {
        gameObject.transform.position = OpenDoor.position;
        gameObject.transform.rotation = OpenDoor.rotation;
        isDoorClosed = false;
    }

    //animation

    public void Close()
    {
        gameObject.transform.position = ClosedDoor.position;
        gameObject.transform.rotation = ClosedDoor.rotation;
        isDoorClosed = true;
    }

    public void Start()
    {
        Close();
    }

    public string GetInteractionText(PlayerItemHolder itemHolder)
    {
        return isDoorClosed ? turnOffText : turnOnText;
    }


    public void Interact(PlayerItemHolder itemHolder)
    {
        if(isDoorClosed)
        {
            Open();
        }
        else
        {
            Close();
        }
    }
}
