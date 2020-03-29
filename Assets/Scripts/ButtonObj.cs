using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Pair {
    public int X, Y;

    public Pair(int X, int Y) {
        this.X = X;
        this.Y = Y;
    }

    public override bool Equals(object obj) {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType())) return false;
        Pair p = (Pair)obj;
        return (X == p.X) && (Y == p.Y);
    }

    public override int GetHashCode() {
        var hashCode = 1861411795;
        hashCode = hashCode * -1521134295 + X.GetHashCode();
        hashCode = hashCode * -1521134295 + Y.GetHashCode();
        return hashCode;
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
    public GameController gameController;
    public AudioSource catClickedSound;
    public AudioSource dogClickedSound;
    [SerializeField]
    private Pair coord;
    public Pair Coord { get => coord; set { coord = value; } }

    private State currState;
    public State CurrState { 
        get => currState;
        set {
            currState = value;
            for (int i = 0; i < imgList.Length; i++) {
                if (i == (int)currState) imgList[i].enabled = true;
                else imgList[i].enabled = false;
            }
        }
    }

    private void Awake() {
        catClickedSound = GetComponent<AudioSource>();
        dogClickedSound = GetComponent<AudioSource>();
    }

    public void SetSpace() {
        gameController.ClickEvent(this);
    }

    public void SetGameControllerReference(GameController controller) {
        this.gameController = controller;
    }
}
