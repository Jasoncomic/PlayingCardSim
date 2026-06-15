using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TitleScreenUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject titleScreenPanel;
    public GameObject gameUIPanel;

    [Header("Title Screen")]
    public GameObject buttonContainer;

    [Header("Popups")]
    public GameObject createGamePopup;
    public GameObject joinGamePopup;
    public GameObject settingsPopup;

    [Header("Join Game")]
    public TMP_InputField gameCodeInputField;

    [Header("Settings")]
    public TMP_Text musicToggleText;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Blackjack")]
    public BlackjackUnityTestController blackjackController;

    private const string MusicEnabledKey = "MusicEnabled";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SfxVolumeKey = "SFXVolume";

    private int selectedPlayerCount = 2;
    private bool musicEnabled = true;

    private void Start()
    {
        FindBlackjackControllerIfMissing();
        LoadAudioSettings();
    }

    private void FindBlackjackControllerIfMissing()
    {
        if (blackjackController == null)
        {
            blackjackController = FindFirstObjectByType<BlackjackUnityTestController>();
        }

        if (blackjackController == null)
        {
            Debug.LogWarning("BlackjackUnityTestController was not found in the scene.");
        }
    }

    private void LoadAudioSettings()
    {
        musicEnabled = PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;

        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.35f);
        float sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, 0.75f);

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(musicVolume);
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);
            sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
        }

        ApplyMusicSettings();
    }

    public void QuickStart()
    {
        FindBlackjackControllerIfMissing();

        titleScreenPanel.SetActive(false);
        gameUIPanel.SetActive(true);

        if (createGamePopup != null)
            createGamePopup.SetActive(false);

        if (joinGamePopup != null)
            joinGamePopup.SetActive(false);

        if (settingsPopup != null)
            settingsPopup.SetActive(false);

        if (blackjackController != null)
        {
            blackjackController.SetPlayerCount(1);
            blackjackController.RestartGame();
        }

        Debug.Log("Quick Start started with 1 player.");
    }

    public void ShowCreateGame()
    {
        buttonContainer.SetActive(false);

        if (createGamePopup != null)
            createGamePopup.SetActive(true);

        if (joinGamePopup != null)
            joinGamePopup.SetActive(false);

        if (settingsPopup != null)
            settingsPopup.SetActive(false);

        selectedPlayerCount = 2;
        Debug.Log("Create Game opened. Default players: 2");
    }

    public void SelectTwoPlayers()
    {
        selectedPlayerCount = 2;
        Debug.Log("Selected players: 2");
    }

    public void SelectThreePlayers()
    {
        selectedPlayerCount = 3;
        Debug.Log("Selected players: 3");
    }

    public void CreateLobby()
    {
        FindBlackjackControllerIfMissing();

        Debug.Log("Create game with players: " + selectedPlayerCount);

        if (createGamePopup != null)
            createGamePopup.SetActive(false);

        titleScreenPanel.SetActive(false);
        gameUIPanel.SetActive(true);

        if (blackjackController != null)
        {
            blackjackController.SetPlayerCount(selectedPlayerCount);
            blackjackController.RestartGame();
        }
        else
        {
            Debug.LogError("Cannot create lobby because BlackjackUnityTestController is missing.");
        }
    }

    public void ShowJoinGame()
    {
        buttonContainer.SetActive(false);

        if (createGamePopup != null)
            createGamePopup.SetActive(false);

        if (joinGamePopup != null)
            joinGamePopup.SetActive(true);
        else
            Debug.LogError("JoinGamePopup is not assigned!");

        if (settingsPopup != null)
            settingsPopup.SetActive(false);

        Debug.Log("Join Game opened");
    }

    public void JoinGame()
    {
        FindBlackjackControllerIfMissing();

        string gameCode = "";

        if (gameCodeInputField != null)
            gameCode = gameCodeInputField.text;

        Debug.Log("Join game with code/host: " + gameCode);

        if (joinGamePopup != null)
            joinGamePopup.SetActive(false);

        titleScreenPanel.SetActive(false);
        gameUIPanel.SetActive(true);

        if (blackjackController != null)
        {
            blackjackController.SetPlayerCount(1);
            blackjackController.RestartGame();
        }
    }

    public void ShowSettings()
    {
        buttonContainer.SetActive(false);

        if (createGamePopup != null)
            createGamePopup.SetActive(false);

        if (joinGamePopup != null)
            joinGamePopup.SetActive(false);

        if (settingsPopup != null)
            settingsPopup.SetActive(true);
        else
            Debug.LogError("SettingsPopup is not assigned!");

        Debug.Log("Settings opened");
    }

    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;

        PlayerPrefs.SetInt(MusicEnabledKey, musicEnabled ? 1 : 0);
        PlayerPrefs.Save();

        ApplyMusicSettings();

        Debug.Log("Music enabled: " + musicEnabled);
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, volume);
        PlayerPrefs.Save();

        ApplyMusicSettings();
    }

    public void SetSfxVolume(float volume)
    {
        PlayerPrefs.SetFloat(SfxVolumeKey, volume);
        PlayerPrefs.Save();

        // For now this only saves the value.
        // Later, when we add button/card sounds, they can read this value.
        Debug.Log("SFX volume saved: " + volume);
    }

    private void ApplyMusicSettings()
    {
        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.35f);
        float targetVolume = musicEnabled ? musicVolume : 0f;

        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.SetVolume(targetVolume);
        }
        else
        {
            Debug.LogWarning("MusicManager not found. Add a MusicManager object to the scene and assign your MP3 clip.");
        }

        if (musicToggleText != null)
        {
            musicToggleText.text = musicEnabled ? "MUSIC: ON" : "MUSIC: OFF";
        }
    }

    public void BackToTitleMenu()
    {
        if (createGamePopup != null)
            createGamePopup.SetActive(false);

        if (joinGamePopup != null)
            joinGamePopup.SetActive(false);

        if (settingsPopup != null)
            settingsPopup.SetActive(false);

        buttonContainer.SetActive(true);

        Debug.Log("Back to title menu");
    }

    public void ExitGame()
    {
        Debug.Log("Exit Game");

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
    Application.Quit();
    #endif
    }

}