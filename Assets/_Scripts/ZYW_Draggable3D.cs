using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Collider))]
public class ZYW_Draggable3D : MonoBehaviour
{
    public string itemType; // "Apple" or "Fish"
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

        if (!PointerPressedThisFrame(out Vector2 screenPos)) return;

        if (RayHitsThisOrChild(screenPos))
        {
            Collect();
        }
    }

    private bool PointerPressedThisFrame(out Vector2 screenPos)
    {
        screenPos = default;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                screenPos = Mouse.current.position.ReadValue();
                return true;
            }
        }

        if (Touchscreen.current != null)
        {
            var t = Touchscreen.current.primaryTouch;
            if (t.press.wasPressedThisFrame)
            {
                screenPos = t.position.ReadValue();
                return true;
            }
        }
#else
        // Old Input fallback
        if (Input.GetMouseButtonDown(0))
        {
            screenPos = Input.mousePosition;
            return true;
        }

        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                screenPos = t.position;
                return true;
            }
        }
#endif
        return false;
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

        PlayCollectSfx();
        OnCollected?.Invoke(itemType);

        if (disableObjectOnCollect)
            gameObject.SetActive(false);
        else
        {
            if (col != null) col.enabled = false;
            var r = GetComponentInChildren<Renderer>();
            if (r != null) r.enabled = false;
        }
    }

    private void PlayCollectSfx()
    {
        if (ZYW_SFXManager.I == null) return;

        if (itemType == "Apple")
            ZYW_SFXManager.I.PlayApple();
        else if (itemType == "Fish")
            ZYW_SFXManager.I.PlayFish();
    }
}
