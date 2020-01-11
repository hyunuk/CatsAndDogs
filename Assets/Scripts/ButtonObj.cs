using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonObj : MonoBehaviour {
    public Button parentButton;
    public Image emptyImg;
    public Image catImg;
    public Image dogImg;
    public Image borderImg;
    public Image obstacleImg;

    public Image[] imgList;
    public enum State { empty, cat, dog, border, obstacle };
    public State currState;

    void Start() {
        currState = State.empty;
        SetButtonImage(State.empty);
    }

    void Update() {
        currState = (State) UnityEngine.Random.Range(0, 4);
        switch(currState) {
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

    public void SetSpace() {
        
        Debug.Log(currState);
    }

    public void SetButtonImage(State state) {
        for (int i = 0; i < imgList.Length; i++) {
            if (i == (int)state) {
                imgList[i].enabled = true;
            } else {
                imgList[i].enabled = false;
            }
        }
    }
}
