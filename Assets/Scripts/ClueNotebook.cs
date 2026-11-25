
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClueNotebook : MonoBehaviour, IPointerClickHandler
{
    [Header("筆記文字")]
    public TMP_Text notebookText;

    [Header("新手教學")]
    public InvestigationTutorial investigationTutorial;

    [Header("可收集的線索")]
    private Dictionary<string, string> clues =
        new Dictionary<string, string>()
        {
            { "argument", "夫妻爭吵" },
            { "night", "失蹤當晚" },
            { "husband", "懷疑丈夫" }
        };

    private HashSet<string> collectedClues =
        new HashSet<string>();

    private List<string> notebookEntries =
        new List<string>();

    
    void Start()
    {
        // 讀取先前已經收錄的克萊兒線索
       InvestigationDataManager data =
            InvestigationDataManager.Instance;

        if (data != null)
        {
            foreach (var clue in clues)
            {
                if (!data.HasClue(clue.Key))
                    continue;

                if (!collectedClues.Add(clue.Key))
                    continue;
  
                notebookEntries.Add(
                    "■ " + clue.Value +
                    "\n   來源：" +
                    data.GetClueSource(clue.Key) + "\n"
                );
            }
        }

        RefreshNotebook();
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        TMP_Text transcript = GetComponent<TMP_Text>();

        if (transcript == null)
            return;

        // Canvas 使用 Screen Space - Overlay 時，相機可傳 null
        Canvas canvas = GetComponentInParent<Canvas>();

        Camera uiCamera = null;

        if (canvas != null &&
            canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        // 偵測滑鼠點到哪一個 TMP 連結
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(
            transcript,
            eventData.position,
            uiCamera
        );

        if (linkIndex == -1)
            return;

        TMP_LinkInfo linkInfo =
            transcript.textInfo.linkInfo[linkIndex];

        string clueID = linkInfo.GetLinkID();

        CollectClue(clueID);
    }

    void CollectClue(string clueID)
    {
        if (!clues.ContainsKey(clueID))
            return;

        // 避免重複收錄
        if (collectedClues.Contains(clueID))
        {
            Debug.Log("線索已經記錄過：" + clues[clueID]);
            return;
        }

        collectedClues.Add(clueID);
        
        // 同步到所有居民共用的調查資料管理器
        if (InvestigationDataManager.Instance != null)
        { 
            InvestigationDataManager.Instance.AddClue(
                clueID,
                "克萊兒的證詞"
            );
        }
        string clueName = clues[clueID];

        notebookEntries.Add(
            "■ " + clueName +
            "\n   來源：克萊兒的證詞\n"
        );

        RefreshNotebook();

        // 通知新手教學：玩家成功收錄了一條線索
        investigationTutorial?.OnClueCollected(clueID);

        Debug.Log("新增調查線索：" + clueName);
    }

    // 提供大型調查筆記查詢線索是否已經收錄
    
    public bool HasClue(string clueID)
    {
        if (InvestigationDataManager.Instance != null)
        {
            return InvestigationDataManager.Instance.HasClue(clueID);
        }

        return collectedClues.Contains(clueID);
    }


    void RefreshNotebook()
    {
        if (notebookText == null)
            return;

        string content = "調查筆記\n\n";

        if (notebookEntries.Count == 0)
        {
            content +=
                "尚未收錄任何線索。\n\n" +
                "點擊訪談紀錄中的關鍵字，\n" +
                "即可加入筆記。";
        }
        else
        {
            foreach (string entry in notebookEntries)
            {
                content += entry + "\n";
            }
        }

        notebookText.text = content;
    }
}
