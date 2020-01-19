using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	private List<ButtonObj> Buttons = new List<ButtonObj>();
	private int playerIndex;
	public string level = "normal";
	public bool isAI;

	public int GetPlayerIndex() {
		return playerIndex;
	}

	public List<ButtonObj> GetButtonObjs() {
		return Buttons;
	}

	public int GetSize() {
		return Buttons.Count;
	}

	public string GetLevel() {
		return level;
	}

	public void AddButton(ButtonObj button) {
		if (this.Buttons.Contains(button)) {
			Debug.Log("Occupied in " + button);
			return;
		}
		this.Buttons.Add(button);
	}

	public void RemoveButton(ButtonObj button) {
		this.Buttons.Remove(button);
	}

	public ButtonObj AIEasyRun(List<ButtonObj> buttons) {
		return null;
	}

	public ButtonObj AINormalRun(List<ButtonObj> buttons) {
		return null;
	}

	internal void SetPlayerIndex(int index) {
		playerIndex = index;
	}

	internal void SetIsAI(bool isAI) {
		this.isAI = isAI;
	}

	internal void SetLevel(string level) {
		this.level = level;
	}
}
