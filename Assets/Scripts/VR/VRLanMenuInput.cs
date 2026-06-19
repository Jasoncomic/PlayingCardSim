using TMPro;
using UnityEngine;

public class VRLanMenuInput : MonoBehaviour
{
    [Header("References")]
    public LanConnectionManager lanConnectionManager;
    public TMP_InputField ipInputField;
    public GameObject menuPanel;

    [Header("LAN Settings")]
    public string defaultHostIp = "192.168.0.130";

    private bool actionTriggered;

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
                lanConnectionManager.StartHost();
            }

            HideMenu();
        }

        // B button on right controller = Join Game / Client
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            actionTriggered = true;

            if (ipInputField != null)
            {
                ipInputField.text = defaultHostIp;
            }

            if (lanConnectionManager != null)
            {
                lanConnectionManager.JoinHost();
            }

            //HideMenu();
        }
    }

    private void HideMenu()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
    }
}