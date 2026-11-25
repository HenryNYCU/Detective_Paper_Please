
using System.Collections;
using TMPro;
using UnityEngine;

public class InvestigationTutorial : MonoBehaviour
{
    [Header("教學介面")]
    public GameObject tutorialPanel;
    public TMP_Text tutorialText;
    public TMP_Text tutorialButtonText;
    public RectTransform tutorialArrow;

    [Header("原本的指引目標")]
    public RectTransform documentsHotspot;
    public TMP_Text transcriptText;
    public RectTransform notebookPanel;
    public Canvas uiCanvas;

    [Header("完整調查教學新增目標")]
    public ClueNotebook clueNotebook;
    public RectTransform openNotebookButton;
    public TMP_Text boardClueText;
    public RectTransform checkRelationButton;

    [Header("測試設定")]
    public bool replayEveryPlayForTesting = true;

    private const string SaveKey =
        "InvestigationTutorial_Completed_V2";

    private enum Stage
    {
        None,
        OpenDocuments,
        CollectNight,
        CollectArgument,
        CollectHusband,
        OpenBoard,
        SelectNight,
        SelectArgument,
        CheckRelation,
        Completed,
        Transition,
        Finished
    }

    private Stage currentStage = Stage.None;
    private bool startedThisSession = false;

    private bool boardNightSelected = false;
    private bool boardArgumentSelected = false;

    private RectTransform panelRect;
    private CanvasGroup panelGroup;
    private Vector2 panelOriginalPosition;
    private Vector2 arrowBasePosition;
    private Coroutine transitionCoroutine;

    void Awake()
    {
        if (tutorialPanel == null ||
            tutorialArrow == null)
        {
            Debug.LogError("尚未連接教學介面");
            enabled = false;
            return;
        }

        panelRect =
            tutorialPanel.GetComponent<RectTransform>();

        panelGroup =
            tutorialPanel.GetComponent<CanvasGroup>();

        if (panelGroup == null)
        {
            panelGroup =
                tutorialPanel.AddComponent<CanvasGroup>();
        }

        panelOriginalPosition =
            panelRect.anchoredPosition;

        tutorialPanel.SetActive(false);
        tutorialArrow.gameObject.SetActive(false);
    }

    // 訪談結束動畫播放完畢後呼叫
    public void BeginTutorial()
    {
        if (startedThisSession)
            return;

        startedThisSession = true;

        if (!replayEveryPlayForTesting &&
            PlayerPrefs.GetInt(SaveKey, 0) == 1)
        {
            return;
        }

        if (tutorialText == null ||
            tutorialButtonText == null ||
            uiCanvas == null ||
            clueNotebook == null)
        {
            Debug.LogError("教學 Inspector 欄位尚未連接完整");
            return;
        }

        ShowStage(Stage.OpenDocuments);
    }

    // 玩家打開左下角文件堆
    public void OnDocumentsOpened()
    {
        if (currentStage == Stage.OpenDocuments)
        {
            GoToNext(Stage.CollectNight);
        }
    }

    // 玩家關閉訪談紀錄
    public void OnDocumentsClosed()
    {
        if (currentStage == Stage.CollectNight ||
            currentStage == Stage.CollectArgument ||
            currentStage == Stage.CollectHusband ||
            currentStage == Stage.OpenBoard)
        {
            ShowStage(Stage.OpenDocuments);
        }
    }

    // 玩家成功收錄新線索
    public void OnClueCollected(string clueID)
    {
        if (currentStage != Stage.CollectNight &&
            currentStage != Stage.CollectArgument &&
            currentStage != Stage.CollectHusband)
        {
            return;
        }

        Stage next = GetNextMissingClueStage();

        if (next != currentStage)
        {
            GoToNext(next);
        }
    }

    // 根據已收錄線索決定下一步
    private Stage GetNextMissingClueStage()
    {
        if (!clueNotebook.HasClue("night"))
            return Stage.CollectNight;

        if (!clueNotebook.HasClue("argument"))
            return Stage.CollectArgument;

        if (!clueNotebook.HasClue("husband"))
            return Stage.CollectHusband;

        return Stage.OpenBoard;
    }

    // 玩家打開大型調查筆記
    public void OnBoardOpened()
    {
        if (currentStage == Stage.OpenBoard)
        {
            boardNightSelected = false;
            boardArgumentSelected = false;

            GoToNext(Stage.SelectNight);
        }
    }

    // 玩家返回訪談紀錄
    public void OnBoardClosed()
    {
        if (currentStage == Stage.SelectNight ||
            currentStage == Stage.SelectArgument ||
            currentStage == Stage.CheckRelation)
        {
            ShowStage(Stage.OpenBoard);
        }
    }

    // 玩家選取大型筆記的關鍵字
    public void OnBoardSelectionChanged(
        bool hasNight,
        bool hasArgument)
    {
        boardNightSelected = hasNight;
        boardArgumentSelected = hasArgument;

        if (currentStage == Stage.SelectNight &&
            hasNight)
        {
            GoToNext(
                hasArgument
                    ? Stage.CheckRelation
                    : Stage.SelectArgument
            );
        }
        else if (currentStage == Stage.SelectArgument &&
                 hasNight && hasArgument)
        {
            GoToNext(Stage.CheckRelation);
        }
    }

    // 玩家按下「查證關聯」
    public void OnRelationChecked(bool success)
    {
        if (currentStage == Stage.CheckRelation &&
            success)
        {
            GoToNext(Stage.Completed);
        }
    }

    // 略過或完成教學
    public void OnTutorialButtonClicked()
    {
        FinishTutorial();
    }

    // 切換階段，先向下淡出再顯示下一段
    private void GoToNext(Stage next)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        // 立即更新教學階段，下一次點擊可以馬上生效
        currentStage = next;

        // 箭頭立即移動到下一個目標
        MoveArrowImmediately(next);

        // 提示框仍然播放原本的淡出動畫
        transitionCoroutine =
            StartCoroutine(PlayTransition(next));
    }
    
    private void MoveArrowImmediately(Stage next)
    {
        tutorialArrow.gameObject.SetActive(true);

        switch (next)
        {
            case Stage.OpenDocuments:
                PointAboveRect(documentsHotspot, 55f);
                break;

            case Stage.CollectNight:
                PointAtLink(transcriptText, "night", 50f);
                break;

            case Stage.CollectArgument:
                PointAtLink(transcriptText, "argument", 50f);
                break;

            case Stage.CollectHusband:
                PointAtLink(transcriptText, "husband", 50f);
                break;

            case Stage.OpenBoard:
                PointAboveRect(openNotebookButton, 50f);
                break;

            case Stage.SelectNight:
                PointAtLink(boardClueText, "night", 45f);
                break;

            case Stage.SelectArgument:
                PointAtLink(boardClueText, "argument", 45f);
                break;

            case Stage.CheckRelation:
                PointAboveRect(checkRelationButton, 45f);
                break;

            case Stage.Completed:
                tutorialArrow.gameObject.SetActive(false);
                break;
        }
    }

    private IEnumerator PlayTransition(Stage next)
    {
        float elapsed = 0f;
        const float duration = 0.35f;

        Vector2 startPosition =
            panelRect.anchoredPosition;

        Vector2 endPosition =
            panelOriginalPosition +
            Vector2.down * 55f;

        float startAlpha = panelGroup.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / duration
            );

            float eased = t * t * (3f - 2f * t);

            panelRect.anchoredPosition =
                Vector2.Lerp(
                    startPosition,
                    endPosition,
                    eased
                );

            panelGroup.alpha =
                Mathf.Lerp(startAlpha, 0f, eased);

            yield return null;
        }

        tutorialPanel.SetActive(false);
        transitionCoroutine = null;

        ShowStage(next);
    }

    // 顯示指定教學階段
    private void ShowStage(Stage requestedStage)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        Stage next = requestedStage;

        // 玩家若提早收集了其他關鍵字，
        // 就不要求他重複點擊。
        if (next == Stage.CollectNight ||
            next == Stage.CollectArgument ||
            next == Stage.CollectHusband)
        {
            next = GetNextMissingClueStage();
        }

        if (next == Stage.SelectNight &&
            boardNightSelected)
        {
            next = boardArgumentSelected
                ? Stage.CheckRelation
                : Stage.SelectArgument;
        }

        if (next == Stage.SelectArgument &&
            boardNightSelected &&
            boardArgumentSelected)
        {
            next = Stage.CheckRelation;
        }

        currentStage = next;

        panelRect.anchoredPosition =
            panelOriginalPosition;

        panelGroup.alpha = 1f;

        tutorialPanel.SetActive(true);
        tutorialArrow.gameObject.SetActive(true);

        tutorialButtonText.text =
            next == Stage.Completed ? "完成" : "略過";

        switch (next)
        {
            case Stage.OpenDocuments:
                tutorialText.text =
                    "訪談紀錄已建立！\n" +
                    "請點擊左下角的文件堆。";

                PointAboveRect(documentsHotspot, 55f);
                break;

            case Stage.CollectNight:
                tutorialText.text =
                    "先找到第一條線索。\n" +
                    "點擊橘色的「失蹤那天晚上」。";

                PointAtLink(transcriptText, "night", 50f);
                break;

            case Stage.CollectArgument:
                tutorialText.text =
                    "成功記錄第一條線索！\n" +
                    "接著點擊「夫妻在爭吵」。";

                PointAtLink(
                    transcriptText, "argument", 50f);
                break;

            case Stage.CollectHusband:
                tutorialText.text =
                    "很好！再收錄最後一條線索。\n" +
                    "點擊「懷疑她的丈夫」。";

                PointAtLink(
                    transcriptText, "husband", 50f);
                break;

            case Stage.OpenBoard:
                tutorialText.text =
                    "三條線索已收錄完成！\n" +
                    "點擊「展開調查筆記」。";

                PointAboveRect(openNotebookButton, 50f);
                break;

            case Stage.SelectNight:
                tutorialText.text =
                    "現在試著組合兩條線索。\n" +
                    "先選取左側的「失蹤當晚」。";

                PointAtLink(
                    boardClueText, "night", 45f);
                break;

            case Stage.SelectArgument:
                tutorialText.text =
                    "第一條已經選好了！\n" +
                    "接著選取「夫妻爭吵」。";

                PointAtLink(
                    boardClueText, "argument", 45f);
                break;

            case Stage.CheckRelation:
                tutorialText.text =
                    "兩條線索已選取。\n" +
                    "按「查證關聯」看看調查結果。";

                PointAboveRect(
                    checkRelationButton, 45f);
                break;

            case Stage.Completed:
                tutorialText.text =
                    "成功發現新的調查方向！\n" +
                    "目前仍需要其他證詞查證。";

                tutorialArrow.gameObject.SetActive(false);
                break;
        }
    }

    private void FinishTutorial()
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        currentStage = Stage.Finished;

        tutorialPanel.SetActive(false);
        tutorialArrow.gameObject.SetActive(false);

        if (!replayEveryPlayForTesting)
        {
            PlayerPrefs.SetInt(SaveKey, 1);
            PlayerPrefs.Save();
        }

        Debug.Log("調查新手教學已完成");
    }

    // 箭頭指向指定 UI 物件上方
    private void PointAboveRect(
        RectTransform target,
        float offset)
    {
        if (target == null)
        {
            Debug.LogWarning("教學箭頭目標未連接");
            tutorialArrow.gameObject.SetActive(false);
            return;
        }

        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Vector3 topCenter =
            (corners[1] + corners[2]) / 2f;

        SetArrowPosition(topCenter, offset);
    }

    // 箭頭指向 TMP 文字中的指定 link
    private void PointAtLink(
        TMP_Text textComponent,
        string linkID,
        float offset)
    {
        if (textComponent == null)
        {
            tutorialArrow.gameObject.SetActive(false);
            return;
        }

        textComponent.ForceMeshUpdate();

        TMP_TextInfo info = textComponent.textInfo;

        for (int i = 0; i < info.linkCount; i++)
        {
            TMP_LinkInfo link = info.linkInfo[i];

            if (link.GetLinkID() != linkID)
                continue;

            int index =
                link.linkTextfirstCharacterIndex;

            if (index < 0 ||
                index >= info.characterCount)
                break;

            TMP_CharacterInfo character =
                info.characterInfo[index];

            Vector3 point = new Vector3(
                (character.bottomLeft.x +
                 character.topRight.x) / 2f,
                character.topRight.y,
                0f
            );

            Vector3 worldPosition =
                textComponent.transform.TransformPoint(point);

            SetArrowPosition(worldPosition, offset);
            return;
        }

        Debug.LogWarning(
            "找不到教學關鍵字 Link ID：" + linkID
        );

        PointAboveRect(
            textComponent.rectTransform,
            offset
        );
    }

    // 將世界座標換成 Canvas 座標
    private void SetArrowPosition(
        Vector3 worldPosition,
        float offset)
    {
        Camera camera = null;

        if (uiCanvas.renderMode !=
            RenderMode.ScreenSpaceOverlay)
        {
            camera = uiCanvas.worldCamera;
        }

        Vector2 screenPoint =
            RectTransformUtility.WorldToScreenPoint(
                camera, worldPosition
            );

        RectTransform canvasRect =
            uiCanvas.GetComponent<RectTransform>();

        Vector2 localPoint;

        if (RectTransformUtility
            .ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPoint,
                camera,
                out localPoint))
        {
            arrowBasePosition =
                localPoint + Vector2.up * offset;

            tutorialArrow.anchoredPosition =
                arrowBasePosition;
        }
    }

    // 箭頭持續輕微上下浮動
    void Update()
    {
        if (tutorialArrow == null ||
            !tutorialArrow.gameObject.activeSelf)
            return;

        float movement =
            Mathf.Sin(Time.unscaledTime * 3f) * 4f;

        tutorialArrow.anchoredPosition =
            arrowBasePosition +
            Vector2.up * movement;
    }
}
