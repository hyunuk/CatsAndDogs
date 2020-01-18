using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    static TitleController instance = null;
    private string gameMode;
    private static string[] levels = { "easy", "normal" };

    private void Awake()
    {
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
    }

    public string GetLevel(int playerIndex) {
        Debug.Log("Returned level: " + levels[playerIndex]);
        return levels[playerIndex];
    }

    public void SetLevel(int playerIndex, string level) {
        Debug.Log("Level is set to " + level);
        levels[playerIndex] = level;
    }

    public void StartGame() {
        Debug.Log("Game start!");
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }
}
