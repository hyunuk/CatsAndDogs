using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
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

    void Start() {
        StartGame();
    }

    void StartGame() {
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

    private void InitPlayers() {
        if (String.Equals(gameMode, "PVE")) {
            catPlayer = gameObject.AddComponent<Player>();
            catPlayer.SetPlayerIndex(0);
            catPlayer.SetIsAI(false);
            dogPlayer = gameObject.AddComponent<Player>();
            dogPlayer.SetPlayerIndex(1);
            dogPlayer.SetIsAI(true);
        } else if (String.Equals(gameMode, "EVE")) {
            catPlayer = gameObject.AddComponent<Player>();
            catPlayer.SetPlayerIndex(0);
            catPlayer.SetIsAI(true);
            dogPlayer = gameObject.AddComponent<Player>();
            dogPlayer.SetPlayerIndex(1);
            dogPlayer.SetIsAI(true);
        } else {
            catPlayer = gameObject.AddComponent<Player>();
            catPlayer.SetPlayerIndex(0);
            catPlayer.SetIsAI(false);
            dogPlayer = gameObject.AddComponent<Player>();
            dogPlayer.SetPlayerIndex(1);
            dogPlayer.SetIsAI(false);
        }
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

    public void StartTurn() {
        Player currPlayer = players[currPlayerIndex];
        if (!currPlayer.isAI) return;
        RunAuto(currPlayer.level);
        //RunAuto(currPlayer.level);
    }

    public void RunAuto(string level) {
        Player currPlayer = players[currPlayerIndex];
        List<ButtonObj> buttons = currPlayer.GetButtonObjs();
        switch(level) {
            case "easy":
                StartCoroutine(RunEasyMode(currPlayer, buttons));
                break;
            case "normal":
                StartCoroutine(RunNormalMode(currPlayer, buttons));
                break;
            default:
                StartCoroutine(RunEasyMode(currPlayer, buttons));
                break;
        }
    }

    public IEnumerator RunEasyMode(Player currPlayer, List<ButtonObj> buttons) {
        int pos = UnityEngine.Random.Range(0, buttons.Count);
        yield return new WaitForSeconds(1f);
        ClickEvent(buttons[pos]);
        List<ButtonObj> selectable = new List<ButtonObj>();
        foreach (ButtonObj button in buttonList) {
            if (button.currState.Equals(State.nearBorder) || button.currState.Equals(State.farBorder)) selectable.Add(button);
        }
        yield return new WaitForSeconds(1f);
        ClickEvent(selectable[UnityEngine.Random.Range(0, selectable.Count)]);
    }

    public IEnumerator RunNormalMode(Player currPlayer, List<ButtonObj> buttons) {
        int max = -1;
        ButtonObj currButton = null;
        ButtonObj nextButton = null;
        foreach (ButtonObj button in buttons) {
            // yield return new WaitForSeconds(1f);
            // UpdateBorders(button);
            List<ButtonObj> neighbors = FindAvailableButtons(button);
            foreach (ButtonObj neighbor in neighbors) {
                int net = FindNet(button, neighbor);
                Debug.Log(net);
                if (net > max) {
                    currButton = button;
                    nextButton = neighbor;
                    max = net;
                }
            }
            // yield return new WaitForSeconds(1f);
            // ClearAvailableButtons();
        }
        yield return new WaitForSeconds(1f);
        ClickEvent(currButton);
        yield return new WaitForSeconds(1f);
        ClickEvent(nextButton);
    }

    private int FindNet(ButtonObj currButton, ButtonObj selectedButton) {
        int net = 0;
        if (GetDistance(currButton, selectedButton) == 1) net++;
        List<ButtonObj> neighbors = FindNeighbors(selectedButton, 1);
        foreach (ButtonObj neighbor in neighbors) {
            State enemyState = currPlayerIndex == 0 ? State.dog : State.cat;
            if (neighbor.currState.Equals(enemyState)) net++;
        }
        return net;
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

    private List<ButtonObj> FindAvailableButtons(ButtonObj clickedButton) {
        List<ButtonObj> retList = new List<ButtonObj>();
        Pair coord = clickedButton.coord;
        for (int x = coord.X - NEARBY; x <= coord.X + NEARBY; x++) {
            for (int y = coord.Y - NEARBY; y <= coord.Y + NEARBY; y++) {
                if ((x != coord.X || y != coord.Y) && WithinBoundary(x, y)) {
                    ButtonObj temp = buttonList[GetPosition(new Pair(x, y))];
                    if (temp.GetState().Equals(State.empty)) retList.Add(temp);
                }
            }
        }
        return retList;
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
                if ((x != coord.X || y != coord.Y) && WithinBoundary(x, y)) {
                    ButtonObj temp = buttonList[GetPosition(new Pair(x, y))];
                    retList.Add(temp);
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
            if (currPlayerIndex == 0 && neighbor.GetState().Equals(State.dog)) {
                neighbor.SetState(State.cat);
                catPlayer.AddButton(neighbor);
                dogPlayer.RemoveButton(neighbor);
            } else if (currPlayerIndex == 1 && neighbor.GetState().Equals(State.cat)) {
                neighbor.SetState(State.dog);
                dogPlayer.AddButton(neighbor);
                catPlayer.RemoveButton(neighbor);
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
            if ((button.currState.Equals(State.nearBorder) || button.currState.Equals(State.farBorder)) && (!button.currState.Equals(State.cat) && !button.currState.Equals(State.dog))) button.currState = State.empty;
        }
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
