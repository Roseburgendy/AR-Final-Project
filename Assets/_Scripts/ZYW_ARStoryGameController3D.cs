using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ZYW_ARStoryGameController3D : MonoBehaviour
{
    [Header("Vuforia")]
    public ObserverBehaviour imageTargetObserver;

    [Header("Camera (for dragging rays)")]
    public Camera arCamera; // ARCamera 拖进来

    [Header("Audio")]
    public AudioSource narrationSource;
    public AudioClip narrative1;
    public AudioClip narrative2;

    [Header("Gameplay")]
    public GameObject gameplayRoot;                 // 初始隐藏
    public ZYW_CircularProgressUI circularProgress;     // UI 圆环
    public List<ZYW_DropZone3D> dropZones = new List<ZYW_DropZone3D>();
    public List<ZYW_Draggable3D> draggableItems = new List<ZYW_Draggable3D>();

    private bool hasTriggered = false;
    private int totalNeeded = 0;
    private int correctCount = 0;

    private void Reset()
    {
        imageTargetObserver = GetComponent<ObserverBehaviour>();
    }

    private void Awake()
    {
        if (gameplayRoot != null) gameplayRoot.SetActive(false);
        if (circularProgress != null) circularProgress.SetProgress01(0f);

        if (imageTargetObserver == null) imageTargetObserver = GetComponent<ObserverBehaviour>();
        if (imageTargetObserver != null)
        {
            imageTargetObserver.OnTargetStatusChanged += OnTargetStatusChanged;
        }
        else
        {
            Debug.LogError("[ARStoryGameController3D] Missing ObserverBehaviour on ImageTarget.");
        }
    }

    private void OnDestroy()
    {
        if (imageTargetObserver != null)
        {
            imageTargetObserver.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
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
        // 1) narrative1
        yield return PlayClip(narrative1);

        // 2) 开启玩法显示 + 绑定拖拽/进度统计 + narrative2
        if (gameplayRoot != null) gameplayRoot.SetActive(true);

        SetupDraggingAndProgress();

        yield return PlayClip(narrative2);

        // narrative2 播完不代表完成；完成由拖拽进度决定
    }

    private IEnumerator PlayClip(AudioClip clip)
    {
        if (narrationSource == null || clip == null) yield break;

        narrationSource.Stop();
        narrationSource.clip = clip;
        narrationSource.Play();

        while (narrationSource.isPlaying) yield return null;
    }

    private void SetupDraggingAndProgress()
    {
        // 需要完成的数量：以框为准（每个框只能填一次）
        totalNeeded = (dropZones != null) ? dropZones.Count : 0;
        correctCount = 0;

        if (circularProgress != null) circularProgress.SetProgress01(0f);

        // 给每个 draggable 注入 camera 和回调
        if (draggableItems != null)
        {
            foreach (var item in draggableItems)
            {
                if (item == null) continue;

                if (item.dragCamera == null) item.dragCamera = arCamera != null ? arCamera : Camera.main;

                // 清理旧回调，避免重复累计
                item.OnCorrectDropped = null;
                item.OnCorrectDropped += OnCorrectDropped;
            }
        }
    }

    private void OnCorrectDropped()
    {
        correctCount++;
        float p = (totalNeeded <= 0) ? 1f : (float)correctCount / totalNeeded;

        if (circularProgress != null) circularProgress.SetProgress01(p);

        if (correctCount >= totalNeeded)
        {
            OnAllCompleted();
        }
    }

    private void OnAllCompleted()
    {
        if (circularProgress != null) circularProgress.SetProgress01(1f);
        Debug.Log("[ARStoryGameController3D] Completed: all items placed correctly.");
        // 这里你可以加：完成音效/粒子/解锁下一页
    }
}
