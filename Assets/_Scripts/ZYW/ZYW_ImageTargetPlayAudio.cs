using UnityEngine;
using Vuforia;

public class ZYW_ImageTargetPlayAudio : MonoBehaviour
{
    [Header("Vuforia")]
    public ObserverBehaviour imageTargetObserver;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip clip;
    public bool loop = false;

    [Header("Play Policy")]
    public bool playOnlyOnce = true;

    private bool hasPlayed = false;

    private void Reset()
    {
        imageTargetObserver = GetComponent<ObserverBehaviour>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        if (imageTargetObserver == null) imageTargetObserver = GetComponent<ObserverBehaviour>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (imageTargetObserver != null)
            imageTargetObserver.OnTargetStatusChanged += OnTargetStatusChanged;
        else
            Debug.LogError("[ZYW_ImageTargetPlayAudio] Missing ObserverBehaviour on ImageTarget.");
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

        if (!tracked) return;

        if (playOnlyOnce && hasPlayed) return;

        Play();
    }

    private void Play()
    {
        if (audioSource == null || clip == null) return;

        hasPlayed = true;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.Play();
    }
}
