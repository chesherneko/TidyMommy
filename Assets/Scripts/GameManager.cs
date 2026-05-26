using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    private const int SCORE_UNIT = 200;
    private const int SCORE_BONUS_INTERVAL = 10;
    private const float SCORE_BONUS_RATE = 0.5f;

    [Header("Settings")]
    [SerializeField] private float comboClearLimit = 5f;
    private float comboClearTimer = 0f;

    [field: Header("DO NOT EDIT AT INSPECTOR")]
    [SerializeField] private int combo;
    [SerializeField] private int score;

    [field: SerializeField] public bool IsGameOver { get; private set; }

    #region UNITY METHOD
    private void Update()
    {
        if (IsGameOver) return;

        UpdateComboClearTime();
    }

    private void UpdateComboClearTime()
    {
        comboClearTimer += Time.deltaTime;

        if (comboClearTimer >= comboClearLimit)
        {
            combo = 0;
            comboClearTimer = 0f;
        }
    }
    #endregion

    #region PRIVATE METHOD
    private void AdvanceCombo()
    {
        combo++;
        comboClearTimer = 0f;
    }
    #endregion

    #region PUBLIC METHOD
    public void GameOver()
    {
        IsGameOver = true;
    }

    public void IncreaseScore(int matchCount = 1)
    {
        for (int i = 0; i < matchCount; i++)
        {
            AdvanceCombo();

            int bonusStep = combo / SCORE_BONUS_INTERVAL;
            float scoreMultiplier = 1f + bonusStep * SCORE_BONUS_RATE;
            score += Mathf.FloorToInt(SCORE_UNIT * scoreMultiplier);
        }
    }
    #endregion
}
