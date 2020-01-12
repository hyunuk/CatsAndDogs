using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Pair
{
    public int X, Y;

    public Pair(int X, int Y)
    {
        this.X = X;
        this.Y = Y;
    }
}

[Serializable]
public enum State { empty, cat, dog, border, obstacle };

public class ButtonObj : MonoBehaviour
{
    public Button parentButton;
    public Image emptyImg;
    public Image catImg;
    public Image dogImg;
    public Image borderImg;
    public Image obstacleImg;
    public Image[] imgList;
    public Pair coord;
    private GameController gameController;
    public State currState = State.empty;

    // void Start()
    // {
    //     currState = State.empty;
    //     SetButtonImage(State.empty);
    // }

    void Update()
    {
        switch (currState)
        {
            case State.empty:
                SetButtonImage(State.empty);
                break;

            case State.cat:
                SetButtonImage(State.cat);
                break;

            case State.dog:
                SetButtonImage(State.dog);
                break;

            case State.border:
                SetButtonImage(State.border);
                break;

            case State.obstacle:
                SetButtonImage(State.obstacle);
                break;
        }
    }

    public void SetSpace()
    {
        gameController.ClickEvent(coord);
        Debug.Log(currState);
    }

    public void SetState(State state)
    {
        currState = state;

    }

    public void SetButtonImage(State state)
    {
        for (int i = 0; i < imgList.Length; i++)
        {
            if (i == (int)state)
            {
                imgList[i].enabled = true;
            }
            else
            {
                imgList[i].enabled = false;
            }
        }
    }

    public void SetGameControllerReference(GameController controller)
    {
        gameController = controller;
    }

    public bool Equals(Pair p)
    {
        return (coord.X == p.X) && (coord.Y == p.Y);
    }
}
