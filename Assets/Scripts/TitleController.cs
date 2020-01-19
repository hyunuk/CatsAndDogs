using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleController : MonoBehaviour
{
    static TitleController instance = null;
    public Button easyLevelButton;
    public Button normalLevelButton;
    public Button hardLevelButton;
    public Button PVPGameModeButton;
    public Button PVEGameModeButton;
    public Button EVEGameModeButton;
    public GameObject creditsPanel;
    private string gameMode;
    private string level = "easy";

    private void Awake()
    {
        creditsPanel.SetActive(false);
        if (instance != null)
        {
            Debug.Log("TitleController instance has been destroyed");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("TitleController instance has been assigned");
            instance = this;
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }

    public string GetGameMode() {
        Debug.Log("Returned game mode: " + gameMode);
        return gameMode;
    }

    public void SetGameMode(string gameMode) {
        Debug.Log("Game mode is set to " + gameMode);
        this.gameMode = gameMode;
        switch (gameMode) {
            case "PVP":
                PVPGameModeButton.interactable = false;
                PVEGameModeButton.interactable = true;
                EVEGameModeButton.interactable = true;
                break;
            case "PVE":
                PVPGameModeButton.interactable = true;
                PVEGameModeButton.interactable = false;
                EVEGameModeButton.interactable = true;
                break;
            case "EVE":
                PVPGameModeButton.interactable = true;
                PVEGameModeButton.interactable = true;
                EVEGameModeButton.interactable = false;
                break;
        }
    }

    public string GetLevel() {
        Debug.Log("Returned level: " + level);
        return level;
    }

    public void SetLevel(string level) {
        Debug.Log("Level is set to " + level);
        this.level = level;
        switch (level) {
            case "easy":
                easyLevelButton.interactable = false;
                normalLevelButton.interactable = true;
                hardLevelButton.interactable = true;
                break;
            case "normal":
                easyLevelButton.interactable = true;
                normalLevelButton.interactable = false;
                hardLevelButton.interactable = true;
                break;
            case "hard":
                easyLevelButton.interactable = true;
                normalLevelButton.interactable = true;
                hardLevelButton.interactable = false;
                break;
        }
    }

    public void StartGame() {
        Debug.Log("Game start!");
        SceneManager.LoadScene("MainScene", LoadSceneMode.Additive);
    }

    public void ToggleCreditsPanel(bool active) {
        creditsPanel.SetActive(active);
    }
}
