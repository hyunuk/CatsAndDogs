using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Button : MonoBehaviour {
    public Button button;
    public Text buttonText;
    public string playerSide;
    public void SetSpace() {
        buttonText.text = playerSide;
        button.button = false;
    }
}
