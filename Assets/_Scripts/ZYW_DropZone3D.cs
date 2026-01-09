using UnityEngine;

public class ZYW_DropZone3D : MonoBehaviour
{
    public string acceptType;          // "Apple" or "Fish"
    public Transform snapAnchor;       // 吸附点（建议必填）
    public float snapRadius = 0.15f;   // 判定半径（根据你的模型大小调）

    public bool IsFilled { get; private set; }
    public ZYW_Draggable3D CurrentItem { get; private set; }

    public bool TryAccept(ZYW_Draggable3D item)
    {
        if (IsFilled) return false;
        if (item == null) return false;
        if (item.itemType != acceptType) return false;

        // Snap
        Transform anchor = snapAnchor != null ? snapAnchor : transform;
        item.SnapTo(anchor);

        IsFilled = true;
        CurrentItem = item;
        return true;
    }
}
