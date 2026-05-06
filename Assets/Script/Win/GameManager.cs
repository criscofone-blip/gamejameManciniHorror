using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int enemyCount = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void IncreaseDifficulty()
    {
        enemyCount++;
    }
}