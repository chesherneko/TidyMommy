using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoSingleton<GameManager>
{
    private GameVisual visual;

    [field: Header("Settings")]
    [SerializeField] private float comboClearLimit = 5f;

    [SerializeField] private int scoreUnit = 200;
    [SerializeField] private int scoreBonusInterval = 10;
    [SerializeField] private float scoreBonusRate = 0.5f;

    [field: SerializeField] public float TimeLimit { get; private set; } = 60f;

    [field: Header("Status")]
    [SerializeField] private int combo;
    [SerializeField] private int score;

    [SerializeField] private float comboClearTimer = 0f;

    [field: SerializeField] public float RemainTime { get; private set; }

    [field: SerializeField] public bool IsGameOver { get; private set; }

    #region UNITY METHOD
    protected override void Awake()
    {
        base.Awake();

        visual = FindFirstObjectByType<GameVisual>();

        combo = 0;
        score = 0;

        comboClearTimer = 0f;

        RemainTime = TimeLimit;

        IsGameOver = false;
    }

    private void Update()
    {
        if (IsGameOver)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
                SceneManager.LoadSceneAsync("Tidy_Mommy");

            return;
        }

        UpdateTimer();
        UpdateComboClearTime();
    }

    private void UpdateTimer()
    {
        RemainTime -= Time.deltaTime;
        visual.UpdateTimer(RemainTime);

        if (RemainTime <= 0) GameOver();
    }

    private void UpdateComboClearTime()
    {
        comboClearTimer += Time.deltaTime;

        if (comboClearTimer >= comboClearLimit)
        {
            combo = 0;
            comboClearTimer = 0f;

            visual.UpdateCombo(combo);
        }
    }
    #endregion

    #region PRIVATE METHOD
    private void AdvanceCombo()
    {
        combo++;
        comboClearTimer = 0f;

        visual.UpdateCombo(combo);
    }
    #endregion

    #region PUBLIC METHOD
    public void GameOver()
    {
        IsGameOver = true;
        visual.DisplayGameOver();
    }

    public void IncreaseScore(int matchCount = 1)
    {
        for (int i = 0; i < matchCount; i++)
        {
            AdvanceCombo();
            RemainTime += 1f; //점수가 오를 때 마다 남은 시간 증가

            int bonusStep = combo / scoreBonusInterval;
            float scoreMultiplier = 1f + bonusStep * scoreBonusRate;
            score += Mathf.FloorToInt(scoreUnit * scoreMultiplier);
            
            visual.UpdateScore(score);
        }
    }
    #endregion
}
