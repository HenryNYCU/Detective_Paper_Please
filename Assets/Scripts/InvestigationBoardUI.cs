
using UnityEngine;

public class InvestigationBoardUI : MonoBehaviour
{
    [Header("大型調查筆記")]
    public GameObject investigationBoardPanel;

    [Header("線索顯示")]
    public InvestigationBoardClues boardClues;

    [Header("新手教學")]
    public InvestigationTutorial investigationTutorial;

    void Awake()
    {
        // 遊戲開始時不顯示大型調查筆記
        if (investigationBoardPanel != null)
        {
            investigationBoardPanel.SetActive(false);
        }
    }

    // 開啟大型調查筆記
    public void OpenBoard()
    {
        if (investigationBoardPanel == null)
            return;

        investigationBoardPanel.SetActive(true);
        boardClues?.RefreshFromNotebook();
        investigationTutorial?.OnBoardOpened();
        Debug.Log("已開啟大型調查筆記");
    }

    // 返回訪談紀錄
    public void CloseBoard()
    {
        if (investigationBoardPanel == null)
            return;

        investigationBoardPanel.SetActive(false);
        investigationTutorial?.OnBoardClosed();
        Debug.Log("已返回訪談紀錄");
    }
}
