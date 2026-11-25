
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CanvasGroup))]
public class InterviewEndAnimation : MonoBehaviour
{
    [Header("辦公室互動")]
    public OfficeInteraction officeInteraction;

    [Header("進場動畫")]
    [SerializeField] private float duration = 1.2f;
    [SerializeField] private float moveDistance = 65f;

    [Header("停留設定")]
    [SerializeField] private float displayDuration = 2f;

    [Header("退場動畫")]
    [SerializeField] private float exitDuration = 0.9f;
    [SerializeField] private float exitDistance = 65f;

    [Header("新手教學")]
    public InvestigationTutorial investigationTutorial;

    private RectTransform panelRect;
    private CanvasGroup canvasGroup;
    private Vector2 targetPosition;
    private Coroutine animationCoroutine;

    void Awake()
    {
        panelRect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // 記住提示框最後停留的位置
        targetPosition = panelRect.anchoredPosition;
    }

    void OnEnable()
    {
        panelRect.anchoredPosition =
            targetPosition + Vector2.down * moveDistance;

        canvasGroup.alpha = 0f;

        animationCoroutine =
            StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        // 第一階段：從下方浮現
        float elapsed = 0f;

        Vector2 startPosition =
            targetPosition + Vector2.down * moveDistance;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, duration)
            );

            float eased = 1f - Mathf.Pow(1f - t, 3f);

            panelRect.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    targetPosition,
                    eased
                );

            canvasGroup.alpha = eased;

            yield return null;
        }

        panelRect.anchoredPosition = targetPosition;
        canvasGroup.alpha = 1f;

        // 第二階段：最多停留 2 秒
        // Enter 或滑鼠左鍵可以提前結束等待
        float waitElapsed = 0f;

        while (waitElapsed < displayDuration)
        {
            bool enterPressed =
                Keyboard.current != null &&
                (Keyboard.current.enterKey.wasPressedThisFrame ||
                 Keyboard.current.numpadEnterKey.wasPressedThisFrame);

            bool mouseClicked =
                Mouse.current != null &&
                Mouse.current.leftButton.wasPressedThisFrame;

            if (enterPressed || mouseClicked)
            {
                break;
            }

            waitElapsed += Time.deltaTime;

            yield return null;
        }

        // 第三階段：向上飄動並淡出
        elapsed = 0f;

        Vector2 exitPosition =
            targetPosition + Vector2.up * exitDistance;

        while (elapsed < exitDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / Mathf.Max(0.01f, exitDuration)
            );

            // 柔和的緩動曲線
            float eased = t * t * (3f - 2f * t);

            panelRect.anchoredPosition =
                Vector2.Lerp(
                    targetPosition,
                    exitPosition,
                    eased
                );

            canvasGroup.alpha = 1f - eased;

            yield return null;
        }

        canvasGroup.alpha = 0f;

        // 動畫完全結束後，才開放文件堆
        if (officeInteraction != null)
        {
            officeInteraction.UnlockDocuments();
        }
        else
        {
            Debug.LogWarning(
                "尚未連接 OfficeInteractionManager"
            );
        }
        
        // 動畫結束後，啟動調查新手教學
        if (investigationTutorial != null)
        {
            investigationTutorial.BeginTutorial();
        }
        animationCoroutine = null;

        // 隱藏訪談完成提示
        gameObject.SetActive(false);
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
