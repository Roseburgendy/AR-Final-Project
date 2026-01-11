using UnityEngine;

public class DragDuck : MonoBehaviour
{
    public Transform destination;
    public float reachDistance = 0.1f;

    private bool dragging = false;
    private bool canDrag = false;
    private Page3Controller pageController;

    // 由 PageController 调用
    public void Enable(Page3Controller controller)
    {
        canDrag = true;
        pageController = controller;
    }

    void Update()
    {
        if (!canDrag) return;

        if (TryGetInput(out Vector3 pos))
        {
            dragging = true;
        }

        if (dragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(pos);
            Plane plane = new Plane(Vector3.up, transform.position);
            if (plane.Raycast(ray, out float dist))
            {
                Vector3 target = ray.GetPoint(dist);
                transform.position = Vector3.Lerp(
                    transform.position,
                    target,
                    Time.deltaTime * 5f
                );
            }

            if (Vector3.Distance(transform.position, destination.position) < reachDistance)
            {
                Arrive();
            }
        }
    }

    void Arrive()
    {
        canDrag = false;
        dragging = false;
        enabled = false;

        pageController?.OnDuckArrived();
    }

    bool TryGetInput(out Vector3 pos)
    {
        pos = Vector3.zero;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            pos = Input.GetTouch(0).position;
            return true;
        }
        if (Input.GetMouseButton(0))
        {
            pos = Input.mousePosition;
            return true;
        }
        return false;
    }
}