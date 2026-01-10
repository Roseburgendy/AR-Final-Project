using UnityEngine;
using UnityEngine.Video;
using Vuforia;

public class ZYW_PlayVideoWhenTracked : MonoBehaviour
{
    [Header("Vuforia")]
    public ObserverBehaviour target;

    [Header("Video")]
    public VideoPlayer videoPlayer;

    [Header("Options")]
    public bool loop = true;
    public bool stopWhenLost = true;   // 丢失识别就停止并回到0

    private void Reset()
    {
        target = GetComponent<ObserverBehaviour>();
    }

    private void Awake()
    {
        if (target == null) target = GetComponent<ObserverBehaviour>();
        if (videoPlayer == null) videoPlayer = GetComponentInChildren<VideoPlayer>(true);

        if (videoPlayer != null)
        {
            // ✅ 关键：开场强制不播放（彻底杜绝提前声音）
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = loop;
            videoPlayer.Stop();
        }

        if (target != null)
            target.OnTargetStatusChanged += OnStatusChanged;
        else
            Debug.LogError("[ZYW_PlayVideoWhenTracked] Missing ObserverBehaviour on ImageTarget.");
    }

    private void OnDestroy()
    {
        if (target != null)
            target.OnTargetStatusChanged -= OnStatusChanged;
    }

    private void OnStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        bool tracked =
            status.Status == Status.TRACKED ||
            status.Status == Status.EXTENDED_TRACKED ||
            status.Status == Status.LIMITED;

        if (videoPlayer == null) return;

        if (tracked)
        {
            if (!videoPlayer.isPlaying) videoPlayer.Play();
        }
        else
        {
            if (stopWhenLost && videoPlayer.isPlaying) videoPlayer.Stop();
        }
    }
}
