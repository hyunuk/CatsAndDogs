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
public enum State { empty, cat, dog, nearBorder, farBorder, obstacle };

public class ButtonObj : MonoBehaviour {
    public Button parentButton;
    public Image emptyImg;
    public Image catImg;
    public Image dogImg;
    public Image nearBorderImg;
    public Image farBorderImg;
    public Image obstacleImg;
    public Image[] imgList;
    public Pair coord;
    public GameController gameController;
    public AudioSource catClickedSound;
    public State currState = State.empty;

    private void Awake() {
        catClickedSound = GetComponent<AudioSource>();

    }

    public void UpdateImg() {
        switch (currState) {
            case State.empty:
                SetButtonImage(State.empty);
                break;

            case State.cat:
                SetButtonImage(State.cat);
                break;

            case State.dog:
                SetButtonImage(State.dog);
                break;

            case State.nearBorder:
                SetButtonImage(State.nearBorder);
                break;

            case State.farBorder:
                SetButtonImage(State.farBorder);
                break;

            case State.obstacle:
                SetButtonImage(State.obstacle);
                break;
        }
    }

    public void SetSpace() {
        gameController.ClickEvent(this);
        if (this.currState.Equals(State.cat)) {
            catClickedSound.Play();
        }
        Debug.Log(currState);
    }

    public State GetState() {
        return currState;
    }

    public Pair GetCoord() {
        return this.coord;
    }

    public void SetState(State state) {
        currState = state;
        UpdateImg();
    }

    public void SetButtonImage(State state) {
        for (int i = 0; i < imgList.Length; i++) {
            if (i == (int)state) imgList[i].enabled = true;
            else imgList[i].enabled = false;
        }
    }

    public void SetGameControllerReference(GameController controller) {
        this.gameController = controller;
    }

    public bool Equals(Pair p) {
        return (coord.X == p.X) && (coord.Y == p.Y);
    }
}
