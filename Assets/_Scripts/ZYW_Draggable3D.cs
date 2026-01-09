using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Collider))]
public class ZYW_Draggable3D : MonoBehaviour
{
    public string itemType; // "Apple" or "Fish"

    // ✅ 改成带类型的回调
    public Action<string> OnCollected;

    [Header("Camera / Raycast")]
    public Camera dragCamera;
    public LayerMask draggableRayMask = ~0;

    [Header("Tap To Collect")]
    public bool enableTapToCollect = true;
    public bool disableObjectOnCollect = true;

    private Rigidbody rb;
    private Collider col;
    private bool locked = false;

#if ENABLE_INPUT_SYSTEM
    private bool pointerDownThisFrame;
    private Vector2 pointerPos;
#endif

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (dragCamera == null) dragCamera = Camera.main;

        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void Update()
    {
        if (locked) return;
        if (!enableTapToCollect) return;

        if (dragCamera == null) dragCamera = Camera.main;
        if (dragCamera == null) return;

#if ENABLE_INPUT_SYSTEM
        ReadPointer();

        if (pointerDownThisFrame && RayHitsThisOrChild(pointerPos))
        {
            Collect();
        }
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private void ReadPointer()
    {
        pointerDownThisFrame = false;

        if (Mouse.current != null)
        {
            pointerDownThisFrame = Mouse.current.leftButton.wasPressedThisFrame;
            pointerPos = Mouse.current.position.ReadValue();
            return;
        }

        if (Touchscreen.current != null)
        {
            var t = Touchscreen.current.primaryTouch;
            pointerDownThisFrame = t.press.wasPressedThisFrame;
            pointerPos = t.position.ReadValue();
        }
    }

    private bool RayHitsThisOrChild(Vector2 screenPos)
    {
        Ray ray = dragCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 999f, draggableRayMask, QueryTriggerInteraction.Ignore))
        {
            return hit.transform == transform || hit.transform.IsChildOf(transform);
        }
        return false;
    }

    private void Collect()
    {
        if (locked) return;
        locked = true;

        // ✅ 上报类型：Apple / Fish
        OnCollected?.Invoke(itemType);

        if (disableObjectOnCollect) gameObject.SetActive(false);
        else
        {
            if (col != null) col.enabled = false;
            var r = GetComponentInChildren<Renderer>();
            if (r != null) r.enabled = false;
        }
    }
#endif
}
