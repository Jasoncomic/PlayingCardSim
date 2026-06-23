using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

public class StandaloneQuestNetworkInput : MonoBehaviour
{
    [Header("Network")]
    public UnityTransport unityTransport; // speichert den Unity Transport, über den Host und Client verbunden werden
    public NetworkBlackjackTable networkBlackjackTable; // verbindet dieses Eingabescript mit dem Netzwerk-Blackjack-Tisch


[Header("UI")]
    public TMP_Text statusText; // Textfeld, in dem Menü-, Verbindungs- und Spielstatus angezeigt werden

    // =====================================
    // Verbindungseinstellungen
    // =====================================

    [Header("Connection")]
    public string defaultHostIp = "192.168.0.220"; // Standard-IP, falls keine lokale IP sinnvoll erkannt wird
    public ushort port = 7777; 

    // =====================================
    // Spielsetup
    // =====================================

    [Header("Game Setup")]
    [Range(1, 3)]
    public int selectedPlayerCount = 2; // Anzahl der Spieler, die vor dem Start ausgewählt wird

    // =====================================
    // Join-IP-Menü
    // =====================================

    [Header("Join IP Menu")]
    [Range(1, 254)]
    public int joinIpLastNumber = 220; // letzte Zahl der IP-Adresse, die im VR-Menü angepasst werden kann

    private bool callbacksRegistered; 

    // =====================================
    // Initialisierung
    // =====================================

    private void Awake()
    {
        if (unityTransport == null && NetworkManager.Singleton != null) // sucht den UnityTransport automatisch, wenn er nicht im Inspector gesetzt wurde
        {
            unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>(); // holt den Transport direkt vom NetworkManager
        }

        ParseDefaultHostIp(); // übernimmt letzte Zahl aus der Standard-IP in das Join-IP-Menü

        RegisterCallbacks(); // registriert Events für Client-Verbindungen und Disconnects
        ShowPreConnectionMenu(); // zeigt direkt beim Start Verbindungsmenü an
    }

    // =====================================
    // Aufräumen beim Zerstören
    // =====================================

    private void OnDestroy()
    {
        UnregisterCallbacks(); // entfernt registrierte Netzwerk-Callbacks, damit keine doppelten Events entstehen
    }

    // =====================================
    // Laufende Eingabeprüfung
    // =====================================

    private void Update()
    {
        if (NetworkManager.Singleton == null) // bricht ab, wenn kein NetworkManager existiert
        {
            return;
        }

        bool networkRunning = NetworkManager.Singleton.IsListening; // prüft, ob Host oder Client bereits läuft

        if (!networkRunning) // solange noch keine Netzwerkverbindung aktiv ist
        {
            HandleMenuInput(); 
        }
        else
        {
            HandleGameInput(); 
        }
    }

    // =====================================
    // Eingaben im Netzwerk-Menü
    // =====================================

    private void HandleMenuInput()
    {
        bool ipEditMode = IsIpEditModePressed(); // prüft, ob der IP-Bearbeitungsmodus aktiv ist

        /*
         * IP EDIT MODE:
         * Hold left or right index trigger.
         * Then press X/Y to change the last number of the Join IP.
         *
         * This avoids using the left joystick, because the joystick is already used
         * for VR movement in the world.
         */
        if (ipEditMode)
        {
            if (OVRInput.GetDown(OVRInput.RawButton.X)) // X verringert  letzte IP-Zahl
            {
                ChangeJoinIpLastNumber(-1);
            }

            if (OVRInput.GetDown(OVRInput.RawButton.Y)) // Y erhöht letzte IP-Zahl
            {
                ChangeJoinIpLastNumber(1);
            }

            return;
        }

        /*
         * NORMAL MENU MODE:
         * X/Y changes player count.
         */
        if (OVRInput.GetDown(OVRInput.RawButton.X)) // X verringert Spieleranzahl
        {
            selectedPlayerCount--;

            if (selectedPlayerCount < 1)
            {
                selectedPlayerCount = 1;
            }

            ShowPreConnectionMenu(); // aktualisiert Menüanzeige nach der Änderung
        }

        if (OVRInput.GetDown(OVRInput.RawButton.Y)) 
        {
            selectedPlayerCount++;

            if (selectedPlayerCount > 3)
            {
                selectedPlayerCount = 3;
            }

            ShowPreConnectionMenu();
        }

        // A = Host
        if (OVRInput.GetDown(OVRInput.RawButton.A)) // A startet auf der Quest einen Host
        {
            StartQuestHost();
        }

        // B = Join
        if (OVRInput.GetDown(OVRInput.RawButton.B)) // B verbindet sich mit einem Host
        {
            JoinQuestHost();
        }


#if UNITY_EDITOR
HandleEditorMenuInput(); // erlaubt Menütests mit Tastatur im Unity Editor
#endif
    }


// =====================================
// IP-Bearbeitungsmodus prüfen
// =====================================

private bool IsIpEditModePressed()
    {
        float leftIndexTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger); // liest linken Index-Trigger aus
        float rightIndexTrigger = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger); // liest rechten Index-Trigger aus

        return leftIndexTrigger > 0.65f || rightIndexTrigger > 0.65f; // IP-Modus ist aktiv, wenn einer der Trigger gedrückt ist
    }


#if UNITY_EDITOR


// =====================================
// Menü-Teststeuerung im Unity Editor
// =====================================

private void HandleEditorMenuInput()
    {
        if (Keyboard.current == null) // bricht ab, wenn kein Keyboard erkannt wird
        {
            return;
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame) // Pfeil runter verringert Spieleranzahl
        {
            selectedPlayerCount--; // reduziert die ausgewählte Spielerzahl

            if (selectedPlayerCount < 1)
            {
                selectedPlayerCount = 1; // setzt Spieleranzahl auf Minimum
            }

            ShowPreConnectionMenu(); // aktualisiert ´ Menüanzeige
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame) // Pfeil hoch erhöht Spieleranzahl
        {
            selectedPlayerCount++;

            if (selectedPlayerCount > 3)
            {
                selectedPlayerCount = 3;
            }

            ShowPreConnectionMenu();
        }

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame) // Pfeil links verringert die letzte IP-Zahl
        {
            ChangeJoinIpLastNumber(-1);
        }

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame) // Pfeil rechts erhöht die letzte IP-Zahl
        {
            ChangeJoinIpLastNumber(1);
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame) // Enter startet Host im Editor
        {
            StartQuestHost(); // erstellt ein Host-Spiel
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame) // Leertaste startet Join im Editor
        {
            JoinQuestHost(); // verbindet als Client mit dem Host
        }
    }


#endif


// =====================================
// Eingaben während des Spiels
// =====================================

private void HandleGameInput()
    {
        if (networkBlackjackTable == null) // bricht ab, wenn kein Blackjack-Tisch zugewiesen ist
        {
            SetStatus("Network running, but NetworkBlackjackTable missing."); // zeigt Fehler im Statusfeld an
            return;
        }

        // Quest controller input during the game.
        if (OVRInput.GetDown(OVRInput.RawButton.A)) // A führt Hit aus
        {
            networkBlackjackTable.HitButton(); // Spieler zieht eine Karte
        }

        if (OVRInput.GetDown(OVRInput.RawButton.B)) // B führt Stand aus
        {
            networkBlackjackTable.StandButton(); // Spieler beendet seinen Zug
        }

        if (OVRInput.GetDown(OVRInput.RawButton.Y)) // Y startet neue Runde
        {
            networkBlackjackTable.StartRoundButton(); // startet oder resettet die Blackjack-Runde
        }


#if UNITY_EDITOR
HandleEditorKeyboardInput(); // erlaubt Spieltests mit Tastatur im Unity Editor
#endif
    }

#if UNITY_EDITOR


// =====================================
// Spiel-Teststeuerung im Unity Editor
// =====================================

private void HandleEditorKeyboardInput()
    {
        if (Keyboard.current == null) // bricht ab, wenn kein Keyboard vorhanden ist
        {
            return;
        }

        // Editor/Laptop test input:
        // H = Hit
        // J = Stand
        // R = New Round
        if (Keyboard.current.hKey.wasPressedThisFrame) // H simuliert Hit
        {
            networkBlackjackTable.HitButton(); // zieht eine Karte
        }

        if (Keyboard.current.jKey.wasPressedThisFrame) // J simuliert Stand
        {
            networkBlackjackTable.StandButton(); // beendet den Spielerzug
        }

        if (Keyboard.current.rKey.wasPressedThisFrame) // R simuliert neue Runde
        {
            networkBlackjackTable.StartRoundButton(); // startet eine neue Runde
        }
    }


#endif


// =====================================
// Letzte Zahl der Join-IP ändern
// =====================================

private void ChangeJoinIpLastNumber(int direction)
    {
        joinIpLastNumber += direction; // verändert die letzte IP-Zahl je nach Richtung

        if (joinIpLastNumber < 1) // verhindert ungültige IP-Endzahl unter 1
        {
            joinIpLastNumber = 1; // setzt Minimum
        }

        if (joinIpLastNumber > 254) // verhindert ungültige IP-Endzahl über 254
        {
            joinIpLastNumber = 254; // setzt Maximum
        }

        ShowPreConnectionMenu(); // aktualisiert die Anzeige mit neuer IP
    }

    // =====================================
    // Menütext vor der Verbindung anzeigen
    // =====================================

    private void ShowPreConnectionMenu()
    {
        string localIp = GetLocalIPv4(); // sucht die lokale IPv4-Adresse dieses Geräts
        string joinIp = BuildJoinIp(); // baut die Join-IP aus Netzwerkpräfix und letzter IP-Zahl

        string message =
            "QUEST STANDALONE MENU\n\n" +
            "Selected Players: " + selectedPlayerCount + "\n\n" +
            "JOIN TARGET\n" +
            "Join IP: " + joinIp + "\n" +
            "Last Number: " + joinIpLastNumber + "\n\n" +
            "THIS DEVICE\n" +
            "This Device IP: " + localIp + "\n\n" +
            "CONTROLS\n" +
            "X = Less players\n" +
            "Y = More players\n" +
            "Hold Index Trigger + X = Join IP -1\n" +
            "Hold Index Trigger + Y = Join IP +1\n\n" +
            "A = Create Game / Host\n" +
            "B = Join Game"; // baut den sichtbaren Menütext zusammen


#if UNITY_EDITOR
message +=
"\n\nEDITOR MENU TEST\n" +
"Up/Down = Players\n" +
"Left/Right = Join IP\n" +
"Enter = Host\n" +
"Space = Join"; // ergänzt Tastaturhilfe, wenn das Spiel im Editor läuft
#endif


    SetStatus(message); // schreibt den Text in UI und Console
    }

    // =====================================
    // Quest als Host starten
    // =====================================

    public void StartQuestHost()
    {
        if (NetworkManager.Singleton == null) // prüft, ob ein NetworkManager vorhanden ist
        {
            SetStatus("NetworkManager missing."); 
            return;
        }

        if (unityTransport == null) // prüft, ob UnityTransport vorhanden ist
        {
            SetStatus("UnityTransport missing.");
            return;
        }

        if (networkBlackjackTable == null) // prüft, ob der Blackjack-Tisch vorhanden ist
        {
            SetStatus("NetworkBlackjackTable missing.");
            return;
        }

        if (NetworkManager.Singleton.IsListening) // verhindert mehrfaches Starten des Netzwerks
        {
            SetStatus("Network already running."); 
            return;
        }

        networkBlackjackTable.ConfigurePlayerCount(selectedPlayerCount); // übergibt die ausgewählte Spieleranzahl an den Blackjack-Tisch

        // Host akzeptiert Verbindungen über alle Netzwerkadapter:
        unityTransport.SetConnectionData("0.0.0.0", port, "0.0.0.0");

        bool started = NetworkManager.Singleton.StartHost();

        if (started)
        {
            if (selectedPlayerCount == 1) // bei einem Spieler wird Einzelspielertext angezeigt
            {
                SetStatus(GetSinglePlayerCreatedText()); // zeigt Einzelspieler-Steuerung
            }
            else
            {
                SetStatus(GetMultiplayerHostCreatedText()); // zeigt Host-IP und Multiplayer-Steuerung
            }
        }
        else
        {
            SetStatus("Host failed to start.");
        }
    }

    // =====================================
    // Quest als Client verbinden
    // =====================================

    public void JoinQuestHost()
    {
        if (NetworkManager.Singleton == null) // prüft, ob ein NetworkManager vorhanden ist
        {
            SetStatus("NetworkManager missing."); // zeigt fehlenden NetworkManager an
            return;
        }

        if (unityTransport == null) // prüft, ob UnityTransport vorhanden ist
        {
            SetStatus("UnityTransport missing.");
            return;
        }

        if (NetworkManager.Singleton.IsListening) // verhindert Join, wenn Netzwerk bereits läuft
        {
            SetStatus("Network already running."); 
            return;
        }

        string joinIp = BuildJoinIp(); // baut Ziel-IP für den Client zusammen

        if (string.IsNullOrWhiteSpace(joinIp)) // prüft, ob Join-IP leer oder ungültig ist
        {
            SetStatus("Join IP is empty.");
            return;
        }

        unityTransport.SetConnectionData(joinIp.Trim(), port); // setzt Ziel-IP und Port für Client-Verbindung

        bool started = NetworkManager.Singleton.StartClient(); // startet Client-Verbindungsversuch

        if (started) // wenn Client-Start erfolgreich ausgelöst wurde
        {
            SetStatus(
                "JOINING GAME\n\n" +
                "Host IP: " + joinIp + "\n" +
                "Port: " + port + "\n\n" +
                "Waiting for connection..."
            ); // zeigt Join-Status an
        }
        else
        {
            SetStatus("Client failed to start."); // zeigt Client-Startfehler an
        }
    }

    // =====================================
    // Statusmeldung für Einzelspieler
    // =====================================

    private string GetSinglePlayerCreatedText()
    {
        return
            "SINGLE PLAYER GAME CREATED\n\n" +
            "Players: 1\n\n" +
            "CONTROLS\n" +
            "Y = Start Round / New Round\n" +
            "A = Reveal / Hit\n" +
            "B = Stand / Dealer Turn"; // Text für ein lokales Einzelspieler-Spiel
    }

    // =====================================
    // Statusmeldung für Multiplayer-Host
    // =====================================

    private string GetMultiplayerHostCreatedText()
    {
        return
            "GAME CREATED / HOST STARTED\n\n" +
            "Players: " + selectedPlayerCount + "\n\n" +
            "HOST IP FOR OTHER QUESTS\n" +
            "This Quest IP: " + GetLocalIPv4() + "\n" +
            "Port: " + port + "\n\n" +
            "On the other Quests:\n" +
            "Set Join IP to this IP.\n" +
            "Then press B to join.\n\n" +
            "Y = New Round\n" +
            "A = Hit\n" +
            "B = Stand"; // Text für Host mit IP-Informationen für andere Geräte
    }

    // =====================================
    // Standard-IP auslesen
    // =====================================

    private void ParseDefaultHostIp()
    {
        string[] parts = defaultHostIp.Split('.'); // teilt die Standard-IP in vier Blöcke

        if (parts.Length != 4) // prüft, ob IP aus vier Teilen besteht
        {
            return;
        }

        int parsedLastNumber; // speichert geparste letzte IP-Zahl

        if (int.TryParse(parts[3], out parsedLastNumber)) // versucht, letzte IP-Zahl zu lesen
        {
            joinIpLastNumber = Mathf.Clamp(parsedLastNumber, 1, 254); // übernimmt sie begrenzt in das Join-Menü
        }
    }

    // =====================================
    // Join-IP zusammenbauen
    // =====================================

    private string BuildJoinIp()
    {
        string localIp = GetLocalIPv4(); // ermittelt lokale IP-Adresse

        string prefix = GetIpPrefix(localIp); // nimmt die ersten drei IP-Blöcke aus der lokalen IP

        if (string.IsNullOrWhiteSpace(prefix)) // falls lokale IP kein gültiges Präfix liefert
        {
            prefix = GetIpPrefix(defaultHostIp); // nutzt das Präfix der Standard-IP
        }

        if (string.IsNullOrWhiteSpace(prefix)) 
        {
            return defaultHostIp; 
        }

        return prefix + joinIpLastNumber;
    }

    // =====================================
    // IP-Präfix ermitteln
    // =====================================

    private string GetIpPrefix(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip)) // prüft leere IP
        {
            return "";
        }

        string[] parts = ip.Split('.'); // teilt IP in Blöcke

        if (parts.Length != 4) // eine IPv4-Adresse muss vier Blöcke haben
        {
            return "";
        }

        return parts[0] + "." + parts[1] + "." + parts[2] + ".";
    }

    // =====================================
    // Netzwerk-Callbacks registrieren
    // =====================================

    private void RegisterCallbacks()
    {
        if (callbacksRegistered) // verhindert doppelte Registrierung
        {
            return;
        }

        if (NetworkManager.Singleton == null) // bricht ab, wenn NetworkManager fehlt
        {
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected; // registriert Callback für neue Clients
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected; // registriert Callback für getrennte Clients

        callbacksRegistered = true; 
    }

    // =====================================
    // Netzwerk-Callbacks entfernen
    // =====================================

    private void UnregisterCallbacks()
    {
        if (!callbacksRegistered) // nichts tun, wenn keine Callbacks registriert sind
        {
            return;
        }

        if (NetworkManager.Singleton == null) // bricht ab, wenn NetworkManager fehlt
        {
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected; // entfernt Connected-Callback
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected; // entfernt Disconnect-Callback

        callbacksRegistered = false;
    }

    // =====================================
    // Reaktion auf Client-Verbindung
    // =====================================

    private void OnClientConnected(ulong clientId)
    {
        string role = NetworkManager.Singleton.IsHost ? "Host" : "Client"; // bestimmt, ob dieses Gerät Host oder Client ist

        if (NetworkManager.Singleton.IsHost && selectedPlayerCount == 1) // Einzelspieler braucht keine Multiplayer-Clientanzeige
        {
            SetStatus(GetSinglePlayerCreatedText()); // zeigt Einzelspielerstatus
            return;
        }

        string message =
            role + " connected.\n\n" +
            "Client connected: " + clientId + "\n" +
            "Connected clients: " + NetworkManager.Singleton.ConnectedClientsIds.Count + "\n\n" +
            "Y = New Round\n" +
            "A = Hit\n" +
            "B = Stand"; // baut Verbindungsstatus mit Steuerung zusammen


#if UNITY_EDITOR
message +=
"\n\nEditor test:\n" +
"R = New Round\n" +
"H = Hit\n" +
"J = Stand"; // ergänzt Editor-Tastatursteuerung
#endif


    SetStatus(message); // zeigt aktualisierten Netzwerkstatus an
    }

    // =====================================
    // Reaktion auf Client-Trennung
    // =====================================

    private void OnClientDisconnected(ulong clientId)
    {
        string role = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost ? "Host" : "Client"; // bestimmt Rolle, falls NetworkManager noch existiert

        int count = 0; // Standardwert, falls kein NetworkManager mehr verfügbar ist

        if (NetworkManager.Singleton != null) // prüft, ob NetworkManager noch existiert
        {
            count = NetworkManager.Singleton.ConnectedClientsIds.Count; // liest aktuelle Anzahl verbundener Clients
        }

        SetStatus(
            role + " disconnected.\n\n" +
            "Client disconnected: " + clientId + "\n" +
            "Connected clients: " + count
        ); // zeigt Disconnect-Status an
    }

    // =====================================
    // Lokale IPv4-Adresse finden
    // =====================================

    private string GetLocalIPv4()
    {
        try
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces()) // geht alle Netzwerkadapter durch
            {
                if (networkInterface.OperationalStatus != OperationalStatus.Up) // überspringt deaktivierte Netzwerkadapter
                {
                    continue;
                }

                foreach (UnicastIPAddressInformation ipInfo in networkInterface.GetIPProperties().UnicastAddresses) // geht alle IP-Adressen des Adapters durch
                {
                    IPAddress address = ipInfo.Address; // speichert aktuelle Adresse

                    if (address.AddressFamily != AddressFamily.InterNetwork) // nimmt nur IPv4-Adressen
                    {
                        continue;
                    }

                    string ip = address.ToString(); // wandelt IP-Adresse in Text um

                    if (ip.StartsWith("127.")) // ignoriert localhost-Adresse
                    {
                        continue;
                    }

                    return ip; // gibt die erste passende IPv4-Adresse zurück
                }
            }
        }
        catch
        {
            // Fallback below.
        }

        return "IP not found"; // Rückgabe, wenn keine passende IPv4 gefunden wurde
    }

    // =====================================
    // Status in Console und UI schreiben
    // =====================================

    private void SetStatus(string message)
    {
        Debug.Log("[StandaloneQuestNetwork] " + message); // schreibt Status zusätzlich in die Unity Console

        if (statusText != null) // prüft, ob ein UI-Textfeld gesetzt ist
        {
            statusText.text = message; // zeigt Status im VR-Menü an
        }
    }


}
