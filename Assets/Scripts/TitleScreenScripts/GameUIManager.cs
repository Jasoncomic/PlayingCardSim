using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    public GameObject gameUIPanel;
    public GameObject settingsPanel;
    public GameObject titleScreenPanel;
    public GameObject buttonContainer;

    public BlackjackUnityTestController blackjackController;

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
    Application.Quit();
    #endif
    }

    public void BackToTitleScreen()
    {
        if (blackjackController != null)
        {
            blackjackController.StopCurrentGame();
        }

        if (gameUIPanel != null)
            gameUIPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (titleScreenPanel != null)
            titleScreenPanel.SetActive(true);

        if (buttonContainer != null)
            buttonContainer.SetActive(true);
    }
}