using UnityEngine;
using Vuforia;

public class Page7TrackingHandler : DefaultObserverEventHandler
{
    [Header("场景控制器")]
    public SeasonTransition seasonTransition;

    private bool hasTriggered = false;

    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();

        Debug.Log("=== ImageTarget 识别成功 ===");

        // 识别到第7页图片后，初始化场景
        if (!hasTriggered && seasonTransition != null)
        {
            seasonTransition.Initialize();
            hasTriggered = true;
        }
        else if (seasonTransition == null)
        {
            Debug.LogError("SeasonTransition 引用为空！请在 Inspector 中关联 PageManager！");
        }
    }

    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();

        Debug.Log("ImageTarget 丢失");
    }
}