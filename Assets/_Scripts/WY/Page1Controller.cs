using UnityEngine;
using _Scripts.WY.DialogueSystem;

public class Page1Controller : MonoBehaviour
{
    [Header("Dialogue Keys")]
    [SerializeField] private string openingKey = "page1_opening";
    [SerializeField] private string instructionKey = "page1_instruction";
    [SerializeField] private string endingKey = "page1_ending";

    [Header("Egg Control")]
    [SerializeField] private EggTap[] eggs;

    private int crackedCount = 0;
    private bool interactionActive = false;

    void Start()
    {
        PlayOpening();
    }

    void PlayOpening()
    {
        DialogueController.instance.PlayDialogue(openingKey);
        Invoke(nameof(PlayInstruction), 6f);
    }

    void PlayInstruction()
    {
        DialogueController.instance.PlayDialogue(instructionKey);
        Invoke(nameof(EnableInteraction), 3f);
    }

    void EnableInteraction()
    {
        interactionActive = true;
        foreach (var egg in eggs)
            egg.EnableInteraction(this);
    }

    public void OnEggCracked()
    {
        crackedCount++;

        if (crackedCount >= eggs.Length)
        {
            interactionActive = false;
            PlayEnding();
        }
    }

    void PlayEnding()
    {
        DialogueController.instance.PlayDialogue(endingKey);
    }

    public bool IsInteractionActive()
    {
        return interactionActive;
    }
}