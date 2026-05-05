using UnityEngine;

public class BodyPartSpawnPoint : MonoBehaviour
{
    [Header("Room")]
    public RoomType roomType;

    public Transform GetSpawnTransform()
    {
        return transform;
    }
}