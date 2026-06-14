using TMPro;
using UnityEngine;

public class HelpPopupToggle : MonoBehaviour
{
    [Header("Help Popup")]
    public GameObject helpPanel;

    [Header("Optional Button Text")]
    public TMP_Text helpButtonText;

    private void Start()
    {
        CloseHelp();

        if (helpButtonText != null)
        {
            helpButtonText.text = "?";
        }
    }

    public void ToggleHelp()
    {
        if (helpPanel == null)
        {
            Debug.LogWarning("HelpPanel is not assigned.");
            return;
        }

        if (helpPanel.activeSelf)
        {
            CloseHelp();
        }
        else
        {
            OpenHelp();
        }
    }

    public void OpenHelp()
    {
        if (helpPanel == null)
        {
            Debug.LogWarning("HelpPanel is not assigned.");
            return;
        }

        helpPanel.SetActive(true);

        if (helpButtonText != null)
        {
            helpButtonText.text = "?";
        }
    }

    public void CloseHelp()
    {
        if (helpPanel == null)
        {
            Debug.LogWarning("HelpPanel is not assigned.");
            return;
        }

        helpPanel.SetActive(false);

        if (helpButtonText != null)
        {
            helpButtonText.text = "?";
        }
    }
}