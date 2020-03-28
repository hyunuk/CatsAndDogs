using System.Collections.Generic;

public static class Stages {
    public static readonly Dictionary<Pair, State> Stage_1 = new Dictionary<Pair, State> {
        { new Pair(0, 0), State.cat },
        { new Pair(1, 0), State.cat },
        { new Pair(5, 0), State.dog },
        { new Pair(6, 0), State.dog },
        { new Pair(0, 6), State.dog },
        { new Pair(1, 6), State.dog },
        { new Pair(5, 6), State.cat },
        { new Pair(6, 6), State.cat }
    };
}
