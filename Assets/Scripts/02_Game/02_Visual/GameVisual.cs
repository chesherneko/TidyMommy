using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameVisual : MonoSingleton<GameVisual>
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI comboTMP;
    [SerializeField] private TextMeshProUGUI scoreTMP;

    [SerializeField] private Image timerGauge;
    [SerializeField] private TextMeshProUGUI timerTMP;

    [SerializeField] private CanvasGroup gameOverCanvas;

    [SerializeField] private BackgroundScroll bgScroll;
    [SerializeField] private SpriteRenderer[] bgRenderers;

    [SerializeField] private FeverFX feverFx;

    [Header("Settings")]
    [SerializeField] private float timerUnit;
    [SerializeField] private float gameOverDuration = 1f;

    [SerializeField] private Sprite[] bgSprites;

    #region UNITY METHOD
    private void Awake()
    {
        timerUnit = 1f / GameManager.Instance.TimeLimit;
    }
    #endregion

    #region PRIVATE METHOD
    private IEnumerator DisplayGameOverCoroutine()
    {
        float time = 0f;

        while (true)
        {
            if (time >= gameOverDuration) break;

            time += Time.deltaTime;

            float progress = time / gameOverDuration;
            float alpha = Mathf.Lerp(0, 1, progress);

            gameOverCanvas.alpha = alpha;

            yield return null;
        }
    }

    private void UpdateBackground(Mode mode)
    {
        Sprite sprite = bgSprites[(int)mode];

        for (int i = 0; i < bgRenderers.Length; i++)
            bgRenderers[i].sprite = sprite;

        float scrollSpeed = Mathf.Max(1f, (int)mode * 4f);
        bgScroll.ScrollSpeed = scrollSpeed;
    }
    #endregion

    #region PUBLIC METHOD
    public void UpdateCombo(int count)
        => comboTMP.text = count.ToString();

    public void UpdateScore(int amount)
        => scoreTMP.text = amount.ToCommaString();

    public void UpdateTimer(float time)
    {
        timerGauge.fillAmount = time * timerUnit;
        timerTMP.text = ((int)time).ToString();
    }

    public void UpdateFever(Mode mode)
    {
        UpdateBackground(mode);
        feverFx.PlayFX(mode);
    }

    public void DisplayGameOver()
    {
        gameOverCanvas.gameObject.SetActive(true);
        StartCoroutine(DisplayGameOverCoroutine());
    }
    #endregion
}
