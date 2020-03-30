using System;

public class Util {
    public static bool WithinBoundary(Pair coord) {
        return WithinBoundary(coord.X, coord.Y);
    }

    public static bool WithinBoundary(int x, int y) {
        return 0 <= x && x < GameController.LINE_COUNT && 0 <= y && y < GameController.LINE_COUNT;
    }

    public static int GetPosition(Pair coord) {
        return GetPosition(coord.X, coord.Y);
    }

    public static int GetPosition(ButtonObj button) {
        return GetPosition(button.Coord);
    }

    public static int GetPosition(int x, int y) {
        return (y * GameController.LINE_COUNT) + x;
    }

    public static int GetDistance(Pair from, Pair to) {
        int xDist = from.X - to.X;
        int yDist = from.Y - to.Y;
        return (int)Math.Sqrt(xDist * xDist + yDist * yDist);
    }

    public static int GetDistance(ButtonObj from, ButtonObj to) {
        return GetDistance(from.Coord, to.Coord);
    }

    public static bool IsBorder(State state) {
        return state == State.nearBorder || state == State.farBorder;
    }

    public static bool IsCurrPlayerButton(ButtonObj button, int currPlayerIndex) {
        return button.CurrState == GetPlayerState(currPlayerIndex);
    }

    public static bool IsEnemy(ButtonObj button, int currPlayerIndex) {
        return IsEnemy(button.CurrState, currPlayerIndex);
    }

    public static bool IsEnemy(State state, int currPlayerIndex) {
        return (currPlayerIndex == 0 && state == State.dog) || (currPlayerIndex == 1 && state == State.cat);
    }

    public static State GetPlayerState(int playerIndex) {
        return playerIndex == 0 ? State.cat : State.dog;
    }

    public static State GetEnemyState(int playerIndex) {
        return playerIndex == 0 ? State.dog : State.cat;
    }
}
