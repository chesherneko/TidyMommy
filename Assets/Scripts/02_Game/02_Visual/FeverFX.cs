using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeverFX : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image flashImage;
    [SerializeField] private TextMeshProUGUI feverTMP;
    [SerializeField] private TextMeshProUGUI superFeverTMP;

    [Header("Settings")]
    [SerializeField] private float flashMaxAlpha = 0.7f;

    private Coroutine coroutine;

    public void PlayFX(Mode mode)
    {
        //Normal모드는 FX 없음
        if (mode == Mode.Normal) return;

        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        TextMeshProUGUI tmp = mode switch
        {
            Mode.Fever => feverTMP,
            Mode.SuperFever => superFeverTMP,
            _ => throw new System.ArgumentException()
        };

        coroutine = StartCoroutine(PlayFXCoroutine(tmp));
    }

    private IEnumerator PlayFXCoroutine(TextMeshProUGUI tmp)
    {
        canvas.enabled = true;

        tmp.enabled = true;
        flashImage.enabled = true;

        ResetAlpha(tmp);
        ResetAlpha(flashImage);

        tmp.transform.localScale = Vector3.one * 0.2f;
        tmp.rectTransform.anchoredPosition = Vector2.zero;

        //화면 번쩍임
        yield return FadeGraphic(flashImage, 0f, flashMaxAlpha, 0.05f);

        float elapsed = 0f;

        float flashOutDuration = 0.15f;
        float textDelay = 0.025f;
        float textFadeInDuration = 0.1f;
        float textScaleUpDuration = 0.15f;

        Vector3 startScale = Vector3.one * 0.25f;
        Vector3 endScale = Vector3.one * 1.15f;

        bool isPlaySE = false;

        while (true)
        {
            if (elapsed >= flashOutDuration) break;

            elapsed += Time.deltaTime;

            //화면 번쩍임 제거
            float flashProgress = elapsed / flashOutDuration;
            float flashAlpha = Mathf.Lerp(flashMaxAlpha, 0f, flashProgress);
            SetAlpha(flashImage, flashAlpha);

            float textElapsed = elapsed - textDelay;

            if (textElapsed >= 0f)
            {
                if (isPlaySE == false)
                {
                    isPlaySE = true;
                    SoundManager.Instance.PlayFeverSFX();
                }

                float textFadeProgress = textElapsed / textFadeInDuration;
                float textFadeAlpha = Mathf.Lerp(0f, 1f, textFadeProgress);
                SetAlpha(tmp, textFadeAlpha);

                float textScaleProgress = textElapsed / textScaleUpDuration;
                float textEased = EasingFunction.EaseOutBack(0, 1, textScaleProgress);
                tmp.transform.localScale = Vector3.LerpUnclamped(startScale, endScale, textEased);
            }

            yield return null;
        }

        yield return ScaleTransform(tmp.transform, endScale, Vector3.one, 0.1f);

        yield return MoveYAndFadeOut(tmp, 0f, 40f, 0.25f, 0.2f);

        flashImage.enabled = false;
        tmp.enabled = false;

        canvas.enabled = false;

        coroutine = null;
    }

    private void ResetAlpha(Graphic graphic)
    {
        Color color = graphic.color;
        color.a = 0f;
        graphic.color = color;
    }

    private void SetAlpha(Graphic graphic, float alpha)
    {
        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    private IEnumerator FadeGraphic(Graphic graphic, float from, float to, float duration)
    {
        float time = 0f;

        while (true)
        {
            if (time >= duration) break;

            time += Time.deltaTime;

            float progress = time / duration;
            float alpha = Mathf.Lerp(from, to, progress);

            SetAlpha(graphic, alpha);

            yield return null;
        }

        SetAlpha(graphic, to);
    }

    private IEnumerator ScaleTransform(Transform trans, Vector3 from, Vector3 to, float duration)
    {
        float time = 0f;

        while (true)
        {
            if (time >= duration) break;

            time += Time.deltaTime;

            float progress = time / duration;
            float eased = EasingFunction.EaseOutQuad(0, 1, progress);

            trans.localScale = Vector3.LerpUnclamped(from, to, eased);

            yield return null;
        }

        trans.localScale = to;
    }

    private IEnumerator MoveYAndFadeOut(TextMeshProUGUI tmp, float from, float to, 
        float moveDuration, float fadeDuration)
    {
        float time = 0f;
        Vector2 rectPos = tmp.rectTransform.anchoredPosition;

        while (true)
        {
            if (time >= moveDuration) break;

            time += Time.deltaTime;

            float moveProgress = time / moveDuration;
            float moveEased = EasingFunction.EaseOutQuad(0, 1, moveProgress);
            float yPos = Mathf.Lerp(from, to, moveEased);
            tmp.rectTransform.anchoredPosition = new Vector2(rectPos.x, yPos);

            float fadeProgress = time / fadeDuration;
            float fadeAlpha = Mathf.Lerp(1f, 0f, fadeProgress);
            SetAlpha(tmp, fadeAlpha);

            yield return null;
        }

        tmp.rectTransform.anchoredPosition = new Vector2(rectPos.x, to);
        SetAlpha(tmp, 0f);
    }
}
