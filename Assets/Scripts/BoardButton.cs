using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardButton : MonoBehaviour {
    public Button button;
    public Text buttonText;
    public Sprite sprite;
    public string playerSide;
    public void SetSpace() {
        buttonText.text = playerSide;
        //sprite.texture
        button.interactable = false;

    }
}
