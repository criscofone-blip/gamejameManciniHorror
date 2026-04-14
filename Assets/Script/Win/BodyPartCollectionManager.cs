using System;
using System.Collections.Generic;
using UnityEngine;

public class BodyPartCollectionManager : MonoBehaviour
{
    public static BodyPartCollectionManager Instance { get; private set; }

    public event Action<BodyPartType> OnBodyPartCollected;
    public event Action OnAllBodyPartsCollected;

    private readonly HashSet<BodyPartType> collectedParts = new();

    public int CollectedCount => collectedParts.Count;
    public int TotalParts => Enum.GetValues(typeof(BodyPartType)).Length;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool HasCollected(BodyPartType partType)
    {
        return collectedParts.Contains(partType);
    }

    public bool TryCollectPart(BodyPartType partType)
    {
        if (collectedParts.Contains(partType))
            return false;

        collectedParts.Add(partType);
        OnBodyPartCollected?.Invoke(partType);

        if (collectedParts.Count >= TotalParts)
            OnAllBodyPartsCollected?.Invoke();

        return true;
    }
}