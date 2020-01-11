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

    void Awake()
    {
        StartGame();
    }

    void StartGame()
    {
        SetGameControllerReferenceOnButtons();
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
        List<Pair> neighbors = FindNeighbors(clickedCell, 2);
        foreach (Pair coord in neighbors)
        {
            int pos = GetPosition(coord.X, coord.Y);
            switch (state) {
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
    }

    public void GameOver()
    {
        for (int i = 0; i < buttonList.Length; i++)
        {
            SetStatus(buttonList[i].coord, false);
        }
        // JOptionPane.showMessageDialog(null, "Game over!", "GAME OVER", JOptionPane.INFORMATION_MESSAGE);
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
                if (WithinBoundary(x, y))
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
        buttonList[pos].button.interactable = status;
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
        int distance = GetDistance(selectedCell, clickedCell);
        if (distance > 2)
        {
            ClearAvailableCells();
            return;
        }
        if (distance == 2)
        {
            RemoveCurrentCell();
        }
        MoveCell(clickedCell);
        // ConsumeCell(clickedCell);
        EndTurn();
    }

    private void MoveCell(Pair clickedCell)
    {
        ClearAvailableCells();
        int pos = GetPosition(clickedCell.X, clickedCell.Y);
        buttonList[pos].buttonText.text = "X";
        // board.put(clickedCell, currentPlayerIndex);
        // players.get(currentPlayerIndex).add(clickedCell.getX(), clickedCell.getY());
    }

    private void RemoveCurrentCell()
    {
        selectedCell = null;
    }

    public void ClickEvent(Pair clickedCell)
    {
        if (selectedCell.Equals(null))
        {
            selectedCell = clickedCell;
            DrawBoard(clickedCell);
        }
        else
        {
            int pos = GetPosition(clickedCell);
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
        turnCount++;
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
        foreach (BoardButton button in buttonList)
        {
            if (button.buttonText.text == "O") button.buttonText.text = null;
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
        ClearAvailableCells();
        turnCount++;
    }

    private bool CanContinue()
    {
        return turnCount < LINE_COUNT * LINE_COUNT;
    }
}
