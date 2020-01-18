﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    public static GameController instance = null;
    private TitleController titleController;
    public ButtonObj[] buttonList;
    public ButtonObj selectedButton;
    public readonly int LINE_COUNT = 7;
    public int turnCount = 0;
    public enum Status { notSelected, clicked };
    private Status status = Status.notSelected;
    private Player catPlayer;
    private Player dogPlayer;
    private Player[] players = new Player[2];
    private int currPlayerIndex;
    public string gameMode = "PVE"; // temp. will add choosing game mode.
    private int NEARBY = 2;

    private delegate bool Function(int x, int y, int X, int Y);
    private delegate int Find(ButtonObj btn1, ButtonObj btn2, State state);

    void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(this);
        DontDestroyOnLoad(this);
        titleController = GetComponent<TitleController>();
        InitGame();
    }

    // void Start() {
    //     InitGame();
    // }

    void InitGame() {
        InitGameMode();
        SetGameControllerReferenceOnButtons();
        InitPlayers();
        InitButtons();
        DrawBoard();
        StartTurn();
    }

    public void SetGameControllerReferenceOnButtons() {
        foreach (ButtonObj button in buttonList) {
            button.SetGameControllerReference(this);
            button.parentButton.interactable = true;
        }
    }

    private void InitGameMode() {
        this.gameMode = titleController.GetGameMode();
        catPlayer.SetLevel(titleController.GetLevel(0));
        dogPlayer.SetLevel(titleController.GetLevel(1));
    }

    private void InitPlayers() {
        catPlayer = gameObject.AddComponent<Player>();
        dogPlayer = gameObject.AddComponent<Player>();
        switch(gameMode) {
            case "PVE":
                catPlayer.SetIsAI(false);
                dogPlayer.SetIsAI(true);
                break;
            case "EVE":
                catPlayer.SetIsAI(true);
                dogPlayer.SetIsAI(true);
                break;
            default:
                catPlayer.SetIsAI(false);
                dogPlayer.SetIsAI(false);
                break;
        }
        catPlayer.SetPlayerIndex(0);
        dogPlayer.SetPlayerIndex(1);
        players[0] = catPlayer;
        players[1] = dogPlayer;
        currPlayerIndex = 0;
    }

    private void InitButtons() {
        // TODO: get stage info and make buttons with using obstacles.
        buttonList[0].SetState(State.cat);
        players[0].AddButton(buttonList[0]);
        buttonList[1].SetState(State.cat);
        players[0].AddButton(buttonList[1]);
        
        buttonList[5].SetState(State.dog);
        players[1].AddButton(buttonList[5]);
        buttonList[6].SetState(State.dog);
        players[1].AddButton(buttonList[6]);
        
        buttonList[42].SetState(State.dog);
        players[1].AddButton(buttonList[42]);
        buttonList[43].SetState(State.dog);
        players[1].AddButton(buttonList[43]);
        
        buttonList[47].SetState(State.cat);
        players[0].AddButton(buttonList[47]);
        buttonList[48].SetState(State.cat);
        players[0].AddButton(buttonList[48]);
    }

    void StartTurn() {
        Player currPlayer = players[currPlayerIndex];
        if (!currPlayer.isAI) return;
        RunAuto(currPlayer.GetLevel());
    }

    void RunAuto(string level) {
        Player currPlayer = players[currPlayerIndex];
        List<ButtonObj> buttons = currPlayer.GetButtonObjs();
        switch(level) {
            case "easy":
                StartCoroutine(RunEasyMode(buttons));
                break;
            case "normal":
                StartCoroutine(RunNormalMode(buttons));
                break;
            case "hard":
                StartCoroutine(RunHardMode(buttons));
                break;
            default:
                StartCoroutine(RunEasyMode(buttons));
                break;
        }
    }

    private IEnumerator RunEasyMode(List<ButtonObj> buttons) {
        int pos = Random.Range(0, buttons.Count);
        yield return new WaitForSeconds(1f);
        ClickEvent(buttons[pos]);
        List<ButtonObj> selectable = new List<ButtonObj>();
        foreach (ButtonObj button in buttonList) {
            if (IsBorder(button.currState)) selectable.Add(button);
        }
        yield return new WaitForSeconds(1f);
        ClickEvent(selectable[Random.Range(0, selectable.Count)]);
    }

    private IEnumerator RunNormalMode(List<ButtonObj> buttons) {
        (ButtonObj currButton, ButtonObj nextButton) = GetNextMove(buttons, FindGain);
        yield return new WaitForSeconds(1f);
        ClickEvent(currButton);
        yield return new WaitForSeconds(1f);
        ClickEvent(nextButton);
    }

    private IEnumerator RunHardMode(List<ButtonObj> buttons) {
        (ButtonObj currButton, ButtonObj nextButton) = GetNextMove(buttons, FindNet);
        yield return new WaitForSeconds(1f);
        ClickEvent(currButton);
        yield return new WaitForSeconds(1f);
        ClickEvent(nextButton);
    }

    private (ButtonObj, ButtonObj) GetNextMove(List<ButtonObj> buttons, Find f) {
        int max = -1;
        ButtonObj currButton = null;
        ButtonObj nextButton = null;
        foreach (ButtonObj button in buttons) {
            List<ButtonObj> neighbors = FindAvailableButtons(button);
            foreach (ButtonObj neighbor in neighbors) {
                int net = f(button, neighbor, GetEnemy(currPlayerIndex));
                if (net > max) {
                    currButton = button;
                    nextButton = neighbor;
                    max = net;
                } else if (net == max) {
                    bool rand = Random.Range(0, 1) > 0.5;
                    currButton = rand ? currButton : button;
                    nextButton = rand ? nextButton : neighbor;
                    max = rand ? max : net;
                }
            }
        }
        return (currButton, nextButton);
    }

    private int FindNet(ButtonObj currButton, ButtonObj selectedButton, State enemyState) {
        return FindGain(currButton, selectedButton, enemyState) - FindLoss(currButton, enemyState);
    }

    private int FindGain(ButtonObj currButton, ButtonObj selectedButton, State enemyState) {
        int gain = 0;
        if (GetDistance(currButton, selectedButton) == 1) gain++;
        foreach (ButtonObj neighbor in FindNeighbors(selectedButton, 1)) if (neighbor.currState.Equals(enemyState)) gain++;
        return gain;
    }

    private int FindLoss(ButtonObj currButton, State enemyState) {
        int loss = 0;
        foreach (ButtonObj neighbor in FindNeighbors(currButton, 2)) if (neighbor.currState.Equals(enemyState)) loss--;
        return loss;
    }

    public void ClickEvent(ButtonObj clickedButton) {
        Player currPlayer = players[currPlayerIndex];
        switch (status) {
            case Status.notSelected:
                if (IsCurrPlayerButton(clickedButton, currPlayer)) {
                    this.selectedButton = clickedButton;
                    this.status = Status.clicked;
                    UpdateBorders(selectedButton);
                    
                    if (clickedButton.currState.Equals(State.cat)) {
                        clickedButton.catClickedSound.Play();
                    } else if (clickedButton.currState.Equals(State.dog)) {
                        //clickedButton.dogClickedSound.Play();
                    }
                }
                break;

            case Status.clicked:
                if (IsCurrPlayerButton(clickedButton, currPlayer)) {
                    this.selectedButton = clickedButton;
                    
                    if (clickedButton.currState.Equals(State.cat)) {
                        clickedButton.catClickedSound.Play();
                    } else if (clickedButton.currState.Equals(State.dog)) {
                        //clickedButton.dogClickedSound.Play();
                    }
                    ClearAvailableButtons();
                    UpdateBorders(selectedButton);
                } else {
                    if (clickedButton.currState.Equals(State.nearBorder) || clickedButton.currState.Equals(State.farBorder)) {
                        this.status = Status.notSelected;
                        Attack(clickedButton);
                    }
                    else ClearAvailableButtons();
                }
                break;

        }
        DrawBoard();
    }

    private bool IsCurrPlayerButton(ButtonObj clickedButton, Player currPlayer) {
        return currPlayer.GetButtonObjs().Contains(clickedButton);
    }

    private void UpdateBorders(ButtonObj selected) {
        List<ButtonObj> retList = FindAvailableButtons(selected);
        foreach (ButtonObj b in retList) {
            if (GetDistance(b, selected) == 1) b.currState = State.nearBorder;
            if (GetDistance(b, selected) == 2) b.currState = State.farBorder;
        }
    }

    private void DrawBoard() {
        foreach (ButtonObj currButton in buttonList) {
            currButton.UpdateImg();
        }
    }

    void GameOver() {
        foreach (ButtonObj button in buttonList) {
            SetStatus(button.coord, false);
        }
    }

    private List<ButtonObj> FindAvailableButtons(ButtonObj btn) {
        return VisitNeighbors(btn, NEARBY, IsAvailable);
    }

    private List<ButtonObj> FindNeighbors(ButtonObj btn, int gap) {
        return VisitNeighbors(btn, gap, IsNeighbor);
    }

    private bool IsNeighbor(int x, int y, int X, int Y) {
        return (x != X || y != Y) && WithinBoundary(x, y);
    }

    private bool IsAvailable(int x, int y, int X, int Y) {
        return IsNeighbor(x, y, X, Y) && buttonList[GetPosition(x, y)].GetState().Equals(State.empty);
    }

    private List<ButtonObj> VisitNeighbors(ButtonObj btn, int gap, Function f) {
        List<ButtonObj> retList = new List<ButtonObj>();
        Pair coord = btn.coord;
        for (int x = coord.X - gap; x <= coord.X + gap; x++) {
            for (int y = coord.Y - gap; y <= coord.Y + gap; y++) {
                if (f(x, y, coord.X, coord.Y)) {
                    retList.Add(buttonList[GetPosition(x, y)]);
                }
            }
        }
        return retList;
    }

    private void SetStatus(Pair coord, bool status) {
        int pos = GetPosition(coord);
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

    private int GetDistance(ButtonObj from, ButtonObj to) {
        return GetDistance(from.GetCoord(), to.GetCoord());
    }

    private void Attack(ButtonObj clickedButton) {
        int dist = GetDistance(selectedButton, clickedButton);
        if (dist > 2) {
            ClearAvailableButtons();
            return;
        } else if (dist == 2) {
            RemoveCurrentButton();
        }
        MoveButton(clickedButton);
        ConsumeButton(clickedButton);
        EndTurn();
    }

    private void ConsumeButton(ButtonObj clickedButton) {
        ClearAvailableButtons();

        List<ButtonObj> neighbors = FindNeighbors(clickedButton, 1);

        foreach (ButtonObj neighbor in neighbors) {
            if (IsEnemy(neighbor)) {
                neighbor.SetState(GetEnemy(neighbor));
                players[currPlayerIndex].AddButton(neighbor);
                players[(currPlayerIndex == 0) ? 1 : 0].RemoveButton(neighbor);        
            }
        }
    }

    private void MoveButton(ButtonObj clickedButton) {
        ClearAvailableButtons();
        int pos = GetPosition(clickedButton);
        State updatedState = currPlayerIndex == 0 ? State.cat : State.dog;
        buttonList[pos].SetState(updatedState);
        players[currPlayerIndex].AddButton(clickedButton);
    }

    private void RemoveCurrentButton() {
        selectedButton.SetState(State.empty);
        players[currPlayerIndex].RemoveButton(selectedButton);
    }

    private void ClearAvailableButtons() {
        foreach (ButtonObj button in buttonList) {
            if (IsBorder(button.currState)) button.currState = State.empty;
        }
    }

    private bool IsBorder(State state) {
        return state.Equals(State.nearBorder) || state.Equals(State.farBorder);
    }

    private bool IsEnemy(ButtonObj button) {
        return IsEnemy(button.currState);
    }

    private bool IsEnemy(State state) {
        return (currPlayerIndex == 0 && state.Equals(State.dog)) || (currPlayerIndex == 1 && state.Equals(State.cat));
    }

    private State GetEnemy(int playerIndex) {
        return playerIndex == 0 ? State.dog : State.cat;
    }

    private State GetEnemy(ButtonObj button) {
        return GetEnemy(button.currState);
    }

    private State GetEnemy(State state) {
        return state.Equals(State.cat) ? State.dog : State.cat;
    }

    private void EndTurn() {
        currPlayerIndex = (currPlayerIndex == 0) ? 1 : 0;
        if (!CanContinue()) {
            GameOver();
            return;
        }
        ClearAvailableButtons();
        turnCount++;
        StartTurn();
    }

    private bool CanContinue() {
        int catCount = catPlayer.GetButtonObjs().Count;
        int dogCount = dogPlayer.GetButtonObjs().Count;
        return catCount != 0 && dogCount != 0 && catCount + dogCount < LINE_COUNT * LINE_COUNT;
    }
}
