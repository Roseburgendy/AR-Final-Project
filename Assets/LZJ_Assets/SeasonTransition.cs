using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using TMPro;

public class SeasonTransition : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("场景管理")]
    public GameObject videoQuad;

    [Header("长按设置")]
    public float holdDuration = 3f;  // 需要长按3秒
    private float holdTimer = 0f;
    private bool isHolding = false;
    private bool hasTransitioned = false;

    [Header("UI反馈")]
    public Image progressRing;
    public TextMeshProUGUI hintText;

    [Header("视频播放")]
    public VideoPlayer videoPlayer;

    

    void Start()
    {
        // 初始化
        if (videoQuad != null) videoQuad.SetActive(false);

        progressRing.fillAmount = 0f;

        // 监听视频播放完成事件
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    void Update()
    {
        if (isHolding && !hasTransitioned)
        {
            holdTimer += Time.deltaTime;

            // 更新进度条
            progressRing.fillAmount = holdTimer / holdDuration;

            // 完成转换
            if (holdTimer >= holdDuration)
            {
                PlayTransitionVideo();
                hasTransitioned = true;
            }
        }
    }

    // 检测按下
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!hasTransitioned)
        {
            isHolding = true;
            hintText.text = "Keep pressing and holding…";
            Debug.Log("开始长按");
        }
    }

    // 检测松开
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!hasTransitioned)
        {
            isHolding = false;
            holdTimer = 0f;  // 松手重置
            progressRing.fillAmount = 0f;
            hintText.text = "Hold down the button to make time pass more quickly.";
            Debug.Log("松开，进度重置");
        }
    }

    void PlayTransitionVideo()
    {
        Debug.Log("开始播放过渡视频");

        

        // 显示视频并播放
        if (videoQuad != null) videoQuad.SetActive(true);
        if (videoPlayer != null) videoPlayer.Play();

        // UI提示
        hintText.text = "As the seasons change…";
        progressRing.fillAmount = 1f;
    }

    // 视频播放完成后的回调
    void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("视频播放完成，切换到春天场景");

        // 隐藏视频
        if (videoQuad != null) videoQuad.SetActive(false);

        

        // UI提示
        hintText.text = "Spring has arrived!";
    }

    void OnDestroy()
    {
        // 清理事件监听
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}