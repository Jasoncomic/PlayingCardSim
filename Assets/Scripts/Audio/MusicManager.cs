using System.Collections;
using UnityEngine;

/// <summary>
/// Simple background music manager for menu/game scenes.
///
/// How to use in Unity:
/// 1. Create an empty GameObject called "MusicManager".
/// 2. Add this script to it.
/// 3. Drag your western theme AudioClip into the "Music Clip" field.
/// 4. Enable "Play On Start" and "Loop" in the Inspector.
/// 5. Keep one MusicManager in the first scene only. It will persist across scene changes.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.35f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;

    [Header("Fade")]
    [SerializeField] private bool fadeInOnStart = true;
    [SerializeField] private float fadeDuration = 1.5f;

    private AudioSource audioSource;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = loop;
        audioSource.clip = musicClip;
        audioSource.volume = fadeInOnStart ? 0f : volume;
        audioSource.spatialBlend = 0f; // 2D audio, good for background music in VR.
    }

    private void Start()
    {
        if (playOnStart && musicClip != null)
        {
            PlayMusic();
        }

            Debug.Log("MusicManager started");

    if (playOnStart && musicClip != null)
    {
        PlayMusic();
    }
    }

    public void PlayMusic()
    {
        if (musicClip == null)
        {
            Debug.LogWarning("MusicManager: No music clip assigned.");
            return;
        }

        audioSource.clip = musicClip;
        audioSource.loop = loop;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        if (fadeInOnStart)
        {
            FadeTo(volume, fadeDuration);
        }
        else
        {
            audioSource.volume = volume;
        }

        Debug.Log("Playing music");
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);

        if (audioSource.isPlaying)
        {
            audioSource.volume = volume;
        }
    }

    public void FadeTo(float targetVolume, float duration)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeVolumeRoutine(Mathf.Clamp01(targetVolume), Mathf.Max(0f, duration)));
    }

    private IEnumerator FadeVolumeRoutine(float targetVolume, float duration)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        if (duration <= 0f)
        {
            audioSource.volume = targetVolume;
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }
}
