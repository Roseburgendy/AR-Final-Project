using _Scripts.WY.DialogueSystem;
using UnityEngine;

public class Page3Controller : MonoBehaviour
{
    [Header("Dialogue Keys")]
    public string introKey = "page3_opening";
    public string instructionKey = "page3_instruction";
    public string endingKey = "page3_ending";

    [Header("Interaction")]
    public DragDuck dragDuck;
    private bool started = false;
    public void OnPageActivated()
    {
        if (started) return;
        started = true;

        PlayIntro();
    }
    void PlayIntro()
    {
        DialogueController.instance.PlayDialogue(introKey);
        
        float dur = DialogueController.instance.GetDialogueDuration(introKey);
        Invoke(nameof(PlayInstruction), dur+1f);
    }

    void PlayInstruction()
    {
        DialogueController.instance.PlayDialogue(instructionKey);
        
        float dur = DialogueController.instance.GetDialogueDuration(instructionKey);
        Invoke(nameof(EnableDrag), dur);
    }

    void EnableDrag()
    {
        dragDuck.Enable(controller: this);
    }

    public void OnDuckArrived()
    {
        DialogueController.instance.PlayDialogue(endingKey);
    }
}