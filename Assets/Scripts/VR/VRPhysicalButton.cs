using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

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

        // Session board menu actions
        OpenCreateMenu,
        JoinGame,
        SelectOnePlayer,
        SelectTwoPlayers,
        SelectThreePlayers,
        BackToMainMenu,
        ConfirmCreateGame
    }

    [Header("References")]
    public NetworkBlackjackTable networkBlackjackTable;

    [Header("Quest Network / Session Menu")]
    public StandaloneQuestNetworkInput questNetworkInput;
    public GameObject mainMenuRoot;
    public GameObject createGameMenuRoot;
    public TMP_Text menuStatusText;

    [Header("Help / Paper Target")]
    public GameObject helpPaper;
    public bool helpPaperStartsHidden = true;

    [Header("Music Settings")]
    public VRMusicSettings musicSettings;

    [Header("Button Settings")]
    public ButtonAction action;
    public float cooldown = 0.7f;

    [Header("Visual Feedback")]
    public Vector3 pressedScaleMultiplier = new Vector3(0.9f, 0.6f, 0.9f);
    public float pressAnimationTime = 0.12f;

    private Vector3 originalScale;
    private float lastPressTime = -999f;
    private Coroutine pressAnimationCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Start()
    {
        if (action == ButtonAction.HelpToggle &&
            helpPaper != null &&
            helpPaperStartsHidden)
        {
            helpPaper.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsControllerOrHand(other.gameObject))
        {
            PressButton();
        }
    }

    public void PressButton()
    {
        if (Time.time - lastPressTime < cooldown)
        {
            return;
        }

        lastPressTime = Time.time;

        PlayButtonAnimation();

        switch (action)
        {
            case ButtonAction.HelpToggle:
                ToggleHelpPaper();
                Debug.Log("VR Button pressed: HELP / SETTINGS TOGGLE");
                break;

            case ButtonAction.ClosePaper:
                ClosePaper();
                Debug.Log("VR Button pressed: CLOSE PAPER");
                break;

            case ButtonAction.MainMenu:
                Debug.Log("VR Button pressed: MAIN MENU");
                StartCoroutine(RestartToMainMenuRoutine());
                break;

            case ButtonAction.Music:
                ToggleMusic();
                Debug.Log("VR Button pressed: MUSIC TOGGLE");
                break;

            case ButtonAction.MusicVolumeDown:
                ChangeMusicVolumeDown();
                Debug.Log("VR Button pressed: MUSIC VOLUME DOWN");
                break;

            case ButtonAction.MusicVolumeUp:
                ChangeMusicVolumeUp();
                Debug.Log("VR Button pressed: MUSIC VOLUME UP");
                break;

            case ButtonAction.Hit:
                if (networkBlackjackTable == null)
                {
                    Debug.LogWarning("VRPhysicalButton: No NetworkBlackjackTable assigned.");
                    return;
                }

                networkBlackjackTable.HitButton();
                Debug.Log("VR Button pressed: HIT");
                break;

            case ButtonAction.Stand:
                if (networkBlackjackTable == null)
                {
                    Debug.LogWarning("VRPhysicalButton: No NetworkBlackjackTable assigned.");
                    return;
                }

                networkBlackjackTable.StandButton();
                Debug.Log("VR Button pressed: STAND");
                break;

            case ButtonAction.NewRound:
                if (networkBlackjackTable == null)
                {
                    Debug.LogWarning("VRPhysicalButton: No NetworkBlackjackTable assigned.");
                    return;
                }

                networkBlackjackTable.StartRoundButton();
                Debug.Log("VR Button pressed: NEW ROUND");
                break;

            case ButtonAction.OpenCreateMenu:
                ShowCreateGameMenu();
                Debug.Log("VR Button pressed: OPEN CREATE MENU");
                break;

            case ButtonAction.JoinGame:
                JoinGame();
                Debug.Log("VR Button pressed: JOIN GAME");
                break;

            case ButtonAction.SelectOnePlayer:
                SelectPlayerCount(1);
                Debug.Log("VR Button pressed: SELECT 1 PLAYER");
                break;

            case ButtonAction.SelectTwoPlayers:
                SelectPlayerCount(2);
                Debug.Log("VR Button pressed: SELECT 2 PLAYERS");
                break;

            case ButtonAction.SelectThreePlayers:
                SelectPlayerCount(3);
                Debug.Log("VR Button pressed: SELECT 3 PLAYERS");
                break;

            case ButtonAction.BackToMainMenu:
                ShowMainMenu();
                Debug.Log("VR Button pressed: BACK TO MAIN MENU");
                break;

            case ButtonAction.ConfirmCreateGame:
                ConfirmCreateGame();
                Debug.Log("VR Button pressed: CONFIRM CREATE GAME");
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

        int selectedPlayers = 3;

        if (questNetworkInput != null)
        {
            selectedPlayers = questNetworkInput.selectedPlayerCount;
        }

        SetMenuStatus(
            "CREATE GAME\n\n" +
            "Selected Players: " + selectedPlayers + "\n\n" +
            "Choose 1, 2 or 3 players.\n" +
            "Then press CREATE."
        );
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

        SetMenuStatus("Choose CREATE GAME or JOIN GAME.");
    }

    private void SelectPlayerCount(int playerCount)
    {
        if (questNetworkInput == null)
        {
            Debug.LogWarning("VRPhysicalButton: No StandaloneQuestNetworkInput assigned.");
            return;
        }

        questNetworkInput.selectedPlayerCount = Mathf.Clamp(playerCount, 1, 3);

        SetMenuStatus(
            "Selected Players: " + questNetworkInput.selectedPlayerCount + "\n\n" +
            "Press CREATE to start the game.\n" +
            "Press BACK to return."
        );
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
    }

    private void SetMenuStatus(string message)
    {
        Debug.Log("[VRPhysicalButton] " + message);

        if (menuStatusText != null)
        {
            menuStatusText.text = message;
        }
    }

    private void ToggleHelpPaper()
    {
        if (helpPaper == null)
        {
            Debug.LogWarning("VRPhysicalButton: Help/Paper target is not assigned.");
            return;
        }

        helpPaper.SetActive(!helpPaper.activeSelf);
    }

    private void ClosePaper()
    {
        if (helpPaper == null)
        {
            Debug.LogWarning("VRPhysicalButton: Help/Paper target is not assigned.");
            return;
        }

        helpPaper.SetActive(false);
    }

    private void ToggleMusic()
    {
        VRMusicSettings settings = GetMusicSettings();

        if (settings == null)
        {
            Debug.LogWarning("VRPhysicalButton: No VRMusicSettings assigned.");
            return;
        }

        settings.ToggleMusic();
    }

    private void ChangeMusicVolumeDown()
    {
        VRMusicSettings settings = GetMusicSettings();

        if (settings == null)
        {
            Debug.LogWarning("VRPhysicalButton: No VRMusicSettings assigned.");
            return;
        }

        settings.DecreaseMusicVolume();
    }

    private void ChangeMusicVolumeUp()
    {
        VRMusicSettings settings = GetMusicSettings();

        if (settings == null)
        {
            Debug.LogWarning("VRPhysicalButton: No VRMusicSettings assigned.");
            return;
        }

        settings.IncreaseMusicVolume();
    }

    private VRMusicSettings GetMusicSettings()
    {
        if (musicSettings != null)
        {
            return musicSettings;
        }

        return FindFirstObjectByType<VRMusicSettings>();
    }

    private IEnumerator RestartToMainMenuRoutine()
    {
        yield return new WaitForSeconds(0.15f);

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
            yield return new WaitForSeconds(0.25f);
        }

        Time.timeScale = 1f;

        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }

    private void PlayButtonAnimation()
    {
        if (pressAnimationCoroutine != null)
        {
            StopCoroutine(pressAnimationCoroutine);
        }

        pressAnimationCoroutine = StartCoroutine(PlayPressAnimation());
    }

    private bool IsControllerOrHand(GameObject obj)
    {
        string objectName = obj.name.ToLower();

        return objectName.Contains("controller") ||
               objectName.Contains("hand");
    }

    private IEnumerator PlayPressAnimation()
    {
        transform.localScale = new Vector3(
            originalScale.x * pressedScaleMultiplier.x,
            originalScale.y * pressedScaleMultiplier.y,
            originalScale.z * pressedScaleMultiplier.z
        );

        yield return new WaitForSeconds(pressAnimationTime);

        transform.localScale = originalScale;
        pressAnimationCoroutine = null;
    }
}