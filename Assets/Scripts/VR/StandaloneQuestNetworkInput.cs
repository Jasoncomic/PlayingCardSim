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
    public UnityTransport unityTransport;
    public NetworkBlackjackTable networkBlackjackTable;

    [Header("UI")]
    public TMP_Text statusText;

    [Header("Connection")]
    public string defaultHostIp = "192.168.0.220";
    public ushort port = 7777;

    [Header("Game Setup")]
    [Range(1, 3)]
    public int selectedPlayerCount = 2;

    [Header("Join IP Menu")]
    [Range(1, 254)]
    public int joinIpLastNumber = 220;

    private bool callbacksRegistered;

    private void Awake()
    {
        if (unityTransport == null && NetworkManager.Singleton != null)
        {
            unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        }

        ParseDefaultHostIp();

        RegisterCallbacks();
        ShowPreConnectionMenu();
    }

    private void OnDestroy()
    {
        UnregisterCallbacks();
    }

    private void Update()
    {
        if (NetworkManager.Singleton == null)
        {
            return;
        }

        bool networkRunning = NetworkManager.Singleton.IsListening;

        if (!networkRunning)
        {
            HandleMenuInput();
        }
        else
        {
            HandleGameInput();
        }
    }

    private void HandleMenuInput()
    {
        bool ipEditMode = IsIpEditModePressed();

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
            if (OVRInput.GetDown(OVRInput.RawButton.X))
            {
                ChangeJoinIpLastNumber(-1);
            }

            if (OVRInput.GetDown(OVRInput.RawButton.Y))
            {
                ChangeJoinIpLastNumber(1);
            }

            return;
        }

        /*
         * NORMAL MENU MODE:
         * X/Y changes player count.
         */
        if (OVRInput.GetDown(OVRInput.RawButton.X))
        {
            selectedPlayerCount--;

            if (selectedPlayerCount < 1)
            {
                selectedPlayerCount = 1;
            }

            ShowPreConnectionMenu();
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
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            StartQuestHost();
        }

        // B = Join
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            JoinQuestHost();
        }

#if UNITY_EDITOR
        HandleEditorMenuInput();
#endif
    }

    private bool IsIpEditModePressed()
    {
        float leftIndexTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger);
        float rightIndexTrigger = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);

        return leftIndexTrigger > 0.65f || rightIndexTrigger > 0.65f;
    }

#if UNITY_EDITOR
    private void HandleEditorMenuInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            selectedPlayerCount--;

            if (selectedPlayerCount < 1)
            {
                selectedPlayerCount = 1;
            }

            ShowPreConnectionMenu();
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            selectedPlayerCount++;

            if (selectedPlayerCount > 3)
            {
                selectedPlayerCount = 3;
            }

            ShowPreConnectionMenu();
        }

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            ChangeJoinIpLastNumber(-1);
        }

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            ChangeJoinIpLastNumber(1);
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            StartQuestHost();
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            JoinQuestHost();
        }
    }
#endif

    private void HandleGameInput()
    {
        if (networkBlackjackTable == null)
        {
            SetStatus("Network running, but NetworkBlackjackTable missing.");
            return;
        }

        // Quest controller input during the game.
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            networkBlackjackTable.HitButton();
        }

        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            networkBlackjackTable.StandButton();
        }

        if (OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            networkBlackjackTable.StartRoundButton();
        }

#if UNITY_EDITOR
        HandleEditorKeyboardInput();
#endif
    }

#if UNITY_EDITOR
    private void HandleEditorKeyboardInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        // Editor/Laptop test input:
        // H = Hit
        // J = Stand
        // R = New Round
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            networkBlackjackTable.HitButton();
        }

        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            networkBlackjackTable.StandButton();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            networkBlackjackTable.StartRoundButton();
        }
    }
#endif

    private void ChangeJoinIpLastNumber(int direction)
    {
        joinIpLastNumber += direction;

        if (joinIpLastNumber < 1)
        {
            joinIpLastNumber = 1;
        }

        if (joinIpLastNumber > 254)
        {
            joinIpLastNumber = 254;
        }

        ShowPreConnectionMenu();
    }

    private void ShowPreConnectionMenu()
    {
        string localIp = GetLocalIPv4();
        string joinIp = BuildJoinIp();

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
            "B = Join Game";

#if UNITY_EDITOR
        message +=
            "\n\nEDITOR MENU TEST\n" +
            "Up/Down = Players\n" +
            "Left/Right = Join IP\n" +
            "Enter = Host\n" +
            "Space = Join";
#endif

        SetStatus(message);
    }

    public void StartQuestHost()
    {
        if (NetworkManager.Singleton == null)
        {
            SetStatus("NetworkManager missing.");
            return;
        }

        if (unityTransport == null)
        {
            SetStatus("UnityTransport missing.");
            return;
        }

        if (networkBlackjackTable == null)
        {
            SetStatus("NetworkBlackjackTable missing.");
            return;
        }

        if (NetworkManager.Singleton.IsListening)
        {
            SetStatus("Network already running.");
            return;
        }

        networkBlackjackTable.ConfigurePlayerCount(selectedPlayerCount);

        // Host listens on all network interfaces.
        unityTransport.SetConnectionData("0.0.0.0", port, "0.0.0.0");

        bool started = NetworkManager.Singleton.StartHost();

        if (started)
        {
            if (selectedPlayerCount == 1)
            {
                SetStatus(GetSinglePlayerCreatedText());
            }
            else
            {
                SetStatus(GetMultiplayerHostCreatedText());
            }
        }
        else
        {
            SetStatus("Host failed to start.");
        }
    }

    public void JoinQuestHost()
    {
        if (NetworkManager.Singleton == null)
        {
            SetStatus("NetworkManager missing.");
            return;
        }

        if (unityTransport == null)
        {
            SetStatus("UnityTransport missing.");
            return;
        }

        if (NetworkManager.Singleton.IsListening)
        {
            SetStatus("Network already running.");
            return;
        }

        string joinIp = BuildJoinIp();

        if (string.IsNullOrWhiteSpace(joinIp))
        {
            SetStatus("Join IP is empty.");
            return;
        }

        unityTransport.SetConnectionData(joinIp.Trim(), port);

        bool started = NetworkManager.Singleton.StartClient();

        if (started)
        {
            SetStatus(
                "JOINING GAME\n\n" +
                "Host IP: " + joinIp + "\n" +
                "Port: " + port + "\n\n" +
                "Waiting for connection..."
            );
        }
        else
        {
            SetStatus("Client failed to start.");
        }
    }

    private string GetSinglePlayerCreatedText()
    {
        return
            "SINGLE PLAYER GAME CREATED\n\n" +
            "Players: 1\n\n" +
            "CONTROLS\n" +
            "Y = Start Round / New Round\n" +
            "A = Reveal / Hit\n" +
            "B = Stand / Dealer Turn";
    }

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
            "B = Stand";
    }

    private void ParseDefaultHostIp()
    {
        string[] parts = defaultHostIp.Split('.');

        if (parts.Length != 4)
        {
            return;
        }

        int parsedLastNumber;

        if (int.TryParse(parts[3], out parsedLastNumber))
        {
            joinIpLastNumber = Mathf.Clamp(parsedLastNumber, 1, 254);
        }
    }

    private string BuildJoinIp()
    {
        string localIp = GetLocalIPv4();

        string prefix = GetIpPrefix(localIp);

        if (string.IsNullOrWhiteSpace(prefix))
        {
            prefix = GetIpPrefix(defaultHostIp);
        }

        if (string.IsNullOrWhiteSpace(prefix))
        {
            return defaultHostIp;
        }

        return prefix + joinIpLastNumber;
    }

    private string GetIpPrefix(string ip)
    {
        if (string.IsNullOrWhiteSpace(ip))
        {
            return "";
        }

        string[] parts = ip.Split('.');

        if (parts.Length != 4)
        {
            return "";
        }

        return parts[0] + "." + parts[1] + "." + parts[2] + ".";
    }

    private void RegisterCallbacks()
    {
        if (callbacksRegistered)
        {
            return;
        }

        if (NetworkManager.Singleton == null)
        {
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        callbacksRegistered = true;
    }

    private void UnregisterCallbacks()
    {
        if (!callbacksRegistered)
        {
            return;
        }

        if (NetworkManager.Singleton == null)
        {
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

        callbacksRegistered = false;
    }

    private void OnClientConnected(ulong clientId)
    {
        string role = NetworkManager.Singleton.IsHost ? "Host" : "Client";

        if (NetworkManager.Singleton.IsHost && selectedPlayerCount == 1)
        {
            SetStatus(GetSinglePlayerCreatedText());
            return;
        }

        string message =
            role + " connected.\n\n" +
            "Client connected: " + clientId + "\n" +
            "Connected clients: " + NetworkManager.Singleton.ConnectedClientsIds.Count + "\n\n" +
            "Y = New Round\n" +
            "A = Hit\n" +
            "B = Stand";

#if UNITY_EDITOR
        message +=
            "\n\nEditor test:\n" +
            "R = New Round\n" +
            "H = Hit\n" +
            "J = Stand";
#endif

        SetStatus(message);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        string role = NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost ? "Host" : "Client";

        int count = 0;

        if (NetworkManager.Singleton != null)
        {
            count = NetworkManager.Singleton.ConnectedClientsIds.Count;
        }

        SetStatus(
            role + " disconnected.\n\n" +
            "Client disconnected: " + clientId + "\n" +
            "Connected clients: " + count
        );
    }

    private string GetLocalIPv4()
    {
        try
        {
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                foreach (UnicastIPAddressInformation ipInfo in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    IPAddress address = ipInfo.Address;

                    if (address.AddressFamily != AddressFamily.InterNetwork)
                    {
                        continue;
                    }

                    string ip = address.ToString();

                    if (ip.StartsWith("127."))
                    {
                        continue;
                    }

                    return ip;
                }
            }
        }
        catch
        {
            // Fallback below.
        }

        return "IP not found";
    }

    private void SetStatus(string message)
    {
        Debug.Log("[StandaloneQuestNetwork] " + message);

        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}