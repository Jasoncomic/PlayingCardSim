using System.Collections;
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
        MusicVolumeUp
    }

    [Header("References")]
    public NetworkBlackjackTable networkBlackjackTable;

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