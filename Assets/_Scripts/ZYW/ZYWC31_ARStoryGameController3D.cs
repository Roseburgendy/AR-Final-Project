using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ZYWC31_ARStoryGameController3D : MonoBehaviour
{
    [Header("Vuforia")]
    public ObserverBehaviour imageTargetObserver;

    [Header("Camera (for clicking rays)")]
    public Camera arCamera;

    [Header("Audio")]
    public AudioSource narrationSource;
    public AudioClip narrative1;
    public AudioClip narrative2;

    [Header("Gameplay")]
    public GameObject gameplayRoot;

    [Header("Progress UI (Split)")]
    public ZYW_CircularProgressUI appleProgress;
    public ZYW_CircularProgressUI fishProgress;

    [Header("Items")]
    public List<ZYW_Draggable3D> draggableItems = new List<ZYW_Draggable3D>();

    [Header("Auto Scan (RECOMMENDED)")]
    public bool autoFindItemsUnderGameplayRoot = true;   // 自动扫描，避免漏物体
    public bool countOnlyActiveItems = true;             // 只统计激活物体
    public Transform itemsRootOverride = null;           // 可选：不填就用 gameplayRoot

    [Header("Idle Tip")]
    public AudioClip tipsAfter5Secs;         // TipsAfter5Secs.mp3
    public float idleTipDelaySeconds = 5f;   // 5 秒
    public bool stopTipWhenCollected = true; // 收集后如果提示在播就停止

    [Header("Winner")]
    public AudioClip winnerClip;             // Winner.mp3

    private bool hasTriggered = false;

    private int appleTotal = 0;
    private int fishTotal = 0;
    private int appleCollected = 0;
    private int fishCollected = 0;

    private Coroutine idleTipCoroutine;
    private bool hasAnyCollectedSinceGameplayShown = false;

    private bool hasPlayedWinner = false;

    private void Reset()
    {
        imageTargetObserver = GetComponent<ObserverBehaviour>();
    }

    private void Awake()
    {
        if (gameplayRoot != null) gameplayRoot.SetActive(false);

        if (imageTargetObserver == null) imageTargetObserver = GetComponent<ObserverBehaviour>();
        if (imageTargetObserver != null)
            imageTargetObserver.OnTargetStatusChanged += OnTargetStatusChanged;
        else
            Debug.LogError("[ZYW_ARStoryGameController3D] Missing ObserverBehaviour on ImageTarget.");
    }

    private void OnDestroy()
    {
        if (imageTargetObserver != null)
            imageTargetObserver.OnTargetStatusChanged -= OnTargetStatusChanged;
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        bool tracked =
            status.Status == Status.TRACKED ||
            status.Status == Status.EXTENDED_TRACKED ||
            status.Status == Status.LIMITED;

        if (!hasTriggered && tracked)
        {
            hasTriggered = true;
            StartCoroutine(RunSequence());
        }
    }

    private IEnumerator RunSequence()
    {
        yield return PlayClip(narrative1);

        if (gameplayRoot != null) gameplayRoot.SetActive(true);

        SetupClickAndProgress();

        // 先完整播放 narrative2
        yield return PlayClip(narrative2);

        // ✅ narrative2 播完之后，才开始 5 秒无操作计时
        StartIdleTipTimer();
    }


    private IEnumerator PlayClip(AudioClip clip)
    {
        if (narrationSource == null || clip == null) yield break;

        narrationSource.Stop();
        narrationSource.clip = clip;
        narrationSource.Play();

        while (narrationSource.isPlaying) yield return null;
    }

    private void SetupClickAndProgress()
    {
        appleCollected = 0;
        fishCollected = 0;
        appleTotal = 0;
        fishTotal = 0;

        hasAnyCollectedSinceGameplayShown = false;
        hasPlayedWinner = false;

        // ✅ 自动扫描：避免手动 list 漏掉物体
        if (autoFindItemsUnderGameplayRoot)
        {
            Transform root = itemsRootOverride != null
                ? itemsRootOverride
                : (gameplayRoot != null ? gameplayRoot.transform : transform);

            draggableItems = new List<ZYW_Draggable3D>(root.GetComponentsInChildren<ZYW_Draggable3D>(true));
        }

        // 统计总数（按 itemType）
        foreach (var item in draggableItems)
        {
            if (item == null) continue;

            if (countOnlyActiveItems && !item.gameObject.activeInHierarchy)
                continue;

            string type = NormalizeType(item.itemType);

            if (type == "APPLE") appleTotal++;
            else if (type == "FISH") fishTotal++;
        }

        // 初始化 UI
        if (appleProgress != null) appleProgress.SetProgress01(0f);
        if (fishProgress != null) fishProgress.SetProgress01(0f);

        // 绑定回调 + 注入相机
        foreach (var item in draggableItems)
        {
            if (item == null) continue;

            if (item.dragCamera == null) item.dragCamera = arCamera != null ? arCamera : Camera.main;

            item.OnCollected = null;
            item.OnCollected += OnItemCollected;
        }

        Debug.Log($"[Setup] AppleTotal={appleTotal}, FishTotal={fishTotal}, ItemsFound={draggableItems.Count}");
    }

    private void OnItemCollected(string rawType)
    {
        // 任意收集发生：标记已操作 + 取消提示计时
        hasAnyCollectedSinceGameplayShown = true;
        StopIdleTipTimer();

        // 如果提示音正在播，收集后停止（可选）
        if (stopTipWhenCollected && narrationSource != null &&
            narrationSource.clip == tipsAfter5Secs && narrationSource.isPlaying)
        {
            narrationSource.Stop();
        }

        string type = NormalizeType(rawType);

        if (type == "APPLE")
        {
            appleCollected++;
            float p = (appleTotal <= 0) ? 1f : (float)appleCollected / appleTotal;
            if (appleProgress != null) appleProgress.SetProgress01(p);
        }
        else if (type == "FISH")
        {
            fishCollected++;
            float p = (fishTotal <= 0) ? 1f : (float)fishCollected / fishTotal;
            if (fishProgress != null) fishProgress.SetProgress01(p);
        }

        // ✅ 全部完成：两类都点完 -> 播 Winner（只播一次）
        if (!hasPlayedWinner &&
            appleCollected >= appleTotal &&
            fishCollected >= fishTotal)
        {
            hasPlayedWinner = true;

            if (appleProgress != null) appleProgress.SetProgress01(1f);
            if (fishProgress != null) fishProgress.SetProgress01(1f);

            // 彻底停止提示计时
            StopIdleTipTimer();

            // 播 Winner
            if (narrationSource != null && winnerClip != null)
            {
                narrationSource.Stop();
                narrationSource.clip = winnerClip;
                narrationSource.Play();
            }

            Debug.Log("[ZYW_ARStoryGameController3D] Winner! All apples and fish collected.");
        }
    }

    private void StartIdleTipTimer()
    {
        StopIdleTipTimer(); // 清理旧的
        idleTipCoroutine = StartCoroutine(IdleTipRoutine());
    }

    private void StopIdleTipTimer()
    {
        if (idleTipCoroutine != null)
        {
            StopCoroutine(idleTipCoroutine);
            idleTipCoroutine = null;
        }
    }

    private IEnumerator IdleTipRoutine()
    {
        float t = 0f;
        while (t < idleTipDelaySeconds)
        {
            // 期间有收集则退出
            if (hasAnyCollectedSinceGameplayShown) yield break;

            // 如果已经完成了也没必要提示
            if (hasPlayedWinner) yield break;

            t += Time.deltaTime;
            yield return null;
        }

        // 5 秒到了还没收集且未完成：播放提示音
        if (!hasAnyCollectedSinceGameplayShown && !hasPlayedWinner)
        {
            if (narrationSource != null && tipsAfter5Secs != null)
            {
                narrationSource.Stop();
                narrationSource.clip = tipsAfter5Secs;
                narrationSource.Play();
            }
        }
    }

    private string NormalizeType(string t)
    {
        if (string.IsNullOrEmpty(t)) return "";
        return t.Trim().ToUpperInvariant();
    }
}
