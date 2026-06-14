using UnityEngine;
using TMPro;

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

    [Header("Blackjack")]
    public BlackjackUnityTestController blackjackController;

    private int selectedPlayerCount = 2;
    private bool musicEnabled = true;

    private void Start()
    {
        FindBlackjackControllerIfMissing();
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

        // For prototype: joining starts a local 1-player game.
        // Real network joining can be implemented later.
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

        if (musicToggleText != null)
        {
            musicToggleText.text = musicEnabled ? "MUSIC: ON" : "MUSIC: OFF";
        }

        Debug.Log("Music enabled: " + musicEnabled);
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
        Application.Quit();
    }
}