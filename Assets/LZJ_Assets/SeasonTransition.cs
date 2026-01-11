using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using TMPro;
using _Scripts.WY.DialogueSystem;

public class SeasonTransition : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("=== 场景管理 ===")]
    public GameObject videoQuad;

    [Header("=== UI 元素 ===")]
    public GameObject holdButton;       // HoldButton 整个物体
    public GameObject hintBar;          // HintBar 整个物体
    public Image progressRing;          // 进度环
    public TextMeshProUGUI hintText;    // 提示文字

    [Header("=== 长按设置 ===")]
    public float holdDuration = 3f;

    [Header("=== 视频播放 ===")]
    public VideoPlayer videoPlayer;

    [Header("=== 对话设置 ===")]
    public string page7DialogueKey = "Scene7";
    public float dialogueStartDelay = 1f;

    [Header("=== UI 激活控制 ===")]
    public bool hideUIOnStart = true;           // 开始时隐藏 UI

    // ============ 私有变量 ============
    private float holdTimer = 0f;
    private bool isHolding = false;
    private bool hasTransitioned = false;
    private bool dialogueFinished = false;
    private bool dialogueStarted = false;
    private bool isInitialized = false;

    // ============ Unity 生命周期 ============

    void Start()
    {
        // 不自动初始化，等待 ImageTarget 识别后调用 Initialize()
        Debug.Log("SeasonTransition 已准备，等待识别 ImageTarget");
    }

    void Update()
    {
        // 检测对话是否播放完成
        if (dialogueStarted && !dialogueFinished)
        {
            if (DialogueController.instance != null && !DialogueController.instance.IsPlaying())
            {
                dialogueFinished = true;

                // 对话结束后显示 UI
                ShowUI();

                if (hintText != null)
                {
                    hintText.text = "Hold down the button to make time pass more quickly.";
                }

                Debug.Log("对话播放完成，显示 UI，可以开始长按");
            }
        }

        // 只有对话播放完成后才能长按
        if (isHolding && dialogueFinished && !hasTransitioned)
        {
            holdTimer += Time.deltaTime;

            if (progressRing != null)
            {
                progressRing.fillAmount = holdTimer / holdDuration;
            }

            if (holdTimer >= holdDuration)
            {
                PlayTransitionVideo();
                hasTransitioned = true;
            }
        }
    }

    void OnDestroy()
    {
        // 清理事件监听
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    // ============ 公共方法 ============

    /// <summary>
    /// 初始化场景（由 Page7TrackingHandler 在识别到 ImageTarget 后调用）
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
        {
            Debug.Log("场景已经初始化过了");
            return;
        }

        Debug.Log("=== ImageTarget 已识别，开始初始化第7页场景 ===");

        // 1. 隐藏 UI
        if (hideUIOnStart)
        {
            HideUI();
        }

        // 2. 初始化视频
        if (videoQuad != null)
        {
            videoQuad.SetActive(false);
        }

        // 3. 初始化进度条
        if (progressRing != null)
        {
            progressRing.fillAmount = 0f;
        }

        // 4. 播放冬天背景音乐
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Play("Winter");
            Debug.Log("播放冬天音乐");
        }

        // 5. 延迟后自动播放对话
        Invoke("StartDialogue", dialogueStartDelay);

        // 6. 设置初始提示（虽然 UI 隐藏，但先设置好）
        if (hintText != null)
        {
            hintText.text = "Listen to the Ugly Duckling's story...";
        }

        // 7. 监听视频播放完成事件
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        isInitialized = true;
    }

    /// <summary>
    /// 重置场景（如果需要重新播放）
    /// </summary>
    public void ResetScene()
    {
        dialogueFinished = false;
        dialogueStarted = false;
        hasTransitioned = false;
        isHolding = false;
        holdTimer = 0f;

        if (progressRing != null) progressRing.fillAmount = 0f;
        if (videoQuad != null) videoQuad.SetActive(false);

        HideUI();
        isInitialized = false;

        Debug.Log("场景已重置");
    }

    // ============ 对话控制 ============

    void StartDialogue()
    {
        Debug.Log("=== StartDialogue 被调用 ===");

        // 检查 DialogueController 是否存在
        if (DialogueController.instance == null)
        {
            Debug.LogError(" DialogueController.instance 是 null！请检查场景中是否有 DialogueController 物体！");

            // 如果没有对话系统，直接显示 UI 并允许长按
            dialogueFinished = true;
            ShowUI();
            if (hintText != null)
            {
                hintText.text = "Hold down the button to make time pass more quickly.";
            }
            return;
        }

        Debug.Log("✓ DialogueController 存在");
        Debug.Log($"尝试播放对话：{page7DialogueKey}");

        // 播放对话
        DialogueController.instance.PlayDialogue(page7DialogueKey);
        dialogueStarted = true;

        Debug.Log("对话播放指令已发送");
    }

    // ============ UI 控制 ============

    /// <summary>
    /// 显示 HoldButton 和 HintBar
    /// </summary>
    void ShowUI()
    {
        if (holdButton != null && !holdButton.activeInHierarchy)
        {
            holdButton.SetActive(true);
            Debug.Log("显示 HoldButton");
        }

        if (hintBar != null && !hintBar.activeInHierarchy)
        {
            hintBar.SetActive(true);
            Debug.Log("显示 HintBar");
        }
    }

    /// <summary>
    /// 隐藏 HoldButton 和 HintBar
    /// </summary>
    void HideUI()
    {
        if (holdButton != null && holdButton.activeInHierarchy)
        {
            holdButton.SetActive(false);
            Debug.Log("隐藏 HoldButton");
        }

        if (hintBar != null && hintBar.activeInHierarchy)
        {
            hintBar.SetActive(false);
            Debug.Log("隐藏 HintBar");
        }
    }

    // ============ 长按检测 ============

    /// <summary>
    /// 按下按钮（实现 IPointerDownHandler 接口）
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        // 只有对话播放完成后才响应长按
        if (dialogueFinished && !hasTransitioned)
        {
            isHolding = true;

            if (hintText != null)
            {
                hintText.text = "Keep pressing and holding…";
            }

            Debug.Log("开始长按");
        }
        else if (!dialogueFinished)
        {
            // 对话还没结束，提示用户
            if (hintText != null)
            {
                hintText.text = "Please finish listening to the story first...";
            }

            Debug.Log("对话未结束，无法长按");
        }
    }

    /// <summary>
    /// 松开按钮（实现 IPointerUpHandler 接口）
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!hasTransitioned && dialogueFinished)
        {
            isHolding = false;
            holdTimer = 0f;

            if (progressRing != null)
            {
                progressRing.fillAmount = 0f;
            }

            if (hintText != null)
            {
                hintText.text = "Hold down the button to make time pass more quickly.";
            }

            Debug.Log("松开，进度重置");
        }
    }

    // ============ 视频控制 ============

    /// <summary>
    /// 长按完成后播放过渡视频
    /// </summary>
    void PlayTransitionVideo()
    {
        Debug.Log("长按完成，开始播放过渡视频");

        // 停止冬天音乐
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Stop("Winter");
        }

        // 显示视频并播放
        if (videoQuad != null)
        {
            videoQuad.SetActive(true);
        }

        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }

        // 更新 UI
        if (hintText != null)
        {
            hintText.text = "As the seasons change…";
        }

        if (progressRing != null)
        {
            progressRing.fillAmount = 1f;
        }
    }

    /// <summary>
    /// 视频播放完成的回调
    /// </summary>
    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("视频播放完成，切换到春天场景");

        // 隐藏视频
        if (videoQuad != null)
        {
            videoQuad.SetActive(false);
        }

        

        // 更新 UI 提示
        if (hintText != null)
        {
            hintText.text = "Spring has arrived!";
        }
    }
}