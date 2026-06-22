using TMPro;
using UnityEngine;

public class VRMusicSettings : MonoBehaviour
{
    private const string MusicEnabledKey = "MusicEnabled";
    private const string MusicVolumeKey = "MusicVolume";

    [Header("Audio")]
    public AudioSource musicAudioSource;
    public bool findMusicManagerAutomatically = true;
    public bool alwaysStartMusicOn = true;

    [Header("UI Texts")]
    public TMP_Text musicToggleText;
    public TMP_Text musicVolumeText;

    [Header("Volume")]
    [Range(0f, 1f)]
    public float defaultVolume = 0.35f;

    [Range(0.01f, 0.25f)]
    public float volumeStep = 0.1f;

    private bool musicEnabled = true;
    private float musicVolume = 0.35f;

    private void Awake()
    {
        FindMusicAudioSourceIfNeeded();
    }

    private void Start()
    {
        LoadSettings();

        if (alwaysStartMusicOn)
        {
            musicEnabled = true;
            PlayerPrefs.SetInt(MusicEnabledKey, 1);
            PlayerPrefs.Save();
        }

        ApplyMusicSettings();
        RefreshTexts();
    }

    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;

        PlayerPrefs.SetInt(MusicEnabledKey, musicEnabled ? 1 : 0);
        PlayerPrefs.Save();

        ApplyMusicSettings();
        RefreshTexts();

        Debug.Log("VR Music enabled: " + musicEnabled);
    }

    public void IncreaseMusicVolume()
    {
        SetMusicVolume(musicVolume + volumeStep);
    }

    public void DecreaseMusicVolume()
    {
        SetMusicVolume(musicVolume - volumeStep);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.Save();

        ApplyMusicSettings();
        RefreshTexts();

        Debug.Log("VR Music volume: " + musicVolume);
    }

    private void LoadSettings()
    {
        musicEnabled = PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, defaultVolume);
        musicVolume = Mathf.Clamp01(musicVolume);
    }

    private void FindMusicAudioSourceIfNeeded()
    {
        if (musicAudioSource != null)
        {
            return;
        }

        if (!findMusicManagerAutomatically)
        {
            return;
        }

        GameObject musicManagerObject = GameObject.Find("MusicManager");

        if (musicManagerObject != null)
        {
            musicAudioSource = musicManagerObject.GetComponent<AudioSource>();
        }

        if (musicAudioSource == null)
        {
            Debug.LogWarning("VRMusicSettings: No AudioSource found. Drag MusicManager AudioSource into Music Audio Source.");
        }
    }

    private void ApplyMusicSettings()
    {
        FindMusicAudioSourceIfNeeded();

        if (musicAudioSource == null)
        {
            Debug.LogWarning("VRMusicSettings: Music Audio Source is missing.");
            return;
        }

        if (musicEnabled)
        {
            musicAudioSource.mute = false;
            musicAudioSource.volume = musicVolume;

            if (musicAudioSource.clip == null)
            {
                Debug.LogWarning("VRMusicSettings: AudioSource has no music clip.");
                return;
            }

            if (!musicAudioSource.isPlaying)
            {
                musicAudioSource.Play();
            }
        }
        else
        {
            musicAudioSource.Stop();
            musicAudioSource.mute = true;
            musicAudioSource.volume = 0f;
        }
    }

    private void RefreshTexts()
    {
        if (musicToggleText != null)
        {
            musicToggleText.text = musicEnabled ? "MUSIC: ON" : "MUSIC: OFF";
        }

        if (musicVolumeText != null)
        {
            int percent = Mathf.RoundToInt(musicVolume * 100f);
            musicVolumeText.text = "VOLUME: " + percent + "%";
        }
    }
}