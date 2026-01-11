using UnityEngine;
using UnityEngine.UI;

public class ZYW_CircularProgressUI : MonoBehaviour
{
    [Header("Assign the FG Image (RingFG) here")]
    [SerializeField] private Image ringFillImage;

    [Range(0f, 1f)]
    [SerializeField] private float current01 = 0f;

    private void Awake()
    {
        AutoBindIfNeeded();
        Apply(current01);
    }

    private void OnEnable()
    {
        // 每次启用强制归零（避免显示旧值）
        SetProgress01(0f);
    }

    private void AutoBindIfNeeded()
    {
        if (ringFillImage != null) return;

        // 尝试按名字找子物体 RingFG
        Transform t = transform.Find("RingFG");
        if (t != null) ringFillImage = t.GetComponent<Image>();

        // 找不到就兜底：在子物体里随便找一个 Image（不推荐，但至少不会 null）
        if (ringFillImage == null)
        {
            ringFillImage = GetComponentInChildren<Image>(true);
        }

        if (ringFillImage == null)
        {
            Debug.LogError("[ZYW_CircularProgressUI] ringFillImage is NULL. Please drag RingFG(Image) into the field.");
        }
    }

    public void SetProgress01(float v)
    {
        current01 = Mathf.Clamp01(v);
        AutoBindIfNeeded();
        Apply(current01);
    }

    private void Apply(float v)
    {
        if (ringFillImage == null) return;

        // 确保是 Filled
        ringFillImage.type = Image.Type.Filled;
        ringFillImage.fillMethod = Image.FillMethod.Radial360;

        ringFillImage.fillAmount = v;
    }
}
