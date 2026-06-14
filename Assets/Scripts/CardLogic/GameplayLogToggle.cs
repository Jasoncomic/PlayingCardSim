using TMPro;
using UnityEngine;

public class GameplayLogToggle : MonoBehaviour
{
    public GameObject logTextObject;
    public TMP_Text infoButtonText;

    private bool logVisible = true;

    private void Start()
    {
        if (logTextObject != null)
        {
            logVisible = logTextObject.activeSelf;
        }

        if (infoButtonText != null)
        {
            infoButtonText.text = "i";
        }
    }

    public void ToggleLogText()
    {
        if (logTextObject == null)
        {
            Debug.LogWarning("LogText is not assigned.");
            return;
        }

        logVisible = !logVisible;
        logTextObject.SetActive(logVisible);

        if (infoButtonText != null)
        {
            infoButtonText.text = "i";
        }
    }
}