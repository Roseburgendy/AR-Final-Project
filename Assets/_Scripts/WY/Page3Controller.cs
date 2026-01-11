using _Scripts.WY.DialogueSystem;
using UnityEngine;

public class Page3Controller : MonoBehaviour
{
    public string introKey = "page3_intro";
    public string endingKey = "page3_ending";
    public DragDuck dragDuck;

    void Start()
    {
        DialogueController.instance.PlayDialogue(introKey);
        Invoke(nameof(EnableDrag), 4f);
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
