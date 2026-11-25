
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InvestigationBoardClues :
    MonoBehaviour, IPointerClickHandler
{
    [Header("線索來源")]
    public ClueNotebook clueNotebook;

    [Header("右側查證介面")]
    public TMP_Text selectedAText;
    public TMP_Text selectedBText;
    public TMP_Text resultText;

    private TMP_Text boardClueText;

    [Header("新手教學")]
    public InvestigationTutorial investigationTutorial;

    private readonly string[] clueIDs =
    {
        "night", "argument", "husband"
    };

    private readonly Dictionary<string, string> clueNames =
        new Dictionary<string, string>()
        {
            { "night", "失蹤當晚" },
            { "argument", "夫妻爭吵" },
            { "husband", "懷疑丈夫" }
        };

    private readonly List<string> selected =
        new List<string>();

    void Awake()
    {
        boardClueText = GetComponent<TMP_Text>();
    }

    // 每次開啟大型筆記時更新
    public void RefreshFromNotebook()
    {
        selected.Clear();
        RefreshClueList();
        RefreshSelection();

        if (InvestigationDataManager.Instance != null &&
        InvestigationDataManager.Instance.HasLead("argument_time"))
        {
            resultText.text =
                "【已記錄調查方向】\n" +
                "爭吵發生時間（待查證）\n\n" +
                "下一步：尋找其他居民的證詞。";
        }
        else
        {
            resultText.text =
                "請選取兩條線索，\n再按「查證關聯」。";
        }
    }

    void RefreshClueList()
    {
        StringBuilder sb = new StringBuilder();
        bool hasAnyClue = false;

        foreach (string id in clueIDs)
        {
            if (clueNotebook == null ||
                !clueNotebook.HasClue(id))
                continue;

            hasAnyClue = true;

            bool isSelected = selected.Contains(id);

            string mark = isSelected ? "■ " : "□ ";
            string color = isSelected
                ? "#765020"
                : "#A66B20";

            sb.Append(
                "<link=\"" + id + "\">" +
                "<color=" + color + "><u>" +
                mark + clueNames[id] +
                "</u></color></link>\n\n"
            );
        }

        if (!hasAnyClue)
        {
            sb.Append(
                "尚未收錄任何線索。\n" +
                "請先從訪談紀錄中蒐集關鍵字。"
            );
        }

        boardClueText.text = sb.ToString();
    }

    // 點擊左側關鍵字
    public void OnPointerClick(PointerEventData eventData)
    {
        int index =
            TMP_TextUtilities.FindIntersectingLink(
                boardClueText,
                eventData.position,
                eventData.pressEventCamera
            );

        if (index == -1)
            return;

        string id =
            boardClueText.textInfo
                .linkInfo[index]
                .GetLinkID();

        if (selected.Contains(id))
        {
            selected.Remove(id);
        }
        else
        {
            // 最多選兩條
            if (selected.Count >= 2)
                selected.RemoveAt(0);

            selected.Add(id);
        }

        RefreshClueList();
        RefreshSelection();
        investigationTutorial?.OnBoardSelectionChanged(
            selected.Contains("night"),
            selected.Contains("argument")
        );
    }

    void RefreshSelection()
    {
        selectedAText.text = selected.Count >= 1
            ? "第一條：" + clueNames[selected[0]]
            : "第一條：尚未選取";

        selectedBText.text = selected.Count >= 2
            ? "第二條：" + clueNames[selected[1]]
            : "第二條：尚未選取";
    }

    // 由查證按鈕呼叫
    public void CheckRelation()
    {
        if (selected.Count < 2)
        {
            resultText.text = "請先選取兩條線索。";
            return;
        }

        string a = selected[0];
        string b = selected[1];

        bool foundLead =
            (a == "night" && b == "argument") ||
            (a == "argument" && b == "night");

        if (foundLead)
        {
            // 將調查方向記錄到共用資料管理器
            if (InvestigationDataManager.Instance != null)
            {
                InvestigationDataManager.Instance.UnlockLead("argument_time");
            }

            resultText.text =
                "【新增調查方向】\n" +
                "爭吵發生時間待查證。\n\n" +
                "需要其他居民的獨立證詞。";
        }
        else
        {
            resultText.text =
                "【目前無法建立關聯】\n" +
                "現有資訊不足以支持推論。\n" +
                "可以在取得新證詞後再次查證。";
        }

        // 通知新手教學：玩家已完成關鍵字查證
        investigationTutorial?.OnRelationChecked(foundLead);
        
    }
}
