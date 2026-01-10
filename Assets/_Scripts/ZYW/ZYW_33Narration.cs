using System;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class ZYW_33Narration : MonoBehaviour
{
    [Serializable]
    public class TargetAudioMap
    {
        [Header("Target")]
        public ObserverBehaviour target;

        [Header("Audio")]
        public AudioSource audioSource;   // 可共用同一个 AudioSource，也可每个目标独立一个
        public AudioClip clip;
        public bool loop = false;

        [Header("Policy")]
        public bool playOnlyOnce = true;

        [NonSerialized] public bool hasPlayed;
    }

    [Header("Mappings (3 targets = add 3 entries)")]
    public List<TargetAudioMap> mappings = new List<TargetAudioMap>();

    private void Awake()
    {
        // 订阅所有 target 的状态变化事件
        foreach (var m in mappings)
        {
            if (m == null || m.target == null) continue;
            m.target.OnTargetStatusChanged += OnTargetStatusChanged;
        }
    }

    private void OnDestroy()
    {
        foreach (var m in mappings)
        {
            if (m == null || m.target == null) continue;
            m.target.OnTargetStatusChanged -= OnTargetStatusChanged;
        }
    }

    private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        bool tracked =
            status.Status == Status.TRACKED ||
            status.Status == Status.EXTENDED_TRACKED ||
            status.Status == Status.LIMITED;

        if (!tracked) return;

        // 找到对应 mapping
        for (int i = 0; i < mappings.Count; i++)
        {
            var m = mappings[i];
            if (m == null || m.target == null) continue;
            if (m.target != behaviour) continue;

            if (m.playOnlyOnce && m.hasPlayed) return;

            PlayMapping(m);
            return;
        }
    }

    private void PlayMapping(TargetAudioMap m)
    {
        if (m.audioSource == null || m.clip == null) return;

        m.hasPlayed = true;

        // 如果你希望“识别 A 时打断其他旁白”，就先停掉所有 AudioSource
        // StopAllNarrations();

        m.audioSource.Stop();
        m.audioSource.clip = m.clip;
        m.audioSource.loop = m.loop;
        m.audioSource.Play();
    }

    public void StopAllNarrations()
    {
        foreach (var m in mappings)
        {
            if (m?.audioSource == null) continue;
            if (m.audioSource.isPlaying) m.audioSource.Stop();
        }
    }

    // 可选：在需要时（例如重新开始关卡）手动重置“只播一次”状态
    public void ResetPlayedFlags()
    {
        foreach (var m in mappings)
        {
            if (m == null) continue;
            m.hasPlayed = false;
        }
    }
}
