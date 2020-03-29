using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
	private List<ButtonObj> Buttons = new List<ButtonObj>();
	public int PlayerIndex { get; set; }
	public string Difficulty { get; set; }
	public bool IsAI { get; set; }


	public List<ButtonObj> GetButtons() {
		return Buttons;
	}

	public int GetSize() {
		return Buttons.Count;
	}
	
	public void AddButton(ButtonObj button) {
		if (this.Buttons.Contains(button)) return;
		button.CurrState = PlayerIndex == 0 ? State.cat : State.dog;
		this.Buttons.Add(button);
	}

	public void RemoveButton(ButtonObj button) {
		this.Buttons.Remove(button);
	}
}
