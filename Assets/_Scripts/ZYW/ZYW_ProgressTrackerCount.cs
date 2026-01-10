using UnityEngine;
using UnityEngine.UI;

public class ZYW_ProgressCounter : MonoBehaviour
{
    [Header("Targets")]
    public int applesRequired = 5;
    public int fishRequired = 2;

    [Header("UI")]
    public Slider progressSlider; // 0..1
    public Text progressText;     // ¿ÉÑ¡£º0/7

    private int _apples;
    private int _fish;

    public void ResetCount()
    {
        _apples = 0;
        _fish = 0;
        UpdateUI();
    }

    public bool IsCompleted(string itemType)
    {
        if (itemType == "apple") return _apples >= applesRequired;
        if (itemType == "fish") return _fish >= fishRequired;
        return false;
    }

    public bool TryAdd(string itemType)
    {
        if (itemType == "apple")
        {
            if (_apples >= applesRequired) return false;
            _apples++;
        }
        else if (itemType == "fish")
        {
            if (_fish >= fishRequired) return false;
            _fish++;
        }
        else
        {
            return false;
        }

        UpdateUI();
        return true;
    }

    private void UpdateUI()
    {
        int done = Mathf.Min(_apples, applesRequired) + Mathf.Min(_fish, fishRequired);
        int total = Mathf.Max(1, applesRequired + fishRequired);

        if (progressSlider != null) progressSlider.value = (float)done / total;
        if (progressText != null) progressText.text = $"{done}/{total}";
    }
}
