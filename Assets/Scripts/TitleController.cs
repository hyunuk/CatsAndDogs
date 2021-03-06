﻿using System;
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
    public GameObject howToPanel;
    private string gameMode;
    private string level = "easy";

    private void Awake()
    {
        creditsPanel.SetActive(false);
        howToPanel.SetActive(false);
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            GameObject.DontDestroyOnLoad(gameObject);
        }
    }

    public string GetGameMode() {
        return gameMode;
    }

    public void SetGameMode(string gameMode) {
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
        return level;
    }

    public void SetLevel(string level) {
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
        SceneManager.LoadScene("MainScene", LoadSceneMode.Additive);
    }

    public void ToggleCreditsPanel(bool active) {
        creditsPanel.SetActive(active);
    }

    public void ToggleHowToPanel(bool active) {
        howToPanel.SetActive(active);
    }
}
