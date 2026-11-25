
using UnityEngine;

public class OfficeInteraction : MonoBehaviour
{
    [Header("辦公室互動")]
    public GameObject documentsHotspot;

    [Header("調查終端機")]
    public GameObject computerPanel;

    [Header("新手教學")]
    public InvestigationTutorial investigationTutorial;

    private bool interviewCompleted = false;

    void Awake()
    {
        // 開始遊戲時不能操作文件堆
        documentsHotspot.SetActive(false);

        // 開始遊戲時關閉調查終端機
        computerPanel.SetActive(false);
    }

    // 訪談結束後，由 DialogueManager 呼叫
    public void UnlockDocuments()
    {
        interviewCompleted = true;
        documentsHotspot.SetActive(true);

        Debug.Log("調查檔案已開放查閱");
    }

    // 玩家點擊文件堆
    public void OpenComputer()
    {
        if (!interviewCompleted)
            return;

        computerPanel.SetActive(true);
        investigationTutorial?.OnDocumentsOpened();
    }

    // 玩家點擊關閉按鈕
    public void CloseComputer()
    {
        computerPanel.SetActive(false);
        investigationTutorial?.OnDocumentsClosed();
    }
}
