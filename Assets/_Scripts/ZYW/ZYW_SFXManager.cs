using UnityEngine;

public class ZYW_SFXManager : MonoBehaviour
{
    public static ZYW_SFXManager I { get; private set; }

    [Header("Audio Source")]
    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip appleSfx;
    public AudioClip fishSfx;

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
    }

    public void PlayApple()
    {
        if (sfxSource != null && appleSfx != null)
            sfxSource.PlayOneShot(appleSfx);
    }

    public void PlayFish()
    {
        if (sfxSource != null && fishSfx != null)
            sfxSource.PlayOneShot(fishSfx);
    }
}
