using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class StandaloneQuestNetworkInput : MonoBehaviour
{
    [Header("Network")]
    public UnityTransport unityTransport;
    public NetworkBlackjackTable networkBlackjackTable;

    [Header("UI")]
    public TMP_Text statusText;

    [Header("Connection")]
    public string defaultHostIp = "192.168.0.130";
    public ushort port = 7777;

    private bool callbacksRegistered;

    private void Awake()
    {
        if (unityTransport == null && NetworkManager.Singleton != null)
        {
            unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        }

        RegisterCallbacks();

        SetStatus(
            "Standalone Quest Mode\n" +
            "Before connect:\n" +
            "A = Host\n" +
            "B = Join " + defaultHostIp + "\n\n" +
            "This Device IP: " + GetLocalIPv4()
        );
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
            HandleConnectionInput();
        }
        else
        {
            HandleGameInput();
        }
    }

    private void HandleConnectionInput()
    {
        // Right controller A = Start Host
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            StartQuestHost();
        }

        // Right controller B = Join Host
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            JoinQuestHost();
        }
    }

    private void HandleGameInput()
    {
        if (networkBlackjackTable == null)
        {
            SetStatus("Network running, but NetworkBlackjackTable missing.");
            return;
        }

        // A = Hit / reveal own cards first
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            networkBlackjackTable.HitButton();
        }

        // B = Stand
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            networkBlackjackTable.StandButton();
        }

        // Y = Start new round
        if (OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            networkBlackjackTable.StartRoundButton();
        }
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

        if (NetworkManager.Singleton.IsListening)
        {
            SetStatus("Network already running.");
            return;
        }

        // Host listens on all available network interfaces.
        unityTransport.SetConnectionData("0.0.0.0", port, "0.0.0.0");

        bool started = NetworkManager.Singleton.StartHost();

        if (started)
        {
            SetStatus(
                "HOST STARTED\n" +
                "This Quest IP: " + GetLocalIPv4() + "\n" +
                "Port: " + port + "\n\n" +
                "Editor/Laptop should join this IP.\n\n" +
                "Y = New Round\n" +
                "A = Hit\n" +
                "B = Stand"
            );
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

        if (string.IsNullOrWhiteSpace(defaultHostIp))
        {
            SetStatus("defaultHostIp is empty.");
            return;
        }

        unityTransport.SetConnectionData(defaultHostIp.Trim(), port);

        bool started = NetworkManager.Singleton.StartClient();

        if (started)
        {
            SetStatus(
                "JOINING HOST\n" +
                "Host IP: " + defaultHostIp + "\n" +
                "Port: " + port
            );
        }
        else
        {
            SetStatus("Client failed to start.");
        }
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

        SetStatus(
            role + " connected.\n" +
            "Client connected: " + clientId + "\n" +
            "Connected clients: " + NetworkManager.Singleton.ConnectedClientsIds.Count + "\n\n" +
            "Y = New Round\n" +
            "A = Hit\n" +
            "B = Stand"
        );
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
            role + " disconnected.\n" +
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
            // Ignore and return fallback.
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