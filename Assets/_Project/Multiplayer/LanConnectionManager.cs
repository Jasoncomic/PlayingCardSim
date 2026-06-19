using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class LanConnectionManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField ipInputField;
    public TMP_Text statusText;

    [Header("Network")]
    public UnityTransport unityTransport;

    private void Awake()
    {
        if (unityTransport == null)
        {
            unityTransport = FindObjectOfType<UnityTransport>();
        }
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    public void StartHost()
    {
        if (unityTransport == null)
        {
            SetStatus("UnityTransport not found.");
            return;
        }

        unityTransport.SetConnectionData("0.0.0.0", 7777);

        bool started = NetworkManager.Singleton.StartHost();

        if (started)
        {
            SetStatus("Host started. Waiting for clients...");
        }
        else
        {
            SetStatus("Host failed to start.");
        }
    }

    public void JoinHost()
    {
        if (unityTransport == null)
        {
            SetStatus("UnityTransport not found.");
            return;
        }

        string ip = "";

        if (ipInputField != null)
        {
            ip = ipInputField.text.Trim();
        }

        if (string.IsNullOrWhiteSpace(ip))
        {
            SetStatus("Enter host IP first.");
            return;
        }

        unityTransport.SetConnectionData(ip, 7777);

        bool started = NetworkManager.Singleton.StartClient();

        if (started)
        {
            SetStatus("Joining host at " + ip + "...");
        }
        else
        {
            SetStatus("Client failed to start.");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        string message =
            "Client connected: " + clientId +
            "\nConnected clients: " + NetworkManager.Singleton.ConnectedClientsIds.Count;

        SetStatus(message);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        string message =
            "Client disconnected: " + clientId +
            "\nConnected clients: " + NetworkManager.Singleton.ConnectedClientsIds.Count;

        SetStatus(message);
    }

    private void SetStatus(string message)
    {
        Debug.Log("[LAN] " + message);

        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}