using _Scripts.WY.DialogueSystem;
using UnityEngine;

public class EggTap : MonoBehaviour
{
    public GameObject duckling;
    public Animator eggAnimator;
    
    private bool cracked = false;
    private Page1Controller pg1controller;
    private bool canInteract = false;

    void Update()
    {
        if (!canInteract || cracked) return;

        Vector3 inputPosition;
        bool hasInput = false;

        // 1. Touch input（Android / iOS）
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            inputPosition = Input.GetTouch(0).position;
            hasInput = true;
        }
        // 2. Mouse input（Editor / PC 调试）
        else if (Input.GetMouseButtonDown(0))
        {
            inputPosition = Input.mousePosition;
            hasInput = true;
        }
        else
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(inputPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == transform)
            {
                CrackEgg();
            }
        }
    }

    // Crack Egg
    void CrackEgg()
    {
        cracked = true;
        // notify event suscriber to progress narrative
        pg1controller.OnEggCracked();

        // play animation
        if (eggAnimator != null)
            eggAnimator.SetTrigger("crack");
        // show ducks
        Invoke(nameof(ShowDuckling), 1.2f);
    }

    // show ducks
    void ShowDuckling()
    {
        duckling.SetActive(true);
        duckling.GetComponent<Animator>()?.SetTrigger("jump");
       // gameObject.SetActive(false);
    }

    public void EnableInteraction(Page1Controller pg1controller)
    {
        this.pg1controller = pg1controller;
        canInteract = true;
    }
}
