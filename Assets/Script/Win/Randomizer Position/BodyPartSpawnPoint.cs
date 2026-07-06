using UnityEngine;

public class BodyPartSpawnPoint : MonoBehaviour
{
    [Header("Room")]
    public RoomType roomType;

    [Header("Light")]
    [Tooltip("Luce figlia da accendere solo se qui spawna un pezzo del corpo.")]
    [SerializeField] private GameObject lightObject;

    public Transform GetSpawnTransform()
    {
        return transform;
    }

    public void SetLightActive(bool active)
    {
        if (lightObject != null)
            lightObject.SetActive(active);
    }
}