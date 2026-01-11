using _Scripts.WY.DialogueSystem;
using UnityEngine;

public class Page2Controller : MonoBehaviour
{
    [Header("Dialogue Keys")]
    public string introKey = "page2_opening";
    public string instructionKey = "page2_instruction";
    public string sadKey = "page2_ending";

    [Header("Bubble Targets")]
    public BubbleTap[] bubbles;

    private int revealedCount = 0;
    private bool interactionEnabled = false;


    private bool started = false;
    public void OnPageActivated()
    {
        if (started) return;
        started = true;
        
        DialogueController.instance.PlayDialogue(introKey);
        
        float dur = DialogueController.instance.GetDialogueDuration(introKey);
        Invoke(nameof(PlayInstruction), dur+1f);
    }
    void PlayInstruction()
    {
        DialogueController.instance.PlayDialogue(instructionKey);
        
        float dur = DialogueController.instance.GetDialogueDuration(instructionKey);
        Invoke(nameof(EnableBubbles), dur);
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
