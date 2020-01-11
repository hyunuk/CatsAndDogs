using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonObj : MonoBehaviour {
    public Button parentButton;
    public Button catButton;
    public Button dogButton;
    public Image emptyImg;
    public Image borderImg;
    public Image obstacleImg;
    public enum State {
        empty = 0,
        cat = 1,
        dog = 2,
        border = 3,
        obstacle = 4
    }
    public object[] stateList;
    public State currState;
    public int stateMutator;

    void Start() {
        currState = State.empty;
        stateList = new object[5];
        stateList[0] = State.empty;
        stateList[1] = State.cat;
        stateList[2] = State.dog;
        stateList[3] = State.border;
        stateList[4] = State.obstacle;
        stateMutator = 0;
        catButton.interactable = false;
        dogButton.interactable = false;
        emptyImg.enabled = true;
        borderImg.enabled = false;
        obstacleImg.enabled = false;
    }

    void SetState(int stateMutator) {
        currState = (State) stateList[stateMutator];
    }

    void Update() {
        switch(currState) {
            case State.empty:
                catButton.enabled = false;
                dogButton.enabled = false;
                emptyImg.enabled = true;
                borderImg.enabled = false;
                obstacleImg.enabled = false;
                break;

            case State.border:
                catButton.enabled = false;
                dogButton.enabled = false;
                emptyImg.enabled = false;
                borderImg.enabled = true;
                obstacleImg.enabled = false;
                break;

            case State.obstacle:
                catButton.enabled = false;
                dogButton.enabled = false;
                emptyImg.enabled = false;
                borderImg.enabled = false;
                obstacleImg.enabled = true;
                break;

            case State.cat:
                catButton.enabled = true;
                dogButton.enabled = false;
                emptyImg.enabled = false;
                borderImg.enabled = false;
                obstacleImg.enabled = false;
                break;

            case State.dog:
                catButton.enabled = false;
                dogButton.enabled = true;
                emptyImg.enabled = false;
                borderImg.enabled = false;
                obstacleImg.enabled = false;
                break;
        }
    }

    public void SetSpace() {
        stateMutator = (stateMutator + 1) % 5;
        SetState(stateMutator);
        Debug.Log(currState);
    }
}
