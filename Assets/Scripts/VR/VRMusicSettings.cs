using TMPro;
using UnityEngine;

public class VRMusicSettings : MonoBehaviour
{
// =====================================
// PlayerPrefs-Schlüssel
// =====================================


private const string MusicEnabledKey = "MusicEnabled"; // speichert, ob Musik an oder aus ist
    private const string MusicVolumeKey = "MusicVolume"; // speichert die Musiklautstärke

    // =====================================
    // Audio
    // =====================================

    [Header("Audio")]
    public AudioSource musicAudioSource; // AudioSource, über die die Musik abgespielt wird
    public bool findMusicManagerAutomatically = true; // sucht MusicManager automatisch, wenn keine AudioSource gesetzt ist
    public bool alwaysStartMusicOn = true; // startet Musik beim Spielstart immer eingeschaltet



    [Header("UI Texts")]
    public TMP_Text musicToggleText; // Text für MUSIC ON/OFF
    public TMP_Text musicVolumeText; 

    // =====================================
    // Lautstärke
    // =====================================

    [Header("Volume")]
    [Range(0f, 1f)]
    public float defaultVolume = 0.35f; // Standardlautstärke

    [Range(0.01f, 0.25f)]
    public float volumeStep = 0.1f; // Schrittgröße beim Lauter- und Leiserstellen



    private bool musicEnabled = true; // aktueller Musik-An/Aus-Status
    private float musicVolume = 0.35f; // aktuelle Musiklautstärke


    // =====================================
    // Initialisierung
    // =====================================

    private void Awake()
    {
        FindMusicAudioSourceIfNeeded(); // sucht die AudioSource früh beim Start
    }

    // =====================================
    // Einstellungen laden und anwenden
    // =====================================

    private void Start()
    {
        LoadSettings(); // lädt gespeicherte Musik-Einstellungen

        if (alwaysStartMusicOn)
        {
            musicEnabled = true;
            PlayerPrefs.SetInt(MusicEnabledKey, 1);
            PlayerPrefs.Save();
        }

        ApplyMusicSettings(); // wendet Musikstatus und Lautstärke an
        RefreshTexts(); 
    }

    // =====================================
    // Musik an- und ausschalten
    // =====================================

    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;

        PlayerPrefs.SetInt(MusicEnabledKey, musicEnabled ? 1 : 0);
        PlayerPrefs.Save();

        ApplyMusicSettings();
        RefreshTexts();

        Debug.Log("VR Music enabled: " + musicEnabled);
    }

    // =====================================
    // Lautstärke erhöhen
    // =====================================

    public void IncreaseMusicVolume()
    {
        SetMusicVolume(musicVolume + volumeStep);
    }

    // =====================================
    // Lautstärke verringern
    // =====================================

    public void DecreaseMusicVolume()
    {
        SetMusicVolume(musicVolume - volumeStep);
    }

    // =====================================
    // Lautstärke setzen
    // =====================================

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume); // begrenzt den Wert zw 0 und 1

        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.Save();

        ApplyMusicSettings();
        RefreshTexts();

        Debug.Log("VR Music volume: " + musicVolume);
    }

    // =====================================
    // Gespeicherte Einstellungen laden
    // =====================================

    private void LoadSettings()
    {
        musicEnabled = PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, defaultVolume);
        musicVolume = Mathf.Clamp01(musicVolume);
    }

    // =====================================
    // AudioSource automatisch suchen
    // =====================================

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

        GameObject musicManagerObject = GameObject.Find("MusicManager"); // sucht das Objekt MusicManager in der Szene

        if (musicManagerObject != null)
        {
            musicAudioSource = musicManagerObject.GetComponent<AudioSource>();
        }

        if (musicAudioSource == null)
        {
            Debug.LogWarning("VRMusicSettings: No AudioSource found. Drag MusicManager AudioSource into Music Audio Source.");
        }
    }

    // =====================================
    // Musik-Einstellungen anwenden
    // =====================================

    private void ApplyMusicSettings()
    {
        FindMusicAudioSourceIfNeeded(); // sucht die AudioSource nochmal, falls sie noch fehlt

        if (musicAudioSource == null)
        {
            Debug.LogWarning("VRMusicSettings: Music Audio Source is missing.");
            return;
        }

        if (musicEnabled)
        {
            musicAudioSource.mute = false;
            musicAudioSource.volume = musicVolume; // setzt die aktuelle Musiklautstärke

            if (musicAudioSource.clip == null)
            {
                Debug.LogWarning("VRMusicSettings: AudioSource has no music clip.");
                return;
            }

            if (!musicAudioSource.isPlaying)
            {
                musicAudioSource.Play(); // startet die Musik, wenn sie noch nicht läuft
            }
        }
        else
        {
            musicAudioSource.Stop(); // stoppt die Musik komplett
            musicAudioSource.mute = true;
            musicAudioSource.volume = 0f;
        }
    }

    // =====================================
    // UI-Texte aktualisieren
    // =====================================

    private void RefreshTexts()
    {
        if (musicToggleText != null)
        {
            musicToggleText.text = musicEnabled ? "MUSIC: ON" : "MUSIC: OFF";
        }

        if (musicVolumeText != null)
        {
            int percent = Mathf.RoundToInt(musicVolume * 100f); // rechnet Lautstärke in Prozent um
            musicVolumeText.text = "VOLUME: " + percent + "%";
        }
    }


}
