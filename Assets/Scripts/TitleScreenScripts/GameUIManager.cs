using UnityEngine;


public class GameUIManager : MonoBehaviour
{
    public GameObject gameUIPanel;
    public GameObject settingsPanel;
    public GameObject titleScreenPanel;

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    //public void BackToTitleScreen()
    //{
    //    gameUIPanel.SetActive(false);
    //    settingsPanel.SetActive(false);
    //    titleScreenPanel.SetActive(true);
    //}

    public void QuitGame()
    {
        Debug.Log("Quit Game");

        Application.Quit();
    }

    public BlackjackUnityTestController blackjackController;

    public void BackToTitleScreen()
    {
        if (blackjackController != null)
        {
            blackjackController.StopCurrentGame();
        }

        gameUIPanel.SetActive(false);
        settingsPanel.SetActive(false);
        titleScreenPanel.SetActive(true);
    }
}