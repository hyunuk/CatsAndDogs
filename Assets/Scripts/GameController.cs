using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public ButtonObj[] buttonList;
    public Pair selectedCell; // null if no selected cell exists
    public readonly int LINE_COUNT = 7;
    public int turnCount = 0;
    public enum Status { notSelected, clicked };

    void Awake()
    {
        StartGame();
    }

    void StartGame()
    {
        SetGameControllerReferenceOnButtons();
        buttonList[0].SetState(State.cat);
        buttonList[1].SetState(State.cat);
        buttonList[5].SetState(State.dog);
        buttonList[6].SetState(State.dog);
        buttonList[42].SetState(State.dog);
        buttonList[43].SetState(State.dog);
        buttonList[47].SetState(State.cat);
        buttonList[48].SetState(State.cat);
    }

    public void SetGameControllerReferenceOnButtons()
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            buttonList[i].SetGameControllerReference(this);
            buttonList[i].parentButton.interactable = true;
        }
    }

    public void DrawBoard(Pair clickedCell)
    {
        Debug.Log("on DrawBoard");
        List<Pair> neighbors = FindNeighbors(clickedCell, 2);
        Debug.Log(neighbors.Count);
        foreach (Pair coord in neighbors)
        {
            int pos = GetPosition(coord.X, coord.Y);
            ButtonObj curr = buttonList[pos];
            switch (curr.currState)
            {
                case State.empty:
                    curr.SetButtonImage(State.empty);
                    break;

                case State.cat:
                    curr.SetButtonImage(State.cat);
                    break;

                case State.dog:
                    curr.SetButtonImage(State.dog);
                    break;

                case State.border:
                    curr.SetButtonImage(State.border);
                    break;

                case State.obstacle:
                    curr.SetButtonImage(State.obstacle);
                    break;
            }
        }
    }

    public void GameOver()
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            SetStatus(buttonList[i].coord, false);
        }
    }

    private List<Pair> FindNeighbors(Pair coord, int gap)
    {
        // BoardButton[] retList = new BoardButton[];
        List<Pair> retList = new List<Pair>();
        int X = coord.X, Y = coord.Y;
        for (int x = X - gap; x <= X + gap; x++)
        {
            for (int y = Y - gap; y <= Y + gap; y++)
            {
                if (x != X && y != Y && WithinBoundary(x, y))
                {
                    retList.Add(new Pair(x, y));
                }
            }
        }
        return retList;
    }

    private void SetStatus(Pair coord, bool status)
    {
        int pos = GetPosition(coord.X, coord.Y);
        buttonList[pos].parentButton.interactable = status;
    }

    private bool WithinBoundary(int x, int y)
    {
        return 0 <= x && x < LINE_COUNT && 0 <= y && y < LINE_COUNT;
    }

    private int GetPosition(Pair coord)
    {
        return GetPosition(coord.X, coord.Y);
    }

    private int GetPosition(int x, int y)
    {
        return x + y * LINE_COUNT;
    }

    private int GetDistance(Pair from, Pair to)
    {
        int xDist = from.X - to.X;
        int yDist = from.Y - to.Y;
        return (int)Math.Sqrt(xDist * xDist + yDist * yDist);
    }

    private void Attack(Pair clickedCell)
    {
        RemoveCurrentCell();
        MoveCell(clickedCell);
        // ConsumeCell(clickedCell);
        EndTurn();
    }

    private void MoveCell(Pair clickedCell)
    {
        ClearAvailableCells();
        int pos = GetPosition(clickedCell);
        // TODO: implement
        buttonList[pos].SetButtonImage(State.cat);
        // board.put(clickedCell, currentPlayerIndex);
        // players.get(currentPlayerIndex).add(clickedCell.getX(), clickedCell.getY());
    }

    private void RemoveCurrentCell()
    {
        selectedCell = null;
    }

    public void ClickEvent(Pair clickedCell)
    {
        Debug.Log("on ClickEvent");
        if (selectedCell.Equals(null))
        {
            Debug.Log("selectedCell is null");
            DrawBoard(clickedCell);
        }
        else
        {
            Debug.Log("selectedCell is NOT null");
            int pos = GetPosition(clickedCell);

            if (buttonList[pos].currState.Equals(State.border))
            {
                Attack(clickedCell);
            }
            else
            {
                return;
            }
            // TODO
        }

        // if (gameController.turnCount < gameController.LINE_COUNT * gameController.LINE_COUNT) {
        //     buttonText.text = playerSide;
        //     //sprite.texture
        //     // button.interactable = false;
        //     gameController.DrawBoard(this);
        // } else {
        //     gameController.GameOver();
        // }

        selectedCell = clickedCell;
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

    private void ClearAvailableCells()
    {
        foreach (ButtonObj button in buttonList)
        {
            if (button.currState.Equals(State.border)) button.currState = State.empty;
        }
    }

    private void EndTurn()
    {
        // currentPlayerIndex = (currentPlayerIndex == 0) ? 1 : 0;

        if (!CanContinue())
        {
            GameOver();
            return;
        }
        // ClearAvailableCells();
        turnCount++;
    }

    private bool CanContinue()
    {
        return turnCount < LINE_COUNT * LINE_COUNT;
    }
}
