using UnityEngine;

public class EggTap : MonoBehaviour
{
    public GameObject duckling;
    public Animator eggAnimator;
    private bool cracked = false;

    void Update()
    {
        if (cracked) return;

        if (Input.touchCount >0 && Input.touches[0].phase == TouchPhase.Began)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform)
                {
                    CrackEgg();
                }
            }
        }
    }

    // Crack Egg
    void CrackEgg()
    {
        cracked = true;

        if (eggAnimator != null)
            eggAnimator.SetTrigger("Crack");

        Invoke(nameof(ShowDuckling), 1.2f);

        //StoryUIManager.Instance.OnEggCracked();
    }

    // show ducks
    void ShowDuckling()
    {
        duckling.SetActive(true);
        duckling.GetComponent<Animator>()?.SetTrigger("Hatch");
        gameObject.SetActive(false);
    }
}
