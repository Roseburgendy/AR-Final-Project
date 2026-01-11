using UnityEngine;
using Vuforia;
using _Scripts.WY.DialogueSystem;

public class Page8TrackingHandler : DefaultObserverEventHandler
{
    [Header("�Ի�����")]
    public string page8DialogueKey = "Scene8";
    
    [Header("���쳡��")]
    public GameObject springScene;
    
    private bool hasTriggered = false;
    
    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();
        
        Debug.Log("=== ʶ�𵽵�8ҳ ===");
        
        if (!hasTriggered)
        {
            InitializePage8();
            hasTriggered = true;
        }
    }
    
    void InitializePage8()
    {
        Debug.Log("��ʼ����8ҳ����");
        
        // ��ʾ���쳡��
        if (springScene != null)
        {
            springScene.SetActive(true);
        }
        
        // ���Ŵ�������
        if (AudioManager.instance != null)
        {
            AudioManager.instance.Play("Spring");
            Debug.Log("���Ŵ�������");
        }
        if (AudioManager.instance == null)
        {
            Debug.Log("Not Found");
        }
        
        // ���ŵ�8ҳ�Ի�
        if (DialogueController.instance != null)
        {
            DialogueController.instance.PlayDialogue(page8DialogueKey);
            Debug.Log($"���ŶԻ���{page8DialogueKey}");
        }
        else
        {
            Debug.LogError("DialogueController �����ڣ�");
        }
    }
    
    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();
        Debug.Log("��8ҳͼƬ��ʧ");
    }
}