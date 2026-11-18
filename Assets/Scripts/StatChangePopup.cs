
using System.Collections;
using UnityEngine;
using TMPro;

public class StatChangePopup : MonoBehaviour
{
    [Header("浮動數字")]
    public TMP_Text reputationDeltaText;
    public TMP_Text justiceDeltaText;

    [Header("動畫設定")]
    public float riseDuration = 0.25f;
    public float holdDuration = 0.45f;
    public float fadeDuration = 0.35f;
    public float riseDistance = 35f;

    private Vector2 repStartPosition;
    private Vector2 justiceStartPosition;

    private Coroutine repAnimation;
    private Coroutine justiceAnimation;

    private readonly Color increaseColor =
        new Color32(135, 226, 167, 255);

    private readonly Color decreaseColor =
        new Color32(255, 133, 133, 255);

    void Awake()
    {
        repStartPosition =
            reputationDeltaText.rectTransform.anchoredPosition;

        justiceStartPosition =
            justiceDeltaText.rectTransform.anchoredPosition;

        HideText(reputationDeltaText);
        HideText(justiceDeltaText);
    }

    // 由 DialogueManager 呼叫
    public void Show(int reputationChange, int justiceChange)
    {
        if (repAnimation != null)
            StopCoroutine(repAnimation);

        if (justiceAnimation != null)
            StopCoroutine(justiceAnimation);

        HideText(reputationDeltaText);
        HideText(justiceDeltaText);

        if (reputationChange != 0)
        {
            repAnimation = StartCoroutine(
                AnimateValue(
                    reputationDeltaText,
                    reputationChange,
                    repStartPosition
                )
            );
        }

        if (justiceChange != 0)
        {
            justiceAnimation = StartCoroutine(
                AnimateValue(
                    justiceDeltaText,
                    justiceChange,
                    justiceStartPosition
                )
            );
        }
    }

    IEnumerator AnimateValue(
        TMP_Text textObject,
        int amount,
        Vector2 targetPosition)
    {
        textObject.text =
            amount > 0 ? "+" + amount : amount.ToString();

        Color baseColor =
            amount > 0 ? increaseColor : decreaseColor;

        RectTransform rect = textObject.rectTransform;

        Vector2 startPosition =
            targetPosition + Vector2.down * riseDistance;

        rect.anchoredPosition = startPosition;
        SetAlpha(textObject, baseColor, 0f);

        // 第一段：上升並淡入
        float elapsed = 0f;

        while (elapsed < riseDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, riseDuration)
            );

            float eased = 1f - Mathf.Pow(1f - t, 3f);

            rect.anchoredPosition =
                Vector2.Lerp(startPosition, targetPosition, eased);

            SetAlpha(textObject, baseColor, eased);

            yield return null;
        }

        rect.anchoredPosition = targetPosition;
        SetAlpha(textObject, baseColor, 1f);

        // 第二段：稍微停留
        yield return new WaitForSecondsRealtime(holdDuration);

        // 第三段：逐漸淡出
        elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, fadeDuration)
            );

            SetAlpha(textObject, baseColor, 1f - t);

            yield return null;
        }

        HideText(textObject);
        rect.anchoredPosition = targetPosition;
    }

    void SetAlpha(TMP_Text target, Color color, float alpha)
    {
        color.a = alpha;
        target.color = color;
    }

    void HideText(TMP_Text target)
    {
        Color color = target.color;
        color.a = 0f;
        target.color = color;
        target.text = "";
    }
}
