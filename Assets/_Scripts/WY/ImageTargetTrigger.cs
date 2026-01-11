using UnityEngine;
using Vuforia;

public class ImageTargetTrigger : MonoBehaviour
{
    public MonoBehaviour pageController;

    private bool triggered = false;

    void Start()
    {
        var observer = GetComponent<ObserverBehaviour>();
        if (observer != null)
        {
            observer.OnTargetStatusChanged += OnTargetStatusChanged;
        }
    }

    void OnDestroy()
    {
        var observer = GetComponent<ObserverBehaviour>();
        if (observer != null)
        {
            observer.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        if (triggered) return;

        if (status.Status == Status.TRACKED ||
            status.Status == Status.EXTENDED_TRACKED)
        {
            triggered = true;

            // Notify PageController
            pageController.SendMessage("OnPageActivated", SendMessageOptions.DontRequireReceiver);
        }
    }
}