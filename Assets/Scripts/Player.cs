using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    private List<ButtonObj> buttons;
    private int playerIndex;
	public bool isAI;

    public Player(int index, bool isAI) {
        this.buttons = new List<ButtonObj>();
        this.playerIndex = index;
		this.isAI = isAI;
    }
	
    public int GetPlayerIndex() {
        return playerIndex;
    }

    public List<ButtonObj> GetButtonObjs() {
        return buttons;
    }

	public int GetSize() {
		return buttons.Count;
	}

	public void AddButton(ButtonObj button) {
		if (this.buttons.Contains(button)) {
			Debug.Log("Occupied in " + button);
			return;
		}
		this.buttons.Add(button);
	}

	public void RemoveButton(ButtonObj button) {
		this.buttons.Remove(button);
	}

	public ButtonObj AIEasyRun(List<ButtonObj> buttons) {
		return null;
    }
	
	public ButtonObj AINormalRun(List<ButtonObj> buttons) {
		return null;
    }
}
