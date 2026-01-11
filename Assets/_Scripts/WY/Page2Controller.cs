using _Scripts.WY.DialogueSystem;
using UnityEngine;

public class Page2Controller : MonoBehaviour
{
    [Header("Dialogue Keys")]
    public string introKey = "page2_intro";
    public string sadKey = "page2_sad";

    [Header("Bubble Targets")]
    public BubbleTap[] bubbles;

    private int revealedCount = 0;
    private bool interactionEnabled = false;

    void Start()
    {
        DialogueController.instance.PlayDialogue(introKey);
        Invoke(nameof(EnableBubbles), 5f);
    }

    void EnableBubbles()
    {
        interactionEnabled = true;
        foreach (var b in bubbles)
            b.Enable(this);
    }

    public void OnBubbleRevealed()
    {
        revealedCount++;
        if (revealedCount >= bubbles.Length)
        {
            interactionEnabled = false;
            DialogueController.instance.PlayDialogue(sadKey);
        }
    }
}
