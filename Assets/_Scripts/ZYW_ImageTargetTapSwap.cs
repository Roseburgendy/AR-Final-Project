using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ZYW_ImageTargetTapSwap : MonoBehaviour
{
    [Header("Ray Camera (ARCamera)")]
    public Camera rayCamera;

    [Header("Click Root (Object with Collider)")]
    public Transform hitRoot; // Collider 在哪个物体上就拖哪个（Plane3.2.1或3.2.2）

    [Header("Renderer A (start visible)")]
    public Renderer planeA;   // Plane3.2.1

    [Header("Renderer B (fade in)")]
    public Renderer planeB;   // Plane3.2.2

    [Header("Textures")]
    public Texture textureA;
    public Texture textureB;

    [Header("Fade")]
    public float fadeDuration = 0.6f;
    public bool fadeOutA = true;
    public bool disableAfterOnce = true;

    [Header("Optional: prevent overlap flicker")]
    public Vector3 planeBLocalOffset = new Vector3(0f, 0f, -0.001f);

    private bool hasSwapped = false;
    private bool isFading = false;
    private int texId;

    private void Awake()
    {
        if (planeA == null || planeB == null || rayCamera == null || hitRoot == null)
        {
            Debug.LogError("Assign rayCamera, hitRoot, planeA, planeB in Inspector.");
            enabled = false;
            return;
        }

        texId = Shader.PropertyToID("_MainTex");
        if (planeA.sharedMaterial != null && planeA.sharedMaterial.HasProperty("_BaseMap"))
            texId = Shader.PropertyToID("_BaseMap");

        // 避免共面
        planeB.transform.localPosition = planeA.transform.localPosition + planeBLocalOffset;

        // ===== 初始状态：只显示A，B完全隐藏 =====
        ApplyTexture(planeA, textureA);
        SetAlpha(planeA, 1f);
        planeA.enabled = true;

        ApplyTexture(planeB, textureB);
        SetAlpha(planeB, 0f);

        // 关键：直接关掉 B 的 Renderer，保证绝不会提前显示
        planeB.enabled = false;
    }

    private void Update()
    {
        if (hasSwapped || isFading) return;

        if (!PointerPressedThisFrame(out Vector2 screenPos)) return;

        Ray ray = rayCamera.ScreenPointToRay(screenPos);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        if (hit.collider == null) return;

        Transform t = hit.collider.transform;
        if (t == hitRoot || t.IsChildOf(hitRoot))
        {
            Debug.Log("HIT: " + hit.collider.name);
            StartCoroutine(FadeAToBOnce());
        }
    }

    private bool PointerPressedThisFrame(out Vector2 screenPos)
    {
        screenPos = default;

        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                screenPos = touch.position.ReadValue();
                return true;
            }
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPos = Mouse.current.position.ReadValue();
            return true;
        }

        return false;
    }

    private IEnumerator FadeAToBOnce()
    {
        isFading = true;

        // 开始渐变前，打开B的Renderer
        planeB.enabled = true;

        // 再确认一次初始alpha
        SetAlpha(planeA, 1f);
        SetAlpha(planeB, 0f);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);

            SetAlpha(planeB, k);
            if (fadeOutA) SetAlpha(planeA, 1f - k);

            yield return null;
        }

        SetAlpha(planeB, 1f);
        if (fadeOutA) SetAlpha(planeA, 0f);

        hasSwapped = true;
        isFading = false;

        if (disableAfterOnce)
        {
            var col = hitRoot.GetComponentInChildren<Collider>();
            if (col != null) col.enabled = false;
        }
    }

    private void ApplyTexture(Renderer r, Texture tex)
    {
        if (r == null || tex == null) return;
        r.material.SetTexture(texId, tex);
    }

    private void SetAlpha(Renderer r, float a)
    {
        if (r == null) return;
        var mat = r.material;

        if (mat.HasProperty("_Color"))
        {
            Color c = mat.GetColor("_Color");
            c.a = a;
            mat.SetColor("_Color", c);
        }
        else
        {
            Color c = mat.color;
            c.a = a;
            mat.color = c;
        }
    }
}
