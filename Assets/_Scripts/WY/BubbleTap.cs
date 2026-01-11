using UnityEngine;

public class BubbleTap : MonoBehaviour
{
    public GameObject bubble;

    private bool revealed = false;
    private bool canInteract = false;
    private Page2Controller pageController;

    // 由 PageController 调用
    public void Enable(Page2Controller controller)
    {
        canInteract = true;
        pageController = controller;
    }

    void Update()
    {
        if (!canInteract || revealed) return;

        if (TryGetInput(out Vector3 pos))
        {
            Ray ray = Camera.main.ScreenPointToRay(pos);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform)
                {
                    Reveal();
                }
            }
        }
    }

    void Reveal()
    {
        revealed = true;
        bubble.SetActive(true);

        pageController?.OnBubbleRevealed();
    }

    bool TryGetInput(out Vector3 pos)
    {
        pos = Vector3.zero;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            pos = Input.GetTouch(0).position;
            return true;
        }
        if (Input.GetMouseButtonDown(0))
        {
            pos = Input.mousePosition;
            return true;
        }
        return false;
    }
}