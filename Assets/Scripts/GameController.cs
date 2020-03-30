using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour {
    private const int DEPTH = 4;
    private const int NEARBY = 2;
    private const float SPEED = 0.2f;
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

    public void InitGame() {
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
        yield return new WaitForSeconds(SPEED);
        ClickEvent(currentButton);
        yield return new WaitForSeconds(SPEED);
        ClickEvent(nextButton);
    }

    private (ButtonObj, ButtonObj) GetNextMove(Find FindNet) {
        int max = Int32.MinValue;
        ButtonObj source = null;
        ButtonObj dest = null;
        List<ButtonObj> myButtons = GetButtons(Util.GetPlayerState(currPlayerIndex));
        foreach (ButtonObj myButton in myButtons) {
            List<ButtonObj> neighbors = FindAvailableButtons(myButton);
            foreach (ButtonObj neighbor in neighbors) {
                int net = FindNet(myButton, neighbor, Util.GetEnemyState(currPlayerIndex));
                if (net > max) {
                    source = myButton;
                    dest = neighbor;
                    max = net;
                } else if (net == max) {
                    bool rand = Random.Range(0, 1) > 0.5;
                    source = rand ? source : myButton;
                    dest = rand ? dest : neighbor;
                    max = rand ? max : net;
                }
            }
        }
        return (source, dest);
    }

    public void ClickEvent(ButtonObj clickedButton) {
        bool isCurrPlayerButton = Util.IsCurrPlayerButton(clickedButton, currPlayerIndex);

        if (isCurrPlayerButton) {
            selectedButton = clickedButton;
            status = Status.clicked;
            ClearBorders();
            DrawBoard();
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
        State selectedButtonState = board[selectedButton.Coord.Y][selectedButton.Coord.X];

        if (selectedButtonState == State.empty || Util.IsEnemy(selectedButtonState, currPlayerIndex)) return;
        if (dist <= 2) {
            if (dist == 2) board[selectedButton.Coord.Y][selectedButton.Coord.X] = State.empty;
            board[clickedButton.Coord.Y][clickedButton.Coord.X] = Util.GetPlayerState(currPlayerIndex);
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

    private void UpdateBorders() {
        List<ButtonObj> retList = FindAvailableButtons(selectedButton);
        foreach (ButtonObj b in retList) {
            int distance = Util.GetDistance(b, selectedButton);
            if (distance == 1) {
                b.CurrState = State.nearBorder;
                board[b.Coord.Y][b.Coord.X] = State.nearBorder;
            } else if (distance == 2) {
                b.CurrState = State.farBorder;
                board[b.Coord.Y][b.Coord.X] = State.farBorder;
            }
        }
    }

    private List<ButtonObj> FindAvailableButtons(ButtonObj button) {
        List<ButtonObj> availableButtons = new List<ButtonObj>();
        Pair coord = button.Coord;
        for (int x = coord.X - NEARBY; x <= coord.X + NEARBY; x++) {
            for (int y = coord.Y - NEARBY; y <= coord.Y + NEARBY; y++) {
                if (IsAvailable(x, y, coord.X, coord.Y)) {
                    availableButtons.Add(buttonList[Util.GetPosition(x, y)]);
                }
            }
        }
        return availableButtons;
    }

    private List<ButtonObj> FindNeighbors(ButtonObj button, int gap) {
        List<ButtonObj> neighbors = new List<ButtonObj>();
        Pair coord = button.Coord;
        for (int x = coord.X - gap; x <= coord.X + gap; x++) {
            for (int y = coord.Y - gap; y <= coord.Y + gap; y++) {
                if (IsNeighbor(x, y, coord.X, coord.Y)) {
                    neighbors.Add(buttonList[Util.GetPosition(x, y)]);
                }
            }
        }
        return neighbors;
    }

    private List<State> FindNeighbors(ButtonObj button, int gap, State[][] snapshot) {
        List<State> neighbors = new List<State>();
        Pair coord = button.Coord;
        for (int x = coord.X - gap; x <= coord.X + gap; x++) {
            for (int y = coord.Y - gap; y <= coord.Y + gap; y++) {
                if (IsNeighbor(x, y, coord.X, coord.Y)) {
                    neighbors.Add(snapshot[y][x]);
                }
            }
        }
        return neighbors;
    }

    private bool IsNeighbor(int x, int y, int X, int Y) {
        return (x != X || y != Y) && Util.WithinBoundary(x, y);
    }

    private bool IsAvailable(int x, int y, int X, int Y) {
        return IsNeighbor(x, y, X, Y) && buttonList[Util.GetPosition(x, y)].CurrState == State.empty;
    }

    // TODO: deprecated?
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

    private void ConsumeButton(ButtonObj clickedButton, State[][] snapshot) {
        List<ButtonObj> neighbors = FindNeighbors(clickedButton, 1);
        foreach (ButtonObj neighbor in neighbors) {
            if (neighbor.CurrState == Util.GetEnemyState(clickedButton.CurrState)) {
                snapshot[neighbor.Coord.Y][neighbor.Coord.X] = clickedButton.CurrState;
            }
        }
    }

    private List<ButtonObj> GetButtons(State state) {
        return buttonList.Where(button => button.CurrState == state).ToList();
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

    void GameOver() {
        int catCount = GetButtons(State.cat).Count;
        int dogCount = GetButtons(State.dog).Count;
        State winnerState = (catCount > dogCount) ? State.cat : State.dog;
        foreach (ButtonObj button in buttonList) {
            board[button.Coord.Y][button.Coord.X] = winnerState;
        }
        winnerText.GetComponent<Text>().text = (winnerState == State.cat ? "Cat" : "Dog") + " Player Won!";
        endGamePanel.SetActive(true);
    }

    public void ClickBackToTitleButton() {
        SceneManager.LoadScene("TitleScene", LoadSceneMode.Single);
        SceneManager.UnloadSceneAsync("MainScene");
    }

    // https://www.geeksforgeeks.org/minimax-algorithm-in-game-theory-set-1-introduction/
    public int Minimax(int depth, bool isMax, ButtonObj source, ButtonObj dest, State enemyState, int h, State[][] origBoard) {
        // TODO: update board to reflect choice made
        State[][] snapshot = DuplicateBoard(origBoard);
        
        int score = isMax? Int32.MinValue : Int32.MaxValue;
        if (depth == h) return FindScore(source, dest, enemyState, snapshot);
        
        List<ButtonObj> availableButtons = FindAvailableButtons(source);
        if (availableButtons.Count == 0) return score;

        List<int> scoreArr = new List<int>();
        foreach (ButtonObj button in availableButtons) {
            State currPlayerState = enemyState == State.cat ? State.dog : State.cat;
            if (Util.GetDistance(button, source) == 2) {
                snapshot[source.Coord.Y][source.Coord.X] = State.empty;
            }
            snapshot[button.Coord.Y][button.Coord.X] = currPlayerState;
            ConsumeButton(button, snapshot);
            scoreArr.Add(Minimax(depth+1, !isMax, dest, button, enemyState == State.cat ? State.dog : State.cat, h, snapshot));
        }
        return isMax ? scoreArr.Max() : scoreArr.Min();
    } 

    private State[][] DuplicateBoard(State[][] orig) {
        State[][] snapshot = new State[orig.Length][];
        for (int y = 0; y < orig.Length; y++) {
            snapshot[y] = new State[orig.Length];
            for (int x = 0; x < board[y].Length; x++) {
                snapshot[y][x] = orig[y][x];
            }
        }
        return snapshot;
    }

    public int Log2(int n) => (n==1)? 0 : 1 + Log2(n/2);

    private int FindMinimax(ButtonObj source, ButtonObj dest, State enemyState) {
        return Minimax(0, true, source, dest, enemyState, DEPTH, board);
    }

    private int FindScore(ButtonObj source, ButtonObj dest, State enemyState) {
        return FindGain(source, dest, enemyState) + FindLoss(source, enemyState);
    }

    private int FindScore(ButtonObj source, ButtonObj dest, State enemyState, State[][] snapshot) {
        return FindGain(source, dest, enemyState, snapshot) + FindLoss(source, enemyState, snapshot);
    }

    private int FindGain(ButtonObj source, ButtonObj dest, State enemyState) {
        return FindNeighbors(dest, 1).Count(neighbor => neighbor.CurrState == enemyState) + (Util.GetDistance(source, dest) == 1 ? 1 : 0);
    }

    private int FindGain(ButtonObj source, ButtonObj dest, State enemyState, State[][] snapshot) {
        return FindNeighbors(dest, 1, snapshot).Count(state => state == enemyState) + (Util.GetDistance(source, dest) == 1 ? 1 : 0);
    }

    private int FindLoss(ButtonObj source, State enemyState) {
        return -FindNeighbors(source, 2).Count(neighbor => neighbor.CurrState == enemyState);
    }

    private int FindLoss(ButtonObj source, State enemyState, State[][] snapshot) {
        return -FindNeighbors(source, 2, snapshot).Count(state => state == enemyState);
    }

    private int FindRandom(ButtonObj source, ButtonObj dest, State enemyState) {
        return Random.Range(0, 5);
    }
}
