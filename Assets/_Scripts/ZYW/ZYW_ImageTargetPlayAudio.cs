using UnityEngine;
using Vuforia;
using _Scripts.WY.DialogueSystem;

public class ZYW_ImageTargetPlayDialogue : MonoBehaviour
{
    [Header("Vuforia")]
    public ObserverBehaviour imageTargetObserver;

    [Header("Dialogue")]
    [SerializeField] private string dialogueKey = "narrative3";

    [Header("Play Policy")]
    public bool playOnlyOnce = true;

    private bool hasPlayed = false;

    private void Reset()
    {
        imageTargetObserver = GetComponent<ObserverBehaviour>();
    }

    private void Awake()
    {
        if (imageTargetObserver == null)
            imageTargetObserver = GetComponent<ObserverBehaviour>();

        if (imageTargetObserver != null)
            imageTargetObserver.OnTargetStatusChanged += OnTargetStatusChanged;
        else
            Debug.LogError("[ZYW_ImageTargetPlayDialogue] Missing ObserverBehaviour on ImageTarget.");
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

        PlayDialogue();
    }

    private void PlayDialogue()
    {
        if (DialogueController.instance == null || string.IsNullOrEmpty(dialogueKey))
            return;

        hasPlayed = true;
        DialogueController.instance.PlayDialogue(dialogueKey);
    }
}