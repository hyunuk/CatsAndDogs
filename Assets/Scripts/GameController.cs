using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    private const int NEARBY = 2;
    public const int LINE_COUNT = 7;

    public ButtonObj[] buttonList;
    public int[][] board;
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
        InitInfo();
        UpdateBoard();
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
        board = new int[LINE_COUNT][];
        Dictionary<Pair, State> stage = Stages.Stage_1;
        for (int y = 0; y < board.Length; y++) {
            board[y] = new int[LINE_COUNT];
            for (int x = 0; x < board[y].Length; x++) {
                Pair curr = new Pair(x, y);
                if (stage.ContainsKey(curr)) {
                    board[y][x] = (int) stage[curr];
                } else {
                    board[y][x] = 0;
                }
                
            }
        }
    }

    private void UpdateBoard() {
        for (int y = 0; y < board.Length; y++) {
            for (int x = 0; x < board.Length; x++) {
                int pos = GetPosition(x, y);
                State currState = (State) board[y][x];
                buttonList[pos].CurrState = currState;
                if (currState == State.cat) players[0].AddButton(buttonList[pos]);
                if (currState == State.dog) players[1].AddButton(buttonList[pos]);
            }
        }
    }

    private void InitInfo() {
        catName.text = catPlayer.IsAI ? "Computer (" + catPlayer.Difficulty + ")" : "Player";
        dogName.text = dogPlayer.IsAI ? "Computer (" + dogPlayer.Difficulty + ")" : "Player";
        catTurnInfo.enabled = true;
        dogTurnInfo.enabled = false;

        catScore.text = catPlayer.GetButtonObjs().Count.ToString();
        dogScore.text = dogPlayer.GetButtonObjs().Count.ToString();
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
        Player currPlayer = players[currPlayerIndex];
        List<ButtonObj> buttons = currPlayer.GetButtonObjs();
        (ButtonObj currButton, ButtonObj nextButton) = GetNextMove(buttons, GetFind(level));
        StartCoroutine(RunCommon(currButton, nextButton));
    }

    private IEnumerator RunCommon(ButtonObj currButton, ButtonObj nextButton) {
        yield return new WaitForSeconds(0.5f);
        ClickEvent(currButton);
        yield return new WaitForSeconds(0.5f);
        ClickEvent(nextButton);
    }

    private (ButtonObj, ButtonObj) GetNextMove(List<ButtonObj> buttons, Find FindNet) {
        int max = Int32.MinValue;
        ButtonObj currButton = null;
        ButtonObj nextButton = null;
        foreach (ButtonObj button in buttons) {
            List<ButtonObj> neighbors = FindAvailableButtons(button);
            foreach (ButtonObj neighbor in neighbors) {
                int net = FindNet(button, neighbor, GetEnemyState(currPlayerIndex));
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

    private int FindRandom(ButtonObj currButton, ButtonObj selectedButton, State enemyState) {
        return Random.Range(0, 5);
    }

    private int FindMinimax(ButtonObj currButton, ButtonObj selectedButton, State enemyState) {
        return FindGain(currButton, selectedButton, enemyState) + FindLoss(currButton, enemyState);
    }

    private int FindGain(ButtonObj currButton, ButtonObj selectedButton, State enemyState) {
        int gain = 0;
        if (GetDistance(currButton, selectedButton) == 1) gain++;
        foreach (ButtonObj neighbor in FindNeighbors(selectedButton, 1)) if (neighbor.CurrState == enemyState) gain++;
        return gain;
    }

    private int FindLoss(ButtonObj currButton, State enemyState) {
        int loss = 0;
        foreach (ButtonObj neighbor in FindNeighbors(currButton, 2)) if (neighbor.CurrState == enemyState) loss--;
        return loss;
    }

    public void ClickEvent(ButtonObj clickedButton) {
        Player currPlayer = players[currPlayerIndex];
        switch (status) {
            case Status.notSelected:
                if (IsCurrPlayerButton(clickedButton, currPlayer)) {
                    this.selectedButton = clickedButton;
                    this.status = Status.clicked;
                    UpdateBorders();
                    PlayAudio(clickedButton);
                }
                break;

            case Status.clicked:
                if (IsCurrPlayerButton(clickedButton, currPlayer)) {
                    this.selectedButton = clickedButton;
                    
                    PlayAudio(clickedButton);
                    ClearBorders();
                    UpdateBorders();
                } else if (IsBorder(clickedButton.CurrState)) {
                    this.status = Status.notSelected;
                    Attack(clickedButton);
                } else ClearBorders();
                break;
            default:
                throw new ArgumentException(String.Format("{0} is not a valid game status!", status));

        }
        // UpdateBoard();
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

    private bool IsCurrPlayerButton(ButtonObj clickedButton, Player currPlayer) {
        return currPlayer.GetButtonObjs().Contains(clickedButton);
    }

    private void UpdateBorders() {
        List<ButtonObj> retList = FindAvailableButtons(selectedButton);
        foreach (ButtonObj b in retList) {
            int distance = GetDistance(b, selectedButton);
            if (distance == 1) b.CurrState = State.nearBorder;
            if (distance == 2) b.CurrState = State.farBorder;
        }
    }

    void GameOver() {
        string winner = (players[0].GetSize() > players[1].GetSize()) ? "Cat" : "Dog";
        foreach (ButtonObj button in buttonList) {
            if (winner.Equals("Cat") && button.CurrState == State.empty) {
                button.CurrState = State.cat;
            } else if (winner.Equals("Dog") && button.CurrState == State.empty) {
                button.CurrState = State.dog;
            }
            
        }
        GetWinner(winner);
        endGamePanel.SetActive(true);
    }

    private void GetWinner(string winner) {
        winnerText.GetComponent<Text>().text = winner + " Player Won!";
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
        return (x != X || y != Y) && WithinBoundary(x, y);
    }

    private bool IsAvailable(int x, int y, int X, int Y) {
        return IsNeighbor(x, y, X, Y) && buttonList[GetPosition(x, y)].CurrState == State.empty;
    }

    private List<ButtonObj> VisitNeighbors(ButtonObj button, int gap, Function f) {
        List<ButtonObj> retList = new List<ButtonObj>();
        Pair coord = button.coord;
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

    private int GetPosition(ButtonObj button) {
        return GetPosition(button.GetCoord());
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
        ClearBorders();

        int dist = GetDistance(selectedButton, clickedButton);
        if (dist <= 2) {
            if (dist == 2) RemoveCurrentButton();
            MoveButton(clickedButton);
            ConsumeButton(clickedButton);
            EndTurn();
        }
    }

    private void ConsumeButton(ButtonObj clickedButton) {
        List<ButtonObj> neighbors = FindNeighbors(clickedButton, 1);
        foreach (ButtonObj neighbor in neighbors) {
            if (IsEnemy(neighbor)) {
                players[currPlayerIndex].AddButton(neighbor);
                players[(currPlayerIndex == 0) ? 1 : 0].RemoveButton(neighbor);        
            }
        }
    }

    private void MoveButton(ButtonObj clickedButton) {
        players[currPlayerIndex].AddButton(clickedButton);
    }

    private void RemoveCurrentButton() {
        selectedButton.CurrState = State.empty;
        players[currPlayerIndex].RemoveButton(selectedButton);
    }

    private void ClearBorders() {
        foreach (ButtonObj button in buttonList) {
            if (IsBorder(button.CurrState)) button.CurrState = State.empty;
        }
    }

    private bool IsBorder(State state) {
        return state == State.nearBorder || state == State.farBorder;
    }

    private bool IsEnemy(ButtonObj button) {
        return IsEnemy(button.CurrState);
    }

    private bool IsEnemy(State state) {
        return (currPlayerIndex == 0 && state == State.dog) || (currPlayerIndex == 1 && state == State.cat);
    }

    private State GetEnemyState(int playerIndex) {
        return playerIndex == 0 ? State.dog : State.cat;
    }

    private State GetEnemyState(ButtonObj button) {
        return GetEnemyState(button.CurrState);
    }

    private State GetEnemyState(State state) {
        return state == State.cat ? State.dog : State.cat;
    }

    private void EndTurn() {
        currPlayerIndex = (currPlayerIndex == 0) ? 1 : 0;
        catScore.text = catPlayer.GetButtonObjs().Count.ToString();
        dogScore.text = dogPlayer.GetButtonObjs().Count.ToString();

        if (CanContinue()) {
            ClearBorders();
            turnCount++;
            catTurnInfo.enabled = currPlayerIndex == 0;
            dogTurnInfo.enabled = currPlayerIndex == 1;
            StartTurn();
        } else GameOver();
    }

    private bool CanContinue() {
        int catCount = catPlayer.GetButtonObjs().Count;
        int dogCount = dogPlayer.GetButtonObjs().Count;
        return (catCount != 0 && dogCount != 0) && (catCount + dogCount < LINE_COUNT * LINE_COUNT) && IsNoMoreMove();
    }

    private bool IsNoMoreMove() {
        List<ButtonObj> buttons = players[currPlayerIndex].GetButtonObjs();
        (ButtonObj currButton, ButtonObj nextButton) = GetNextMove(buttons, FindMinimax);
        if (!nextButton) return false;
        return true;
    }
}
