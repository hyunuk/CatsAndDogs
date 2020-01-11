using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Pair
{
    public int X, Y;

    public Pair(int X, int Y)
    {
        this.X = X;
        this.Y = Y;
    }
}

public class BoardButton : MonoBehaviour
{
    public Button button;
    public Text buttonText;
    public Pair coord;
    public Sprite sprite;
    public string playerSide;
    private GameController gameController;

    public void SetGameControllerReference(GameController controller)
    {
        gameController = controller;
    }

    public void SetSpace()
    {
        gameController.ClickEvent(coord);
    }

    public bool Equals(Pair p)
    {
        return (coord.X == p.X) && (coord.Y == p.Y);
    }
}
