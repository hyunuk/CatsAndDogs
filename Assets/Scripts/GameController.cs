using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public ButtonObj[] buttonList;
    public Pair selectedCell; // null if no selected cell exists
    public ButtonObj selectedButton;
    public readonly int LINE_COUNT = 7;
    public int turnCount = 0;
    public enum Status { notSelected, clicked };
    private Status status;

    void Awake() {
        StartGame();
    }

    void StartGame() {
        SetGameControllerReferenceOnButtons();
        InitPlayers();
        DrawBoard();
    }

    public void SetGameControllerReferenceOnButtons() {
        foreach (ButtonObj button in buttonList) {
            button.SetGameControllerReference(this);
            button.parentButton.interactable = true;
        }
    }

    private void InitPlayers() {
        buttonList[0].SetState(State.cat);
        buttonList[1].SetState(State.cat);
        buttonList[5].SetState(State.dog);
        buttonList[6].SetState(State.dog);
        buttonList[42].SetState(State.dog);
        buttonList[43].SetState(State.dog);
        buttonList[47].SetState(State.cat);
        buttonList[48].SetState(State.cat);
        status = Status.notSelected;
    }

    private void DrawBoard() {
        foreach (ButtonObj currButton in buttonList) {
            currButton.UpdateImg();
        }
    }

    public void GameOver() {
        foreach (ButtonObj button in buttonList) {
            SetStatus(button.coord, false);
        }
    }

    private List<ButtonObj> FindNeighbors(ButtonObj btn, int gap) {
        List<ButtonObj> retList = new List<ButtonObj>();
        Pair coord = btn.coord;
        for (int x = coord.X - gap; x <= coord.X + gap; x++) {
            for (int y = coord.Y - gap; y <= coord.Y + gap; y++) {
                if (x != coord.X && y != coord.Y && WithinBoundary(x, y)) {
                    ButtonObj temp = buttonList[GetPosition(new Pair(x, y))];
                    retList.Add(temp);
                }
            }
        }
        return retList;
    }

    private void SetStatus(Pair coord, bool status) {
        int pos = GetPosition(coord.X, coord.Y);
        buttonList[pos].parentButton.interactable = status;
    }

    private bool WithinBoundary(Pair coord) {
        return WithinBoundary(coord.X, coord.Y);
    }

    private bool WithinBoundary(int x, int y) {
        return 0 <= x && x < LINE_COUNT && 0 <= y && y < LINE_COUNT;
    }

    private int GetPosition(Pair coord) {
        return GetPosition(coord.X, coord.Y);
    }

    private int GetPosition(ButtonObj btn) {
        return GetPosition(btn.GetCoord());
    }

    private int GetPosition(int x, int y) {
        return (y * LINE_COUNT) + x;
    }

    private int GetDistance(Pair from, Pair to) {
        int xDist = from.X - to.X;
        int yDist = from.Y - to.Y;
        return (int)Math.Sqrt(xDist * xDist + yDist * yDist);
    }

    private void Attack(ButtonObj clickedCell) {
        RemoveCurrentCell();
        MoveCell(clickedCell);
        // ConsumeCell(clickedCell);
        EndTurn();
    }

    private void MoveCell(ButtonObj clickedCell) {
        ClearAvailableCells();
        int pos = GetPosition(clickedCell);
        // TODO: implement
        buttonList[pos].SetButtonImage(State.cat);
        // board.put(clickedCell, currentPlayerIndex);
        // players.get(currentPlayerIndex).add(clickedCell.getX(), clickedCell.getY());
    }

    private void RemoveCurrentCell() {
        selectedCell = null;
    }

    public void ClickEvent(ButtonObj clickedButton) {
        Debug.Log("on ClickEvent");
        switch (status) {
            case Status.notSelected:
                if (clickedButton.GetState() == State.cat || clickedButton.GetState() == State.dog) {
                    this.selectedButton = clickedButton;
                    this.status = Status.clicked;
                    UpdateAvailableCells(selectedButton);
                }
                break;

            case Status.clicked:
                if (clickedButton.GetState().Equals(State.border)) {
                    selectedButton = clickedButton;
                    ClearAvailableCells();
                    UpdateAvailableCells(selectedButton);
                }
                this.status = Status.notSelected;
                Attack(clickedButton);
                break;
        }

        DrawBoard();



        // if (gameController.turnCount < gameController.LINE_COUNT * gameController.LINE_COUNT) {
        //     buttonText.text = playerSide;
        //     //sprite.texture
        //     // button.interactable = false;
        //     gameController.DrawBoard(this);
        // } else {
        //     gameController.GameOver();
        // }

        // turnCount++;
        // Player currPlayer = this.players.get(this.currentPlayerIndex);
        // switch (status) {
        // 	case notSelected:
        // 		if (currentPlayerIndex != board.get(clickedCell)) break;
        // 		if (isPlayerCell(clickedCell, currPlayer)) {
        // 			this.selectedCell = clickedCell;
        // 			this.status = Status.clicked;
        // 			updateAvailableCells(selectedCell);
        // 		}
        // 		break;

        // 	case clicked:
        // 		if (isPlayerCell(clickedCell, currPlayer)) {
        // 			this.selectedCell = clickedCell;
        // 			clearAvailableCells();
        // 			updateAvailableCells(selectedCell);
        // 			notifyObserver();
        // 			break;
        // 		}

        // 		if (currentPlayerIndex == 0) {
        // 			if (players.get(1).getCellCoords().contains(clickedCell)) break;

        // 		} else if (currentPlayerIndex == 1) {
        // 			if (players.get(0).getCellCoords().contains(clickedCell)) break;
        // 		}

        // 		this.status = Status.notSelected;
        // 		Attack(clickedCell);
        // 		break;
        // }
    }

    private void ClearAvailableCells() {
        foreach (ButtonObj button in buttonList) {
            if (button.currState.Equals(State.border)) button.currState = State.empty;
        }
    }

    private List<ButtonObj> FindAvailableCells(ButtonObj clickedCell) {
        List<ButtonObj> retList = FindNeighbors(clickedCell, 2);
        foreach (ButtonObj b in retList) {
            if (b.GetState().Equals(State.cat) || b.GetState().Equals(State.dog)) {
                retList.Remove(b);
            }
        }
        return retList;
    }

    private void UpdateAvailableCells(ButtonObj selected) {
        List<ButtonObj> retList = FindAvailableCells(selected);
        foreach (ButtonObj b in retList) {
            if (WithinBoundary(b.coord)) b.currState = State.border;
        }
    }

    private void EndTurn() {
        // currentPlayerIndex = (currentPlayerIndex == 0) ? 1 : 0;
        if (!CanContinue()) {
            GameOver();
            return;
        }
        // ClearAvailableCells();
        turnCount++;
    }

    private bool CanContinue() {
        return turnCount < LINE_COUNT * LINE_COUNT;
    }
}
