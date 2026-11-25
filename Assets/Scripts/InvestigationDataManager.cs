
using System.Collections.Generic;
using UnityEngine;

public class InvestigationDataManager : MonoBehaviour
{
    // 讓不同場景可以使用同一份調查資料
    public static InvestigationDataManager Instance
    {
        get;
        private set;
    }

    // 已收錄的線索
    private HashSet<string> collectedClues =
        new HashSet<string>();

    // 線索來源
    private Dictionary<string, string> clueSources =
        new Dictionary<string, string>();

    // 已解鎖的調查方向
    private HashSet<string> unlockedLeads =
        new HashSet<string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 切換場景時保留資料
        DontDestroyOnLoad(gameObject);
    }

    // 收錄線索
    public bool AddClue(string clueID, string source)
    {
        if (collectedClues.Contains(clueID))
            return false;

        collectedClues.Add(clueID);
        clueSources[clueID] = source;

        Debug.Log("已收錄線索：" + clueID);

        return true;
    }

    // 確認線索是否已收錄
    public bool HasClue(string clueID)
    {
        return collectedClues.Contains(clueID);
    }

    // 查詢線索來源
    public string GetClueSource(string clueID)
    {
        if (clueSources.TryGetValue(clueID, out string source))
            return source;

        return "來源不明";
    }

    // 解鎖新的調查方向
    public bool UnlockLead(string leadID)
    {
        if (!unlockedLeads.Add(leadID))
            return false;

        Debug.Log("解鎖調查方向：" + leadID);
        return true;
    }

    // 確認調查方向是否已解鎖
    public bool HasLead(string leadID)
    {
        return unlockedLeads.Contains(leadID);
    }
}
