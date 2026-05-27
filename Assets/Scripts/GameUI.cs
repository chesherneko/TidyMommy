using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class GameUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI comboTMP;
    [SerializeField] private TextMeshProUGUI scoreTMP;

    [SerializeField] private Image timerGauge;
    [SerializeField] private TextMeshProUGUI timerTMP;

    [SerializeField] private CanvasGroup gameOverCanvas;

    [Header("Settings")]
    [SerializeField] private float gameOverDuration = 1f;

    private float timeUnit;

    private void Awake()
    {
        timeUnit = 1f / GameManager.Instance.TimeLimit;
    }

    public void UpdateCombo(int count)
        => comboTMP.text = count.ToString();

    public void UpdateScore(int amount)
        => scoreTMP.text = amount.ToCommaString();

    public void UpdateTimer(float time)
    {
        timerGauge.fillAmount = time * timeUnit;
        timerTMP.text = ((int)time).ToString();
    }

    public void DisplayGameOver()
    {
        gameOverCanvas.gameObject.SetActive(true);
        StartCoroutine(DisplayGameOverCoroutine());
    }

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
}
