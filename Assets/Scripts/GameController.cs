using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour {
    private const int DEPTH = 1;
    private const int NEARBY = 2;
    public static int LINE_COUNT = 7;

    public ButtonObj[] buttonList;
    public State[][] board;
    public ButtonObj selectedButton;
    public enum Status { notSelected, clicked };

    public int turnCount = 0;
    private Status status = Status.notSelected;
    private Player catPlayer;
    private Player dogPlayer;
    private Player[] players = new Player[2];
    private int currPlayerIndex;
    public string gameMode;

    private TitleController controller;
    public GameObject endGamePanel;
    public GameObject winnerText;
    public Text catScore;
    public Text dogScore;
    public Text catName;
    public Text dogName;
    public Text catTurnInfo;
    public Text dogTurnInfo;

    private delegate bool Function(int x, int y, int X, int Y);
    private delegate int Find(ButtonObj button1, ButtonObj button2, State state);

    void Awake() {
        controller = GameObject.FindWithTag("TitleController").GetComponent<TitleController>();
        InitGame();
    }

    void InitGame() {
        endGamePanel.SetActive(false);
        SetGameControllerReferenceOnButtons();
        InitPlayers();
        LoadStage();
        DrawBoard();
        InitInfo();
        StartTurn();
    }

    public void SetGameControllerReferenceOnButtons() {
        foreach (ButtonObj button in buttonList) {
            button.SetGameControllerReference(this);
            button.parentButton.interactable = true;
        }
    }

    private void InitPlayers() {
        catPlayer = gameObject.AddComponent<Player>();
        dogPlayer = gameObject.AddComponent<Player>();
        catPlayer.Difficulty = controller.GetLevel();
        dogPlayer.Difficulty = controller.GetLevel();
        SetGameMode();
        catPlayer.PlayerIndex = 0;
        dogPlayer.PlayerIndex = 1;
        players[0] = catPlayer;
        players[1] = dogPlayer;

        currPlayerIndex = 0;
    }

    private void SetGameMode() {
        this.gameMode = controller.GetGameMode();
        switch (gameMode) {
            case "PVE":
                catPlayer.IsAI = false;
                dogPlayer.IsAI = true;
                break;
            case "EVE":
                catPlayer.IsAI = true;
                dogPlayer.IsAI = true;
                break;
            default:
                catPlayer.IsAI = false;
                dogPlayer.IsAI = false;
                break;
        }
    }

    private void LoadStage() {
        // TODO: get stage info and make buttons with using obstacles.
        board = new State[LINE_COUNT][];
        Dictionary<Pair, State> stage = Stages.Stage_1;
        for (int y = 0; y < board.Length; y++) {
            board[y] = new State[LINE_COUNT];
            for (int x = 0; x < board[y].Length; x++) {
                Pair curr = new Pair(x, y);
                if (stage.ContainsKey(curr)) {
                    board[y][x] = stage[curr];
                } else {
                    board[y][x] = State.empty;
                }
            }
        }
    }

    private void DrawBoard() {
        for (int y = 0; y < board.Length; y++) {
            for (int x = 0; x < board.Length; x++) {
                int pos = Util.GetPosition(x, y);
                State currState = board[y][x];
                buttonList[pos].CurrState = currState;
            }
        }
    }

    private void InitInfo() {
        catName.text = catPlayer.IsAI ? "Computer (" + catPlayer.Difficulty + ")" : "Player";
        dogName.text = dogPlayer.IsAI ? "Computer (" + dogPlayer.Difficulty + ")" : "Player";
        catTurnInfo.enabled = true;
        dogTurnInfo.enabled = false;
        catScore.text = GetButtons(State.cat).Count.ToString();
        dogScore.text = GetButtons(State.dog).Count.ToString();
    }

    void StartTurn() {
        Player currPlayer = players[currPlayerIndex];
        if (currPlayer.IsAI) RunAuto(currPlayer.Difficulty);
    }

    Find GetFind(string level) {
        switch(level) {
            case "easy":
                return FindRandom;
            case "normal":
                return FindGain;
            case "hard":
                return FindMinimax;
            default:
                throw new ArgumentException(String.Format("{0} is not a valid difficulty level!", level));
        }
    }

    void RunAuto(string level) {
        (ButtonObj currentButton, ButtonObj nextButton) = GetNextMove(GetFind(level));
        StartCoroutine(RunCommon(currentButton, nextButton));
    }

    private IEnumerator RunCommon(ButtonObj currentButton, ButtonObj nextButton) {
        yield return new WaitForSeconds(0.5f);
        ClickEvent(currentButton);
        yield return new WaitForSeconds(0.5f);
        ClickEvent(nextButton);
    }

    private (ButtonObj, ButtonObj) GetNextMove(Find FindNet) {
        int max = Int32.MinValue;
        ButtonObj currentButton = null;
        ButtonObj nextButton = null;
        List<ButtonObj> buttons = GetButtons(Util.GetPlayerState(currPlayerIndex));
        foreach (ButtonObj button in buttons) {
            List<ButtonObj> neighbors = FindAvailableButtons(button);
            foreach (ButtonObj neighbor in neighbors) {
                int net = FindNet(button, neighbor, Util.GetEnemyState(currPlayerIndex));
                if (net > max) {
                    currentButton = button;
                    nextButton = neighbor;
                    max = net;
                } else if (net == max) {
                    bool rand = Random.Range(0, 1) > 0.5;
                    currentButton = rand ? currentButton : button;
                    nextButton = rand ? nextButton : neighbor;
                    max = rand ? max : net;
                }
            }
        }
        return (currentButton, nextButton);
    }

    // https://www.geeksforgeeks.org/minimax-algorithm-in-game-theory-set-1-introduction/
    public int minimax(int depth, bool isMax, ButtonObj currentButton, ButtonObj selectedButton, State enemyState, int h) {
        // TODO: update board to reflect choice made
        int score;
        if (depth == h) {
            score = FindScore(currentButton, selectedButton, enemyState);
        } else {
            List<ButtonObj> availableButtons = FindAvailableButtons(currentButton);
            List<int> arr = new List<int>();
            foreach (ButtonObj button in availableButtons) {
                arr.Add(minimax(depth+1, !isMax, selectedButton, button, enemyState == State.cat ? State.dog : State.cat, h));
            }
            score = isMax ? arr.Max() : arr.Min();
        }
        Debug.Log(score);
        return score;
    } 

    public int Log2(int n) => (n==1)? 0 : 1 + Log2(n/2);

    private int FindRandom(ButtonObj currentButton, ButtonObj selectedButton, State enemyState) {
        return Random.Range(0, 5);
    }

    private int FindMinimax(ButtonObj currentButton, ButtonObj selectedButton, State enemyState) {
        return minimax(0, true, currentButton, selectedButton, enemyState, DEPTH);
    }

    private int FindScore(ButtonObj currentButton, ButtonObj selectedButton, State enemyState) {
        return FindGain(currentButton, selectedButton, enemyState) + FindLoss(currentButton, enemyState);
    }

    private int FindGain(ButtonObj currentButton, ButtonObj selectedButton, State enemyState) {
        int gain = 0;
        if (Util.GetDistance(currentButton, selectedButton) == 1) gain++;
        foreach (ButtonObj neighbor in FindNeighbors(selectedButton, 1)) if (neighbor.CurrState == enemyState) gain++;
        return gain;
    }

    private int FindLoss(ButtonObj currentButton, State enemyState) {
        int loss = 0;
        foreach (ButtonObj neighbor in FindNeighbors(currentButton, 2)) if (neighbor.CurrState == enemyState) loss--;
        return loss;
    }

    public void ClickEvent(ButtonObj clickedButton) {
        bool isCurrPlayerButton = IsCurrPlayerButton(clickedButton);
        if (isCurrPlayerButton) {
            selectedButton = clickedButton;
            status = Status.clicked;
            ClearBorders();
            UpdateBorders();
            PlayAudio(clickedButton);
        }

        if (status == Status.clicked && !isCurrPlayerButton) {
            ClearBorders();
            Attack(selectedButton, clickedButton, currPlayerIndex);
            status = Status.notSelected;
        }
        DrawBoard();
    }
    
    private void Attack(ButtonObj selectedButton, ButtonObj clickedButton, int currPlayerIndex) {
        int dist = Util.GetDistance(selectedButton, clickedButton);
        Pair selectedCoord = selectedButton.Coord;
        State opposite = Util.GetEnemyState(currPlayerIndex);

        if (board[selectedCoord.Y][selectedCoord.X] == State.empty || board[selectedCoord.Y][selectedCoord.X] == opposite) {
            return;
        }
        if (dist <= 2) {
            if (dist == 2) RemoveCurrentButton(selectedButton);
            MoveButton(clickedButton);
            ConsumeButton(clickedButton);
            DrawBoard();
            EndTurn();
        }
    }

    private void PlayAudio(ButtonObj button) {
        switch(button.CurrState) {
            case State.cat:
                button.catClickedSound.Play();
                break;
            case State.dog:
                // button.dogClickedSound.Play();
                break;
        }
    }

    private bool IsCurrPlayerButton(ButtonObj clickedButton) {
        return clickedButton.CurrState == Util.GetPlayerState(currPlayerIndex);
    }

    private void UpdateBorders() {
        List<ButtonObj> retList = FindAvailableButtons(selectedButton);
        foreach (ButtonObj b in retList) {
            int distance = Util.GetDistance(b, selectedButton);
            Debug.Log(distance);
            if (distance == 1) {
                b.CurrState = State.nearBorder;
                board[b.Coord.Y][b.Coord.X] = State.nearBorder;
            } else if (distance == 2) {
                b.CurrState = State.farBorder;
                board[b.Coord.Y][b.Coord.X] = State.farBorder;
            }
        }
    }

    void GameOver() {
        int catCount = GetButtons(State.cat).Count;
        int dogCount = GetButtons(State.dog).Count;
        State winnerState = (catCount > dogCount) ? State.cat : State.dog;
        foreach (ButtonObj button in buttonList) {
            board[button.Coord.Y][button.Coord.X] = winnerState;
        }
        GetWinner(winnerState);
        endGamePanel.SetActive(true);
    }

    private void GetWinner(State winnerState) {
        winnerText.GetComponent<Text>().text = (winnerState == State.cat ? "Cat" : "Dog") + " Player Won!";
    }

    public void ClickRestartButton() {
        InitGame();
    }

    public void ClickBackToTitleButton() {
        SceneManager.LoadScene("TitleScene", LoadSceneMode.Single);
        SceneManager.UnloadSceneAsync("MainScene");
    }

    private List<ButtonObj> FindAvailableButtons(ButtonObj button) {
        return VisitNeighbors(button, NEARBY, IsAvailable);
    }

    private List<ButtonObj> FindNeighbors(ButtonObj button, int gap) {
        return VisitNeighbors(button, gap, IsNeighbor);
    }

    private bool IsNeighbor(int x, int y, int X, int Y) {
        return (x != X || y != Y) && Util.WithinBoundary(x, y);
    }

    private bool IsAvailable(int x, int y, int X, int Y) {
        return IsNeighbor(x, y, X, Y) && buttonList[Util.GetPosition(x, y)].CurrState == State.empty;
    }

    private List<ButtonObj> VisitNeighbors(ButtonObj button, int gap, Function f) {
        List<ButtonObj> retList = new List<ButtonObj>();
        Pair coord = button.Coord;
        for (int x = coord.X - gap; x <= coord.X + gap; x++) {
            for (int y = coord.Y - gap; y <= coord.Y + gap; y++) {
                if (f(x, y, coord.X, coord.Y)) {
                    retList.Add(buttonList[Util.GetPosition(x, y)]);
                }
            }
        }
        return retList;
    }

    private void SetStatus(Pair coord, bool status) {
        int pos = Util.GetPosition(coord);
        buttonList[pos].parentButton.interactable = status;
    }

    private void ConsumeButton(ButtonObj clickedButton) {
        List<ButtonObj> neighbors = FindNeighbors(clickedButton, 1);
        foreach (ButtonObj neighbor in neighbors) {
            if (Util.IsEnemy(neighbor, currPlayerIndex)) {
                board[neighbor.Coord.Y][neighbor.Coord.X] = Util.GetPlayerState(currPlayerIndex);
            }
        }
    }

    private void MoveButton(ButtonObj clickedButton) {
        board[clickedButton.Coord.Y][clickedButton.Coord.X] = Util.GetPlayerState(currPlayerIndex);
    }

    private void RemoveCurrentButton(ButtonObj selectedButton) {
        board[selectedButton.Coord.Y][selectedButton.Coord.X] = State.empty;
    }

    private void ClearBorders() {
        foreach (ButtonObj button in buttonList) {
            if (Util.IsBorder(button.CurrState)) {
                board[button.Coord.Y][button.Coord.X] = State.empty;
            }
        }
    }

    private void EndTurn() {
        currPlayerIndex = (currPlayerIndex == 0) ? 1 : 0;
        catScore.text = GetButtons(State.cat).Count.ToString();
        dogScore.text = GetButtons(State.dog).Count.ToString();

        if (CanContinue()) {
            ClearBorders();
            turnCount++;
            catTurnInfo.enabled = currPlayerIndex == 0;
            dogTurnInfo.enabled = currPlayerIndex == 1;
            StartTurn();
        } else GameOver();
    }

    private bool CanContinue() {
        (ButtonObj currentButton, ButtonObj nextButton) = GetNextMove(FindScore);
        return !(nextButton is null);
    }

    private List<ButtonObj> GetButtons(State state) {
        List<ButtonObj> buttons = new List<ButtonObj>();
        foreach (ButtonObj button in buttonList) {
            if (state == button.CurrState) buttons.Add(button);
        }
        return buttons;
    }
}
