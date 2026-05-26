using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Difficulty")]
    [SerializeField] private int startingEnemyCount = 0;

    public int CurrentSlot { get; private set; } = 0;
    public int EnemyCount { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnemyCount = startingEnemyCount;
    }

    public void StartNewGame(int slot)
    {
        CurrentSlot = slot;
        EnemyCount = startingEnemyCount;
        SaveManager.SaveEnemyCount(CurrentSlot, EnemyCount);
    }

    public void LoadGame(int slot)
    {
        CurrentSlot = slot;
        EnemyCount = SaveManager.LoadEnemyCount(CurrentSlot);
    }

    public void IncreaseDifficultyAndSave()
    {
        EnemyCount++;
        SaveManager.SaveEnemyCount(CurrentSlot, EnemyCount);
    }

    public void DeleteSave(int slot)
    {
        SaveManager.DeleteSave(slot);
    }
}