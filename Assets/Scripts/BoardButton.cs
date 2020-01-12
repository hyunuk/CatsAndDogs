using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
