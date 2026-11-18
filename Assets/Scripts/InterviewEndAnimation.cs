
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class InterviewEndAnimation : MonoBehaviour
{
    [Header("動畫設定")]
    [SerializeField] private float duration = 0.7f;
    [SerializeField] private float moveDistance = 90f;

    private RectTransform panelRect;
    private CanvasGroup canvasGroup;
    private Vector2 targetPosition;
    private Coroutine animationCoroutine;

    void Awake()
    {
        panelRect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // 記住面板最後停留的位置
        targetPosition = panelRect.anchoredPosition;
    }

    void OnEnable()
    {
        // 每次顯示面板，都重新播放動畫
        canvasGroup.alpha = 0f;

        panelRect.anchoredPosition =
            targetPosition + Vector2.down * moveDistance;

        animationCoroutine =
            StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        float elapsed = 0f;

        Vector2 startPosition =
            targetPosition + Vector2.down * moveDistance;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            // 先快後慢的緩動效果
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            panelRect.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    targetPosition,
                    eased
                );

            // 整個面板逐漸淡入
            canvasGroup.alpha = eased;

            yield return null;
        }

        // 確保最後位置與透明度精準
        panelRect.anchoredPosition = targetPosition;
        canvasGroup.alpha = 1f;

        animationCoroutine = null;
    }

    void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }
}
