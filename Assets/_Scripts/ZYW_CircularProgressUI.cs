using UnityEngine;
using UnityEngine.UI;

public class ZYW_CircularProgressUI : MonoBehaviour
{
    [Range(0f, 1f)]
    public float fill01 = 0f;

    private Image img;

    private void Awake()
    {
        img = GetComponent<Image>();
        if (img == null)
        {
            Debug.LogError("[CircularProgressUI] Missing Image component.");
        }
    }

    public void SetProgress01(float v)
    {
        fill01 = Mathf.Clamp01(v);
        if (img != null) img.fillAmount = fill01;
    }
}
