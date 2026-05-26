using UnityEngine;
using UnityEngine.InputSystem;

public enum Level
{
    One, Two,   Three, Four, Five,
    Six, Seven, Eight, Nine, Ten
}

public enum Mode
{
    Normal,
    Fever,
    SuperFever
}

public class TidyMommy : MonoBehaviour
{
    private const int INITIAL_BLOCK_SPAWN_COUNT = 3;

    [Header("Components")]
    [SerializeField] private BlockManager blockManager;

    [field: Header("Settings")]
    [SerializeField] private float spawnInterval = 5f;
    private float spawnTimer;

    [SerializeField] private int levelUpUnit = 10;
    [SerializeField] private int levelUpCount = 0;

    [field: SerializeField] public Level CurrentLevel { get; private set; } = Level.One;
    [field: SerializeField] public Mode CurrentMode { get; private set; } = Mode.Normal;

    #region UNITY METHOD
    private void Start()
    {
        //게임 시작 시, 모든 라인에 정해둔 수량의 블록 스폰
        blockManager.SpawnRandomBlocksToAllLines(INITIAL_BLOCK_SPAWN_COUNT);
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver) return;

        UpdateSpawnTimer();
    }

    private void UpdateSpawnTimer()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            blockManager.SpawnRandomBlocksToAllLines();
        }
    }
    #endregion

    #region PRIVATE METHOD
    private void AdvanceLevel()
    {
        if (CurrentLevel == Level.Ten) return;
        CurrentLevel = (Level)((int)CurrentLevel + 1);
    }
    #endregion

    #region PUBLIC METHOD
    public void AdvanceLevelUpCount()
    {
        levelUpCount++;

        if (levelUpCount >= levelUpUnit)
        {
            AdvanceLevel();
            levelUpCount = 0;
        }
    }
    #endregion
}
