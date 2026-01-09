using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ZYW_Draggable3D : MonoBehaviour
{
    public string itemType; // "Apple" or "Fish"
    public Action OnCorrectDropped;

    [Header("Drag Settings")]
    public Camera dragCamera;                 // 不填就用 Camera.main
    public LayerMask draggableRayMask = ~0;   // 射线可命中的层（默认全层）
    public LayerMask dropZoneMask = ~0;       // DropZone 所在层（建议设置成专用层）
    public float dropSearchRadius = 0.20f;    // 松手时找框的半径

    private Rigidbody rb;
    private Collider col;

    private bool locked = false;
    private bool dragging = false;

    private Plane dragPlane;      // 拖拽平面
    private float planeEnter = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (dragCamera == null) dragCamera = Camera.main;

        // 建议有 Rigidbody 更稳
        if (rb != null) rb.isKinematic = true;
    }

    private void Update()
    {
        if (locked) return;
        if (dragCamera == null) dragCamera = Camera.main;
        if (dragCamera == null) return;

        // 鼠标/触摸统一处理
        if (PointerDownThisObject())
        {
            BeginDrag();
        }

        if (dragging)
        {
            if (PointerHeld())
            {
                DragMove();
            }
            else
            {
                EndDrag();
            }
        }
    }

    private bool PointerDownThisObject()
    {
        // Mouse
        if (Input.GetMouseButtonDown(0))
        {
            return RayHitsThis(Input.mousePosition);
        }

        // Touch
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            return RayHitsThis(Input.GetTouch(0).position);
        }

        return false;
    }

    private bool PointerHeld()
    {
        if (Input.touchCount > 0)
        {
            var ph = Input.GetTouch(0).phase;
            return ph == TouchPhase.Moved || ph == TouchPhase.Stationary;
        }
        return Input.GetMouseButton(0);
    }

    private Vector2 PointerPos()
    {
        if (Input.touchCount > 0) return Input.GetTouch(0).position;
        return Input.mousePosition;
    }

    private bool RayHitsThis(Vector2 screenPos)
    {
        Ray ray = dragCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 999f, draggableRayMask, QueryTriggerInteraction.Ignore))
        {
            return hit.collider == col;
        }
        return false;
    }

    private void BeginDrag()
    {
        dragging = true;

        // 固定深度拖拽：拖拽平面法线 = 相机 forward，平面过物体当前位置
        dragPlane = new Plane(dragCamera.transform.forward, transform.position);

        // 拖拽时为了稳定：可临时关掉 collider 的一些交互（不强制）
        if (rb != null) rb.isKinematic = true;
    }

    private void DragMove()
    {
        Ray ray = dragCamera.ScreenPointToRay(PointerPos());
        if (dragPlane.Raycast(ray, out planeEnter))
        {
            Vector3 worldPos = ray.GetPoint(planeEnter);
            transform.position = worldPos;
        }
    }

    private void EndDrag()
    {
        dragging = false;

        // 查找附近 DropZone
        Collider[] near = Physics.OverlapSphere(transform.position, dropSearchRadius, dropZoneMask, QueryTriggerInteraction.Collide);

        ZYW_DropZone3D bestZone = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < near.Length; i++)
        {
            var zone = near[i].GetComponentInParent<ZYW_DropZone3D>();
            if (zone == null) continue;

            float d = Vector3.Distance(transform.position, (zone.snapAnchor != null ? zone.snapAnchor.position : zone.transform.position));
            if (d < bestDist)
            {
                bestDist = d;
                bestZone = zone;
            }
        }

        if (bestZone != null)
        {
            // 同时满足：类型正确 + 没被占用
            bool accepted = bestZone.TryAccept(this);
            if (accepted)
            {
                LockInPlace();
                OnCorrectDropped?.Invoke();
                return;
            }
        }

        // 没成功落框：保持当前位置（你也可以改成回到出生点）
    }

    public void SnapTo(Transform anchor)
    {
        transform.position = anchor.position;
        transform.rotation = anchor.rotation;
    }

    public void LockInPlace()
    {
        locked = true;

        // 锁定后建议关掉 collider 防止再被点到
        if (col != null) col.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
