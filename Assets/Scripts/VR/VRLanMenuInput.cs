using TMPro;
using UnityEngine;

public class VRLanMenuInput : MonoBehaviour
{
[Header("References")]
    public LanConnectionManager lanConnectionManager; // verwaltet Host- und Client-Verbindung im LAN
    public TMP_InputField ipInputField; // Eingabefeld für Host-IP
    public GameObject menuPanel; // Menüfenster, das nach Host-Start ausgeblendet wird

    // =====================================
    // LAN-Einstellungen
    // =====================================

    [Header("LAN Settings")]
    public string defaultHostIp = "192.168.0.130"; // Standard-IP für den Host

    
    private bool actionTriggered; // verhindert mehrfaches Auslösen von Host oder Join

    // =====================================
    // Eingaben prüfen
    // =====================================

    private void Update()
    {
        if (actionTriggered)
        {
            return;
        }

        // A button on right controller = Create Game / Host
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            actionTriggered = true;

            if (lanConnectionManager != null)
            {
                lanConnectionManager.StartHost(); // startet das Spiel als Host
            }

            HideMenu();
        }

        // B button on right controller = Join Game / Client
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            actionTriggered = true;

            if (ipInputField != null)
            {
                ipInputField.text = defaultHostIp; // trägt die Standard-Host-IP ein
            }

            if (lanConnectionManager != null)
            {
                lanConnectionManager.JoinHost(); // verbindet sich als Client mit dem Host
            }

            //HideMenu();
        }
    }

    // =====================================
    // Menü ausblenden
    // =====================================

    private void HideMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false); // blendet das LAN-Menü aus
        }
    }


}
