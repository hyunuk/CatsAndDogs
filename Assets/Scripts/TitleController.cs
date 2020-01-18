using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleController : MonoBehaviour
{
    private string gameMode;
    private List<string> levels = new List<string>();

    public string GetGameMode() {
        return gameMode;
    }

    public void SetGameMode(string gameMode) {
        this.gameMode = gameMode;
    }

    public string GetLevel(int playerIndex) {
        return levels[playerIndex];
    }

    public void SetLevel(int playerIndex, string level) {
        levels[playerIndex] = level;
    }
}
