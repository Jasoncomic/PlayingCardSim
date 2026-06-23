using TMPro;
using UnityEngine;

public class VRPhysicalButton : MonoBehaviour
{
// =====================================
// Button-Aktionen
// =====================================


public enum ButtonAction
    {
        Hit, // Karte ziehen
        Stand, 
        NewRound, 

        HelpToggle, // Hilfe-Paper öffnen/schließen
        SettingsToggle, 
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

    // =====================================
    // Button Action
    // =====================================

    [Header("Button Action")]
    public ButtonAction action; // Aktion, die dieser Button ausführt

    

    [Header("Blackjack")]
    public NetworkBlackjackTable networkBlackjackTable; // Referenz zum Netzwerk-Blackjack-Tisch



    [Header("Quest Network Menu")]
    public StandaloneQuestNetworkInput questNetworkInput; // steuert Host, Join und Spieleranzahl
    public GameObject mainMenuRoot; // Hauptmenü-Objekt
    public GameObject createGameMenuRoot; // Create-Game-Menü-Objekt
    public TMP_Text menuStatusText; // Textanzeige für Menüstatus

    // =====================================
    // Paper / Menu UI
    // =====================================

    [Header("Paper / Menu UI")]
    public GameObject helpPaperRoot; // Hilfe-Paper
    public GameObject settingsPaperRoot; // Settings-Paper

    // =====================================
    // Music
    // =====================================

    [Header("Music")]
    public VRMusicSettings musicSettings; // Musiksteuerung über VRMusicSettings
    public AudioSource musicAudioSource; // Fallback-AudioSource

    [Range(0f, 1f)]
    public float volumeStep = 0.1f; // Schrittgröße für Lautstärkeänderung

    // =====================================
    // Press Settings
    // =====================================

    [Header("Press Settings")]
    public Transform visualTarget; // sichtbares Objekt, das beim Drücken bewegt wird
    public Vector3 pressedLocalOffset = new Vector3(0f, -0.015f, 0f); // lokale Verschiebung beim Drücken
    public float pressDuration = 0.12f; // Dauer der Druckanimation
    public float cooldown = 0.7f; // Wartezeit bis zum nächsten Druck

    // =====================================
    // Interner Button-Zustand
    // =====================================

    private Vector3 originalLocalPosition; // ursprüngliche Position des Button-Visuals
    private bool hasOriginalPosition; // merkt, ob die Originalposition gespeichert wurde
    private bool isPressed; // verhindert doppelte Druckanimation
    private float lastPressTime = -999f; // Zeitpunkt des letzten Drückens

    // =====================================
    // Initialisierung
    // =====================================

    private void Awake()
    {
        if (visualTarget == null)
        {
            visualTarget = transform; // nutzt eigenes Transform, wenn kein Visual gesetzt ist
        }

        originalLocalPosition = visualTarget.localPosition; // speichert Startposition
        hasOriginalPosition = true; // bestätigt gespeicherte Position
    }

    // =====================================
    // Physischer Trigger-Kontakt
    // =====================================

    private void OnTriggerEnter(Collider other)
    {
        TryPress(); // versucht Buttondruck auszulösen
    }

    // =====================================
    // Button drücken
    // =====================================

    public void Press()
    {
        TryPress(); // externe Press-Funktion
    }

    public void PressButton()
    {
        TryPress(); // Press-Funktion für Ray/Button-Systeme
    }

    public void TriggerButton()
    {
        TryPress(); // alternative Trigger-Funktion
    }

    // =====================================
    // Buttondruck prüfen
    // =====================================

    private void TryPress()
    {
        if (Time.time - lastPressTime < cooldown) // verhindert zu schnelles mehrfaches Drücken
        {
            return;
        }

        lastPressTime = Time.time; 

        PlayPressAnimation(); // startet visuelle Druckanimation
        ExecuteAction(); // führt ausgewählte Aktion aus
    }

    // =====================================
    // Druckanimation starten
    // =====================================

    private void PlayPressAnimation()
    {
        if (!gameObject.activeInHierarchy) // keine Animation, wenn Objekt inaktiv ist
        {
            return;
        }

        StopAllCoroutines(); 
        StartCoroutine(PressAnimationRoutine()); 
    }

    // =====================================
    // Druckanimation ausführen
    // =====================================

    private System.Collections.IEnumerator PressAnimationRoutine()
    {
        if (visualTarget == null) // bricht ab, wenn kein Visual vorhanden ist
        {
            yield break;
        }

        if (!hasOriginalPosition) // speichert Position nach, falls nötig
        {
            originalLocalPosition = visualTarget.localPosition;
            hasOriginalPosition = true;
        }

        if (isPressed) // verhindert doppelte Animation
        {
            yield break;
        }

        isPressed = true; // markiert Button als gedrückt

        visualTarget.localPosition = originalLocalPosition + pressedLocalOffset; // bewegt Button nach unten

        yield return new WaitForSeconds(pressDuration); // wartet kurz in gedrückter Position

        visualTarget.localPosition = originalLocalPosition; // setzt Button zurück
        isPressed = false; // gibt Button wieder frei
    }

    // =====================================
    // Ausgewählte Aktion ausführen
    // =====================================

    private void ExecuteAction()
    {
        switch (action)
        {
            case ButtonAction.Hit:
                if (networkBlackjackTable != null)
                {
                    networkBlackjackTable.HitButton(); // Karte ziehen
                }
                break;

            case ButtonAction.Stand:
                if (networkBlackjackTable != null)
                {
                    networkBlackjackTable.StandButton(); // Zug beenden
                }
                break;

            case ButtonAction.NewRound:
                if (networkBlackjackTable != null)
                {
                    networkBlackjackTable.StartRoundButton(); // neue Runde starten
                }
                break;

            case ButtonAction.NewGame:
                if (networkBlackjackTable != null)
                {
                    networkBlackjackTable.NewGameButton(); // komplett neues Spiel starten
                }
                break;

            case ButtonAction.HelpToggle:
                ToggleObject(helpPaperRoot); // Hilfe-Paper umschalten
                break;

            case ButtonAction.SettingsToggle:
                ToggleObject(settingsPaperRoot); // Settings-Paper umschalten
                break;

            case ButtonAction.MainMenu:
                ShowMainMenu(); 
                break;

            case ButtonAction.Music:
                ToggleMusic(); // Musik an/aus schalten
                break;

            case ButtonAction.ClosePaper:
                ClosePapers(); // offene Paper schließen
                break;

            case ButtonAction.MusicVolumeDown:
                ChangeMusicVolume(-volumeStep); 
                break;

            case ButtonAction.MusicVolumeUp:
                ChangeMusicVolume(volumeStep); 
                break;

            case ButtonAction.OpenCreateMenu:
                ShowCreateGameMenu(); // Create-Game-Menü öffnen
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
                ShowMainMenu(); // zurück ins Hauptmenü
                break;

            case ButtonAction.ConfirmCreateGame:
                ConfirmCreateGame(); 
                break;
        }
    }

    // =====================================
    // Musik an- und ausschalten
    // =====================================

    private void ToggleMusic()
    {
        if (musicSettings != null)
        {
            musicSettings.ToggleMusic(); // nutzt VRMusicSettings, wenn zugewiesen
            return;
        }

        if (musicAudioSource != null)
        {
            musicAudioSource.mute = !musicAudioSource.mute; // Fallback: AudioSource stumm schalten
        }
        else
        {
            Debug.LogWarning("VRPhysicalButton: No VRMusicSettings or AudioSource assigned for music button.");
        }
    }

    // =====================================
    // Musiklautstärke ändern
    // =====================================

    private void ChangeMusicVolume(float amount)
    {
        if (musicSettings != null)
        {
            if (amount > 0f)
            {
                musicSettings.IncreaseMusicVolume(); // erhöht Lautstärke über VRMusicSettings
            }
            else
            {
                musicSettings.DecreaseMusicVolume(); // verringert Lautstärke über VRMusicSettings
            }

            return;
        }

        if (musicAudioSource != null)
        {
            musicAudioSource.volume = Mathf.Clamp01(musicAudioSource.volume + amount); // Fallback-Lautstärke begrenzt ändern
        }
        else
        {
            Debug.LogWarning("VRPhysicalButton: No VRMusicSettings or AudioSource assigned for volume button.");
        }
    }

    // =====================================
    // Create-Game-Menü anzeigen
    // =====================================

    private void ShowCreateGameMenu()
    {
        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(false); // Hauptmenü ausblenden
        }

        if (createGameMenuRoot != null)
        {
            createGameMenuRoot.SetActive(true); // Create-Menü einblenden
        }

        if (questNetworkInput != null)
        {
            SetMenuStatus("Selected Players: " + questNetworkInput.selectedPlayerCount); // zeigt aktuelle Spieleranzahl
        }
        else
        {
            SetMenuStatus("Select player count."); // Fallback-Text
        }
    }

    // =====================================
    // Hauptmenü anzeigen
    // =====================================

    private void ShowMainMenu()
    {
        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(true); // Hauptmenü einblenden
        }

        if (createGameMenuRoot != null)
        {
            createGameMenuRoot.SetActive(false); // Create-Menü ausblenden
        }

        SetMenuStatus("Create Game or Join Game"); // setzt Hauptmenü-Status
    }

    // =====================================
    // Spieleranzahl auswählen
    // =====================================

    private void SelectPlayerCount(int playerCount)
    {
        if (questNetworkInput == null)
        {
            Debug.LogWarning("VRPhysicalButton: No StandaloneQuestNetworkInput assigned.");
            return;
        }

        questNetworkInput.selectedPlayerCount = Mathf.Clamp(playerCount, 1, 3); // begrenzt Spieleranzahl auf 1 bis 3

        SetMenuStatus("Selected Players: " + questNetworkInput.selectedPlayerCount); // zeigt gewählte Spieleranzahl
    }

    // =====================================
    // Create Game bestätigen
    // =====================================

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
        ); // zeigt Host-Erstellung im Menü an

        questNetworkInput.StartQuestHost(); // startet die Quest als Host
    }

    // =====================================
    // Spiel beitreten
    // =====================================

    private void JoinGame()
    {
        if (questNetworkInput == null)
        {
            Debug.LogWarning("VRPhysicalButton: No StandaloneQuestNetworkInput assigned.");
            return;
        }

        SetMenuStatus("Joining game..."); // zeigt Join-Status

        questNetworkInput.JoinQuestHost(); // verbindet sich als Client

        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(false); // Hauptmenü ausblenden
        }

        if (createGameMenuRoot != null)
        {
            createGameMenuRoot.SetActive(false); // Create-Menü ausblenden
        }
    }

    // =====================================
    // Menüstatus setzen
    // =====================================

    private void SetMenuStatus(string message)
    {
        Debug.Log(message); // schreibt Menüstatus in Console

        if (menuStatusText != null)
        {
            menuStatusText.text = message; // schreibt Menüstatus in UI
        }
    }

    // =====================================
    // Objekt ein- und ausschalten
    // =====================================

    private void ToggleObject(GameObject target)
    {
        if (target != null)
        {
            target.SetActive(!target.activeSelf); // schaltet Zielobjekt um
        }
        else
        {
            Debug.LogWarning("VRPhysicalButton: Toggle target is not assigned on " + gameObject.name);
        }
    }

    // =====================================
    // Paper schließen
    // =====================================

    private void ClosePapers()
    {
        if (helpPaperRoot != null)
        {
            helpPaperRoot.SetActive(false); // Hilfe-Paper schließen
        }

        if (settingsPaperRoot != null)
        {
            settingsPaperRoot.SetActive(false); // Settings-Paper schließen
        }
    }


}
