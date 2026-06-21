using TMPro;
using UnityEngine;

public class VRPhysicalButton : MonoBehaviour
{
    public enum ButtonAction
    {
        Hit,
        Stand,
        NewRound,

        HelpToggle,
        MainMenu,
        Music,
        ClosePaper,
        MusicVolumeDown,
        MusicVolumeUp,

        OpenCreateMenu,
        JoinGame,
        SelectOnePlayer,
        SelectTwoPlayers,
        SelectThreePlayers,
        BackToMainMenu,
        ConfirmCreateGame,

        NewGame
    }

    [Header("Button Action")]
    public ButtonAction action;

    [Header("Blackjack")]
    public NetworkBlackjackTable networkBlackjackTable;

    [Header("Quest Network Menu")]
    public StandaloneQuestNetworkInput questNetworkInput;
    public GameObject mainMenuRoot;
    public GameObject createGameMenuRoot;
    public TMP_Text menuStatusText;

    [Header("Paper / Menu UI")]
    public GameObject helpPaperRoot;
    public GameObject settingsPaperRoot;

    [Header("Music")]
    public AudioSource musicAudioSource;
    [Range(0f, 1f)]
    public float volumeStep = 0.1f;

    [Header("Press Settings")]
    public Transform visualTarget;
    public Vector3 pressedLocalOffset = new Vector3(0f, -0.015f, 0f);
    public float pressDuration = 0.12f;
    public float cooldown = 0.7f;

    private Vector3 originalLocalPosition;
    private bool hasOriginalPosition;
    private bool isPressed;
    private float lastPressTime = -999f;

    private void Awake()
    {
        if (visualTarget == null)
        {
            visualTarget = transform;
        }

        originalLocalPosition = visualTarget.localPosition;
        hasOriginalPosition = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryPress();
    }

    public void Press()
    {
        TryPress();
    }

    public void PressButton()
    {
        TryPress();
    }

    public void TriggerButton()
    {
        TryPress();
    }

    private void TryPress()
    {
        if (Time.time - lastPressTime < cooldown)
        {
            return;
        }

        lastPressTime = Time.time;

        PlayPressAnimation();
        ExecuteAction();
    }

    private void PlayPressAnimation()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(PressAnimationRoutine());
    }

    private System.Collections.IEnumerator PressAnimationRoutine()
    {
        if (visualTarget == null)
        {
            yield break;
        }

        if (!hasOriginalPosition)
        {
            originalLocalPosition = visualTarget.localPosition;
            hasOriginalPosition = true;
        }

        if (isPressed)
        {
            yield break;
        }

        isPressed = true;

        visualTarget.localPosition = originalLocalPosition + pressedLocalOffset;

        yield return new WaitForSeconds(pressDuration);

        visualTarget.localPosition = originalLocalPosition;
        isPressed = false;
    }

    private void ExecuteAction()
    {
        switch (action)
        {
            case ButtonAction.Hit:
                if (networkBlackjackTable != null)
                {
                    networkBlackjackTable.HitButton();
                }
                break;

            case ButtonAction.Stand:
                if (networkBlackjackTable != null)
                {
                    networkBlackjackTable.StandButton();
                }
                break;

            case ButtonAction.NewRound:
                if (networkBlackjackTable != null)
                {
                    networkBlackjackTable.StartRoundButton();
                }
                break;

            case ButtonAction.NewGame:
                if (networkBlackjackTable != null)
                {
                    networkBlackjackTable.NewGameButton();
                }
                break;

            case ButtonAction.HelpToggle:
                ToggleObject(helpPaperRoot);
                break;

            case ButtonAction.MainMenu:
                ShowMainMenu();
                break;

            case ButtonAction.Music:
                ToggleMusic();
                break;

            case ButtonAction.ClosePaper:
                ClosePapers();
                break;

            case ButtonAction.MusicVolumeDown:
                ChangeMusicVolume(-volumeStep);
                break;

            case ButtonAction.MusicVolumeUp:
                ChangeMusicVolume(volumeStep);
                break;

            case ButtonAction.OpenCreateMenu:
                ShowCreateGameMenu();
                break;

            case ButtonAction.JoinGame:
                JoinGame();
                break;

            case ButtonAction.SelectOnePlayer:
                SelectPlayerCount(1);
                break;

            case ButtonAction.SelectTwoPlayers:
                SelectPlayerCount(2);
                break;

            case ButtonAction.SelectThreePlayers:
                SelectPlayerCount(3);
                break;

            case ButtonAction.BackToMainMenu:
                ShowMainMenu();
                break;

            case ButtonAction.ConfirmCreateGame:
                ConfirmCreateGame();
                break;
        }
    }

    private void ShowCreateGameMenu()
    {
        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(false);
        }

        if (createGameMenuRoot != null)
        {
            createGameMenuRoot.SetActive(true);
        }

        if (questNetworkInput != null)
        {
            SetMenuStatus("Selected Players: " + questNetworkInput.selectedPlayerCount);
        }
        else
        {
            SetMenuStatus("Select player count.");
        }
    }

    private void ShowMainMenu()
    {
        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(true);
        }

        if (createGameMenuRoot != null)
        {
            createGameMenuRoot.SetActive(false);
        }

        SetMenuStatus("Create Game or Join Game");
    }

    private void SelectPlayerCount(int playerCount)
    {
        if (questNetworkInput == null)
        {
            Debug.LogWarning("VRPhysicalButton: No StandaloneQuestNetworkInput assigned.");
            return;
        }

        questNetworkInput.selectedPlayerCount = Mathf.Clamp(playerCount, 1, 3);

        SetMenuStatus("Selected Players: " + questNetworkInput.selectedPlayerCount);
    }

    private void ConfirmCreateGame()
    {
        if (questNetworkInput == null)
        {
            Debug.LogWarning("VRPhysicalButton: No StandaloneQuestNetworkInput assigned.");
            return;
        }

        SetMenuStatus(
            "Creating game with " +
            questNetworkInput.selectedPlayerCount +
            " player(s)..."
        );

        questNetworkInput.StartQuestHost();

        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(false);
        }

        if (createGameMenuRoot != null)
        {
            createGameMenuRoot.SetActive(false);
        }
    }

    private void JoinGame()
    {
        if (questNetworkInput == null)
        {
            Debug.LogWarning("VRPhysicalButton: No StandaloneQuestNetworkInput assigned.");
            return;
        }

        SetMenuStatus("Joining game...");

        questNetworkInput.JoinQuestHost();

        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(false);
        }

        if (createGameMenuRoot != null)
        {
            createGameMenuRoot.SetActive(false);
        }
    }

    private void SetMenuStatus(string message)
    {
        Debug.Log(message);

        if (menuStatusText != null)
        {
            menuStatusText.text = message;
        }
    }

    private void ToggleObject(GameObject target)
    {
        if (target != null)
        {
            target.SetActive(!target.activeSelf);
        }
    }

    private void ClosePapers()
    {
        if (helpPaperRoot != null)
        {
            helpPaperRoot.SetActive(false);
        }

        if (settingsPaperRoot != null)
        {
            settingsPaperRoot.SetActive(false);
        }
    }

    private void ToggleMusic()
    {
        if (musicAudioSource == null)
        {
            return;
        }

        musicAudioSource.mute = !musicAudioSource.mute;
    }

    private void ChangeMusicVolume(float amount)
    {
        if (musicAudioSource == null)
        {
            return;
        }

        musicAudioSource.volume = Mathf.Clamp01(musicAudioSource.volume + amount);
    }
}