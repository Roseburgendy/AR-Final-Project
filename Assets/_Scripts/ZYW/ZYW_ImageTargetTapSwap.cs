using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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

    [Header("SFX (On Tap)")]
    public AudioSource sfxSource;
    public AudioClip tapSfx;
    public bool playTapSfx = true;

    [Header("UI Tap Hint (tap image to hide)")]
    public Canvas tapHintCanvas;              // 你的 TapHintCanvas
    public Graphic tapHintGraphic;            // 你的 TapHintImage(Image组件)
    public bool hideHintOnTap = true;         // 点击提示图就隐藏
    public bool consumeTapWhenHiding = true;  // 隐藏提示时是否阻止本次点击继续触发3D
    public bool hideHintWhenHitPlane = true;  // ✅ 新增：点到Plane也隐藏提示

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

        // SFX 不强制，但尽量自动拿一个
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();

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

        // UI 提示自动兜底：如果你没拖 tapHintGraphic，但拖了 Canvas，就尝试自动抓一个 Graphic
        if (tapHintGraphic == null && tapHintCanvas != null)
            tapHintGraphic = tapHintCanvas.GetComponentInChildren<Graphic>(true);
    }

    private void Update()
    {
        if (hasSwapped || isFading) return;

        if (!PointerPressedThisFrame(out Vector2 screenPos)) return;

        // ① 优先处理 UI 提示图：点到就隐藏
        if (hideHintOnTap && tapHintGraphic != null && tapHintGraphic.gameObject.activeInHierarchy)
        {
            if (IsPointerOverGraphic(tapHintGraphic, screenPos))
            {
                PlayTapSfx();

                // 隐藏提示（优先隐藏整个 Canvas，避免残留子物体挡点击）
                HideTapHint();

                if (consumeTapWhenHiding) return;
            }
        }

        // ② 再走 3D Raycast 点击逻辑
        Ray ray = rayCamera.ScreenPointToRay(screenPos);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        if (hit.collider == null) return;

        Transform t = hit.collider.transform;
        if (t == hitRoot || t.IsChildOf(hitRoot))
        {
            Debug.Log("HIT: " + hit.collider.name);

            // ✅ 点到Plane也隐藏提示
            if (hideHintWhenHitPlane) HideTapHint();

            PlayTapSfx();

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

    private bool IsPointerOverGraphic(Graphic g, Vector2 screenPos)
    {
        if (g == null) return false;

        // Overlay 模式传 null camera 即可；Screen Space - Camera 也能正常工作
        RectTransform rt = g.rectTransform;
        return RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, null);
    }

    private void PlayTapSfx()
    {
        if (playTapSfx && sfxSource != null && tapSfx != null)
            sfxSource.PlayOneShot(tapSfx);
    }

    private void HideTapHint()
    {
        if (tapHintCanvas != null && tapHintCanvas.gameObject.activeSelf)
            tapHintCanvas.gameObject.SetActive(false);
        else if (tapHintGraphic != null && tapHintGraphic.gameObject.activeSelf)
            tapHintGraphic.gameObject.SetActive(false);
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
