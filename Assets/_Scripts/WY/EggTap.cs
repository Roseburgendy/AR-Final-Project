using _Scripts.WY.DialogueSystem;
using UnityEngine;

public class EggTap : MonoBehaviour
{
    public GameObject duckling;
    public Animator eggAnimator;
    private bool cracked = false;

    void Update()
    {
        if (cracked) return;

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

        if (eggAnimator != null)
            eggAnimator.SetTrigger("crack");

        Invoke(nameof(ShowDuckling), 1.2f);
        DialogueController.instance.PlayDialogue("test");
        //StoryUIManager.Instance.OnEggCracked();
    }

    // show ducks
    void ShowDuckling()
    {
        duckling.SetActive(true);
        duckling.GetComponent<Animator>()?.SetTrigger("jump");
       // gameObject.SetActive(false);
    }
}
