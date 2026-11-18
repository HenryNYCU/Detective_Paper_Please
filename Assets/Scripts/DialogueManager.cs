
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("對話介面")]
    public TMP_Text speakerText;
    public TMP_Text dialogueText;
    public GameObject continueIndicator;

    [Header("面板控制")]
    public GameObject dialoguePanel;
    public GameObject interviewEndPanel;
    public GameObject choicePanel;

    [Header("回答選項")]
    public Button choiceAButton;
    public Button choiceBButton;

    [Header("數值顯示")]
    public TMP_Text reputationText;
    public TMP_Text justiceText;

    [Header("數值變動動畫")]
    public StatChangePopup statChangePopup;

    [Header("打字速度")]
    [Range(5f, 100f)]
    public float charactersPerSecond = 30f;

    private int reputation = 50;
    private int justice = 50;

    private string[] lines =
    {
        "警探先生，你終於來了！這幾天鎮上的人都在討論那件事。",
        "我聽陳太太說，失蹤那天晚上，有人聽見那對夫妻在爭吵。",
        "現在大家都懷疑她的丈夫，你難道不這麼認為嗎？"
    };

    private enum DialoguePhase
    {
        Talking,
        Choosing,
        Responding,
        Complete
    }

    private DialoguePhase phase = DialoguePhase.Talking;

    private int currentLine = 0;
    private int visibleCharacters = 0;
    private int totalCharacters = 0;

    private float typingTimer = 0f;
    private bool isTyping = false;

    void Start()
    {
        // 將按鈕與程式連接
        choiceAButton.onClick.AddListener(ChooseA);
        choiceBButton.onClick.AddListener(ChooseB);

        // 初始化介面
        dialoguePanel.SetActive(true);
        interviewEndPanel.SetActive(false);
        choicePanel.SetActive(false);
        continueIndicator.SetActive(false);

        speakerText.text = "克萊兒－美髮師";

        UpdateStatus();
        StartDialogueLine(lines[currentLine]);
    }

    void Update()
    {
        // 選項期間由按鈕處理輸入
        if (phase == DialoguePhase.Choosing ||
            phase == DialoguePhase.Complete)
        {
            return;
        }

        bool enterPressed =
            Keyboard.current != null &&
            (Keyboard.current.enterKey.wasPressedThisFrame ||
             Keyboard.current.numpadEnterKey.wasPressedThisFrame);

        bool mouseClicked =
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;

        if (enterPressed || mouseClicked)
        {
            OnContinuePressed();
            return;
        }

        // 打字機效果
        if (isTyping)
        {
            typingTimer += Time.deltaTime;

            float interval = 1f / charactersPerSecond;

            while (typingTimer >= interval &&
                   visibleCharacters < totalCharacters)
            {
                typingTimer -= interval;
                visibleCharacters++;

                dialogueText.maxVisibleCharacters =
                    visibleCharacters;
            }

            if (visibleCharacters >= totalCharacters)
            {
                FinishTyping();
            }
        }
    }

    void OnContinuePressed()
    {
        // 打字中：立即顯示完整句子
        if (isTyping)
        {
            FinishTyping();
            return;
        }

        // 玩家正在閱讀克萊兒的陳述
        if (phase == DialoguePhase.Talking)
        {
            currentLine++;

            if (currentLine < lines.Length)
            {
                StartDialogueLine(lines[currentLine]);
            }
            else
            {
                ShowChoices();
            }
        }
        // 玩家已經選擇回答，克萊兒也回應完畢
        else if (phase == DialoguePhase.Responding)
        {
            CompleteInterview();
        }
    }

    void StartDialogueLine(string content)
    {
        dialogueText.text = content;

        dialogueText.maxVisibleCharacters = int.MaxValue;
        dialogueText.ForceMeshUpdate();

        totalCharacters =
            dialogueText.textInfo.characterCount;

        visibleCharacters = 0;
        typingTimer = 0f;
        isTyping = true;

        dialogueText.maxVisibleCharacters = 0;
        continueIndicator.SetActive(false);
    }

    void FinishTyping()
    {
        isTyping = false;
        visibleCharacters = totalCharacters;

        dialogueText.maxVisibleCharacters =
            totalCharacters;

        // 最後一句結束時，自動顯示回答選項
        if (phase == DialoguePhase.Talking &&
            currentLine == lines.Length - 1)
        {
            ShowChoices();
        }
        else
        {
            continueIndicator.SetActive(true);
        }
    }

    void ShowChoices()
    {
        phase = DialoguePhase.Choosing;
        isTyping = false;

        dialogueText.maxVisibleCharacters = int.MaxValue;
        continueIndicator.SetActive(false);

        choicePanel.SetActive(true);
    }

    // 選擇 A：謹慎查證
    void ChooseA()
    {
        if (phase != DialoguePhase.Choosing)
            return;

        reputation -= 5;
        justice += 10;

        UpdateStatus();
        // 顯示聲譽 -5、公正 +10 的浮動動畫
        statChangePopup.Show(-5, 10);

        choicePanel.SetActive(false);

        phase = DialoguePhase.Responding;

        StartDialogueLine(
            "你們外地來的警探就是這樣，什麼事都說還要調查！"
        );
    }

    // 選擇 B：附和居民的懷疑
    void ChooseB()
    {
        if (phase != DialoguePhase.Choosing)
            return;

        reputation += 10;
        justice -= 10;

        UpdateStatus();
        // 顯示聲譽 +10、公正 -10 的浮動動畫
        statChangePopup.Show(10, -10);

        choicePanel.SetActive(false);

        phase = DialoguePhase.Responding;

        StartDialogueLine(
            "我就知道警探先生會重視我們說的話！"
        );
    }

    void UpdateStatus()
    {
        reputation = Mathf.Clamp(reputation, 0, 100);
        justice = Mathf.Clamp(justice, 0, 100);

        reputationText.text = "聲譽值：" + reputation;
        justiceText.text = "公正值：" + justice;
    }

    void CompleteInterview()
    {
        phase = DialoguePhase.Complete;
        continueIndicator.SetActive(false);

        // 隱藏居民對話框
        dialoguePanel.SetActive(false);

        // 顯示完成通知，觸發原有浮現動畫
        interviewEndPanel.SetActive(true);

        Debug.Log(
            "克萊兒訪談完成。聲譽：" +
            reputation + "，公正：" + justice
        );
    }
}
