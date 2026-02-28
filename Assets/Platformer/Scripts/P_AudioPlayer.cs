using UnityEngine;

public class P_AudioPlayer : MonoBehaviour
{
    public static P_AudioPlayer Instance { get; private set; }

    [Header("SFX")]
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 1f;

    [Header("Music")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.5f;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.volume = musicVolume;

        if (musicClip != null)
            PlayMusic(musicClip);
    }

    /// <summary> Play a one-shot SFX clip. Usage: P_AudioPlayer.Instance.PlaySFX(clip); </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    /// <summary> Play a SFX clip at a world position. Usage: P_AudioPlayer.Instance.PlaySFXAt(clip, transform.position); </summary>
    public void PlaySFXAt(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, sfxVolume);
    }

    /// <summary> Play or swap the background music. Usage: P_AudioPlayer.Instance.PlayMusic(clip); </summary>
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();

    public void SetSFXVolume(float volume) => sfxVolume = Mathf.Clamp01(volume);
    public void SetMusicVolume(float volume) => musicSource.volume = Mathf.Clamp01(volume);
}
