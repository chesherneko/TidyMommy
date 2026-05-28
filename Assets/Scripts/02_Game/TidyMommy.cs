using UnityEngine;

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

public class TidyMommy : MonoSingleton<TidyMommy>
{
    private const int INITIAL_BLOCK_SPAWN_COUNT = 3;

    [Header("Settings")]
    [SerializeField] private float spawnInterval = 5f;

    [SerializeField] private int levelUpUnit = 10;

    [SerializeField] private float bombSpawnUnit = 10;
    [SerializeField] private float bombGaugeDecaySpeed = 0.25f;

    [SerializeField] private float feverUnit = 5f;
    [SerializeField] private float feverDuration = 5f;
    [SerializeField] private float feverGaugeDecaySpeed = 0.25f;

    [field: Header("Status")]
    [SerializeField] private float spawnTimer;
    [SerializeField] private float spawnSpeed = 1f;

    [SerializeField] private int levelUpCount = 0;

    [SerializeField] private float bombSpawnGauge = 0f;

    [SerializeField] private float feverGauge = 0f;
    [SerializeField] private float feverTimer = 0f;

    [field: SerializeField] public Level CurrentLevel { get; private set; } = Level.One;
    [field: SerializeField] public Mode CurrentMode { get; private set; } = Mode.Normal;

    //Property
    public BlockManager BlockManager => BlockManager.Instance;
    public GameVisual Visual => GameVisual.Instance;

    #region UNITY METHOD
    private void Start()
    {
        //게임 시작 시, 모든 라인에 정해둔 수량의 블록 스폰
        BlockManager.SpawnRandomBlocksToAllLines(INITIAL_BLOCK_SPAWN_COUNT);
    }

    private void Update()
    {
        if (GameManager.Instance.IsGameOver) return;

        UpdateSpawnTimer();
        UpdateFeverTimer();
        UpdateFeverGauge();

        if (CurrentMode == Mode.SuperFever) return;

        UpdateBombSpawnGauge();
    }

    private void UpdateSpawnTimer()
    {
        spawnTimer += Time.deltaTime * spawnSpeed;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            BlockManager.SpawnRandomBlocksToAllLines();
        }
    }

    private void UpdateBombSpawnGauge() 
        => UpdateGauge(ref bombSpawnGauge, bombGaugeDecaySpeed);

    private void UpdateFeverGauge() 
        => UpdateGauge(ref feverGauge, feverGaugeDecaySpeed);

    private void UpdateFeverTimer()
    {
        if (CurrentMode == Mode.Normal) return;

        feverTimer += Time.deltaTime;

        if (feverTimer >= feverDuration)
        {
            CurrentMode = Mode.Normal;
            ApplyMode();

            feverTimer = 0f;
        }
    }
    #endregion

    #region PRIVATE METHOD
    private void AdvanceLevel()
    {
        if (CurrentLevel == Level.Ten) return;
        CurrentLevel = (Level)((int)CurrentLevel + 1);
    }

    private void AdvanceLevelUpCount()
    {
        levelUpCount++;

        if (levelUpCount >= levelUpUnit)
        {
            AdvanceLevel();
            levelUpCount = 0;
        }
    }

    private void IncreaseBombSpawnGauge()
    {
        bombSpawnGauge += 1f;

        if (bombSpawnGauge >= bombSpawnUnit)
        {
            if (BlockManager.TrySpawnBombBlock()) 
                bombSpawnGauge = 0f;
        }
    }

    private void IncreaseFeverGauge()
    {
        feverGauge += 1f;

        if (feverGauge >= feverUnit)
        {
            CurrentMode = (Mode)((int)CurrentMode + 1);
            ApplyMode();

            feverGauge = 0f;
            feverTimer = 0f;
        }
    }

    private void ApplyMode()
    {
        spawnSpeed = CurrentMode switch
        {
            Mode.Fever => 1.5f,
            Mode.SuperFever => 2f,
            _ => 1f
        };

        if (CurrentMode == Mode.SuperFever)
            SuperFever();

        Visual.UpdateFever(CurrentMode);
        SoundManager.Instance.SetBGMPitch(CurrentMode);
    }

    private void SuperFever()
    {
        int removeCount = BlockBucket.MAX_BLOCKS_PER_BUCKET;
        BlockManager.RemoveBlocksFromAllLines(removeCount);

        BlockManager.SpawnBlocksToAllLines(BlockType.Red, 2);
        BlockManager.SpawnBlocksToAllLines(BlockType.Blue, 2);
        BlockManager.SpawnBlocksToAllLines(BlockType.Red, 2);
    }

    private void UpdateGauge(ref float gauge, float speed)
    {
        if (gauge == 0f) return;

        float decreaseAmount = Time.deltaTime * speed;
        gauge = Mathf.Max(0, gauge - decreaseAmount);
    }
    #endregion

    #region PUBLIC METHOD
    public void OnBlockMatched()
    {
        spawnTimer = 0f; //매치 될 때마다 타이머 초기화

        if (CurrentMode == Mode.SuperFever) return;

        AdvanceLevelUpCount();
        IncreaseBombSpawnGauge();
        IncreaseFeverGauge();
    }
    #endregion
}
