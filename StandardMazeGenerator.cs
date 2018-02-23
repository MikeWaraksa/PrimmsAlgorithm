using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MazeCell {
    public bool RightWall;
    public bool DownWall;
}

public enum CardinalDirection {
    Up,
    Right,
    Down,
    Left
}

public class PathingNode {
    public Vector2 node;
    public CardinalDirection direction;

    public PathingNode(Vector2 v, CardinalDirection d) {
        node = v;
        direction = d;
    }

    public Vector2 LeadsTo() {
        Vector2 res = node;
        switch (direction) {
            case CardinalDirection.Up:
                res -= Vector2.up;
                break;
            case CardinalDirection.Down:
                res -= Vector2.down;
                break;
            case CardinalDirection.Left:
                res += Vector2.left;
                break;
            case CardinalDirection.Right:
                res += Vector2.right;
                break;
            default:
                break;
        }
        return res;
    }

    public static bool operator ==(PathingNode left, PathingNode right) {
        if (left.node != right.node) return false;
        if (left.direction != right.direction) return false;
        return true;
    }

    public static bool operator !=(PathingNode left, PathingNode right) {
        return !(left == right);
    }
    
}

[System.Serializable]
public class StandardMazeGenerator {

    [Header("Data")]
    public MazeCell[,] m_cell;
    public Vector2 Entrance;
    public Vector2 Exit;

    public int MazeWidth;
    public int MazeLength;
    public int SeedValue;

    public List<PathingNode> LeftHandPath;
    public List<PathingNode> RightHandPath;
    public List<PathingNode> DirectPath;

    public IEnumerator GenerateDirectPath() {

        Vector2 current = Exit;

        int[,] grid = new int[MazeWidth, MazeLength];

        PriorityQueue<Vector2> queue = new PriorityQueue<Vector2>();

        queue.Add(current, 0f);
        grid[(int)current.x, (int)current.y] = 1;

        float LoopTime = Time.realtimeSinceStartup;

        bool solved = false;

        while (queue.Count > 0) {
            current = queue.Get(0);
            queue.RemoveAt(0);


            if (current == Entrance) {
                solved = true;
                break;
            }

            for (int n = 0; n < 4; n++) {
                // doesn't exit this direction, discard.
                if (!HasExit(current, (CardinalDirection)n)) continue;

                PathingNode d = new PathingNode(current, (CardinalDirection)n);
                Vector2 next = d.LeadsTo();

                // the other node has already been searched, and is a lower value. Discard.
                if (grid[(int)next.x, (int)next.y] != 0 && grid[(int)next.x, (int)next.y] < grid[(int)current.x, (int)current.y] + 1) continue;

                // set arrival cost in grid
                grid[(int)next.x, (int)next.y] = grid[(int)current.x, (int)current.y] + 1;

                // add this node to the queue
                float heuristic = Mathf.Abs(next.x - Entrance.x) + Mathf.Abs(next.y - Entrance.y);
                queue.Add(next, grid[(int)next.x, (int)next.y] + heuristic);
            }

            // grid[(int)current.x, (int)current.y];


            if (Time.realtimeSinceStartup - LoopTime > 0.1) {
                Debug.Log("searching...");
                yield return null;
                LoopTime = Time.realtimeSinceStartup;
            }

        }

        if (!solved) yield break;

        // backtrace...
        current = Entrance;

        List<PathingNode> path = new List<PathingNode>();
        while (current != Exit) {

            for (int n = 0; n < 4; n++) {
                CardinalDirection c = (CardinalDirection)n;
                if (!HasExit(current, c)) continue;

                PathingNode d = new PathingNode(current, c);
                Vector2 next = d.LeadsTo();

                if (grid[(int)next.x, (int)next.y] >= grid[(int)current.x, (int)current.y]) continue;

                path.Insert(0, d); // path from exit to entrance...
                current = next;
                break;
                // check if any of the four directions has a node with lower value than the current.
            }

            if (Time.realtimeSinceStartup - LoopTime > 0.1) {
                yield return null;
                Debug.Log("backtrace...");
                LoopTime = Time.realtimeSinceStartup;
            }

        }

        DirectPath = path;

        yield break;
    }

    // generate a path, always taking the left turn, when possible.
    public IEnumerator GenerateLeftHandPath() {

        List<PathingNode> path = new List<PathingNode>();

        Vector2 current = Entrance;
        CardinalDirection direction = CardinalDirection.Left;

        float LoopTime = Time.realtimeSinceStartup;

        while (current != Exit) {

            // check each exit in turn...
            for (int n = 0; n < 4; n++) {
                CardinalDirection newDirection = (CardinalDirection)(((int)direction + 4 + 1 - n) % 4);
                if (HasExit(current, newDirection)) {
                    direction = newDirection;
                    PathingNode directionNode = new PathingNode(current, direction);
                    path.Add(directionNode);

                    current = directionNode.LeadsTo();

                    break;
                }
            }


            if (Time.realtimeSinceStartup - LoopTime > 0.1) {
                yield return null;
                LoopTime = Time.realtimeSinceStartup;
            }

            if (current.x < 0 || current.y < 0) break;

            bool PathLoops = false;
            for (int n = 0; n < path.Count - 1; n++) {
                if (path[n] == path[path.Count - 1]) {
                    PathLoops = true;
                    break;
                }

                if (Time.realtimeSinceStartup - LoopTime > 0.1) {
                    Debug.Log("yield: " + path.Count + ": " + current);
                    yield return null;
                    LoopTime = Time.realtimeSinceStartup;
                }
            }

            if (PathLoops) break;

        }

        LeftHandPath = path;

        yield break;
    }

    public IEnumerator GenerateRightHandPath() {

        List<PathingNode> path = new List<PathingNode>();

        Vector2 current = Entrance;
        CardinalDirection direction = CardinalDirection.Right;

        float LoopTime = Time.realtimeSinceStartup;

        while (current != Exit) {

            // check each exit in turn...
            for (int n = 0; n < 4; n++) {
                CardinalDirection newDirection = (CardinalDirection)(((int)direction + 4 + 3 + n) % 4);
                if (HasExit(current, newDirection)) {
                    direction = newDirection;
                    PathingNode directionNode = new PathingNode(current, direction);
                    path.Add(directionNode);

                    current = directionNode.LeadsTo();

                    break;
                }
            }


            if (Time.realtimeSinceStartup - LoopTime > 0.1) {
                yield return null;
                LoopTime = Time.realtimeSinceStartup;
            }

            if (current.x < 0 || current.y < 0) break;

            bool PathLoops = false;
            for (int n = 0; n < path.Count - 1; n++) {
                if (path[n] == path[path.Count - 1]) {
                    PathLoops = true;
                    break;
                }

                if (Time.realtimeSinceStartup - LoopTime > 0.1) {
                    // Debug.Log("yield: " + path.Count + ": " + current);
                    yield return null;
                    LoopTime = Time.realtimeSinceStartup;
                }
            }

            if (PathLoops) break;

        }

        RightHandPath = path;

        yield break;
    }

    public void PrepareGridData(int ArgMazeWidth, int ArgMazeLength, float WallBias = 50) {
        MazeWidth = ArgMazeWidth;
        MazeLength = ArgMazeLength;


        int RandomSeed = SeedValue;
        if (RandomSeed == 0) {
            RandomSeed = Mathf.FloorToInt(Random.Range(0, int.MaxValue));
        }

        Random.InitState(RandomSeed);

        // prepare a blank cell data for population
        m_cell = new MazeCell[MazeWidth, MazeLength];

        // grouping data
        int[,] Grid = new int[MazeWidth, MazeLength];

        // for each row
        for (int y = 0; y < MazeLength; y++) {

            for (int x = 0; x < MazeWidth; x++) {
                // if the square doesn't belong to a set yet, add it to a unique set.
                if (Grid[x, y] == 0) Grid[x, y] = (y * MazeWidth) + x + 1;
            }

            // if this isn't the last row
            if (y < MazeLength - 1) {
                for (int x = 0; x < MazeWidth - 1; x++) {
                    if (Grid[x, y] == Grid[x + 1, y]) {
                        // create a right wall
                        m_cell[x, y].RightWall = true;
                    } else if (Random.Range(0, 100) > WallBias) {
                        // create a right wall
                        m_cell[x, y].RightWall = true;

                    } else if (x < MazeWidth - 1) {
                        // if this isn't the last square
                        // join the next unit to this set
                        MergeSets(Grid, Grid[x, y], Grid[x + 1, y]);
                    }
                }
            } else {

                // if this is the last row...
                for (int x = 0; x < MazeWidth - 1; x++) {

                    if (Grid[x, y] == Grid[x + 1, y]) {
                        // create a right wall
                        m_cell[x, y].RightWall = true;

                    } else if (Random.Range(0, 100) > WallBias) {
                        // create a right wall
                        m_cell[x, y].RightWall = true;

                    } else if (x < MazeWidth - 2) {
                        // join the next unit to this set
                        MergeSets(Grid, Grid[x, y], Grid[x + 1, y]);
                    }
                }

                for (int x = 0; x < MazeWidth - 1; x++) {
                    if (Grid[x, y] != Grid[x + 1, y]) {
                        m_cell[x, y].RightWall = false;
                        // Destroy(CreatedWalls[x]);
                    }
                }

            }

            Dictionary<int, int> GroupWallsCount = new Dictionary<int, int>();
            // bottom wall decisions
            if (y != MazeLength)
                for (int x = 0; x < MazeWidth; x++) {
                    bool CreatedBottomWall = false;
                    // we need to track the number of cells in the group with walls created
                    if (Alone(Grid, Grid[x, y])) {
                        // do not create a wall
                    } else if ((GroupWallsCount.ContainsKey(Grid[x, y])) ? (GroupWallsCount[Grid[x, y]] >= CountSet(Grid, Grid[x, y], y) - 1) : (false)) {
                        // do not create a wall
                    } else if (Random.Range(0, 100) < WallBias) {
                        // create a bottom wall
                        if (!GroupWallsCount.ContainsKey(Grid[x, y])) {
                            GroupWallsCount.Add(Grid[x, y], 1);
                        } else {
                            GroupWallsCount[Grid[x, y]]++;
                        }

                        m_cell[x, y].DownWall = true;
                        CreatedBottomWall = true;
                    }

                    if (y < MazeLength - 1 && !CreatedBottomWall) {
                        // join the below unit to this set
                        AddToSet(Grid, x, y + 1, Grid[x, y]);
                    }
                }

            for (int x = 0; x < MazeWidth; x++) {
                NullSet(Grid, x, y);
            }

        }

        // if this is the bottom of the maze, all bottoms are set true.
        for (int x = 0; x < MazeWidth; x++) {
            m_cell[x, MazeLength - 1].DownWall = true;
        }

        // if this is the right edge, all rights are set true.
        for (int y = 0; y < MazeLength; y++) {
            m_cell[MazeWidth - 1, y].RightWall = true;
        }

    }

    public void MostDistantPoints() {
        if (m_cell == null) return;

        int LongestPath = 0;
        Vector2 start = -1f * Vector2.one, origin = -1f * Vector2.one, destination = -1f * Vector2.one;

        for (int x = 0; x < MazeWidth; x++) {
            for (int y = 0; y < MazeLength; y++) {
                // flood algorithm
                int CurLongest = 1;
                int[,] Grid = new int[MazeWidth, MazeLength];
                for (int m = 0; m < MazeWidth; m++) {
                    for (int n = 0; n < MazeLength; n++) {
                        Grid[m, n] = 0;
                    }
                }
                origin = new Vector2(x, y);
                Vector2 LocalFurtherest = origin;

                List<Vector2> search = new List<Vector2>();
                search.Add(origin);
                Grid[(int)origin.x, (int)origin.y] = 1;

                while (search.Count > 0) {
                    Vector2 Current = search[0];
                    search.RemoveAt(0);

                    if (Current.y < 0 || Current.x < 0 || Current.y >= MazeLength || Current.x >= MazeWidth) {
                        continue;
                    }

                    if (Grid[(int)Current.x, (int)Current.y] > CurLongest) {
                        CurLongest = Grid[(int)Current.x, (int)Current.y];
                        LocalFurtherest = Current;
                    }

                    // we search the four directions.
                    // if it is zero, we assign it current + 1 and add it to the queue.
                    // if there is a value there, and it is less than our current value + 1,
                    // we skip it.


                    if (Current.y < MazeLength - 1 && !m_cell[(int)Current.x, (int)Current.y].DownWall &&
                        (Grid[(int)Current.x, (int)Current.y + 1] == 0 || Grid[(int)Current.x, (int)Current.y] + 1 < Grid[(int)Current.x, (int)Current.y + 1])) {
                        Grid[(int)Current.x, (int)Current.y + 1] = Grid[(int)Current.x, (int)Current.y] + 1;
                        search.Add(new Vector2((int)Current.x, (int)Current.y + 1));
                    }


                    if (Current.x < MazeWidth - 1 && !m_cell[(int)Current.x, (int)Current.y].RightWall &&
                        (Grid[(int)Current.x + 1, (int)Current.y] == 0 || Grid[(int)Current.x, (int)Current.y] + 1 < Grid[(int)Current.x + 1, (int)Current.y])) {
                        Grid[(int)Current.x + 1, (int)Current.y] = Grid[(int)Current.x, (int)Current.y] + 1;
                        search.Add(new Vector2((int)Current.x + 1, (int)Current.y));
                    }

                    if (Current.x > 0 && !m_cell[(int)Current.x - 1, (int)Current.y].RightWall &&
                        (Grid[(int)Current.x - 1, (int)Current.y] == 0 || Grid[(int)Current.x, (int)Current.y] + 1 < Grid[(int)Current.x - 1, (int)Current.y])) {
                        Grid[(int)Current.x - 1, (int)Current.y] = Grid[(int)Current.x, (int)Current.y] + 1;
                        search.Add(new Vector2((int)Current.x - 1, (int)Current.y));
                    }

                    if (Current.y > 0 && !m_cell[(int)Current.x, (int)Current.y - 1].DownWall &&
                        (Grid[(int)Current.x, (int)Current.y - 1] == 0 || Grid[(int)Current.x, (int)Current.y] + 1 < Grid[(int)Current.x, (int)Current.y - 1])) {
                        Grid[(int)Current.x, (int)Current.y - 1] = Grid[(int)Current.x, (int)Current.y] + 1;
                        search.Add(new Vector2((int)Current.x, (int)Current.y - 1));
                    }
                }

                if (CurLongest > LongestPath) {
                    LongestPath = CurLongest;
                    start = origin;
                    destination = LocalFurtherest;
                }


            }
        }
        Entrance = start;
        Exit = destination;


        // Debug.Log("Entrance: " + Entrance + " | Exit: " + Exit + " | Length: " + LongestPath);
    }


    // add grid[x,y] to Sets[s]
    void AddToSet(int[,] Grid, int x, int y, int s) {
        // if we aren't a set member, go remove ourselves
        Grid[x, y] = s;
    }

    void NullSet(int[,] Grid, int x, int y) {
        Grid[x, y] = 0;
    }

    // MergeSet one and set two
    void MergeSets(int[,] Grid, int Set1, int Set2) {
        if (Set1 == 0 || Set2 == 0) return;

        for (int x = 0; x < MazeWidth; x++)
            for (int y = 0; y < MazeLength; y++) {
                if (Grid[x, y] == Set2) Grid[x, y] = Set1;
            }

    }

    int CountSet(int[,] Grid, int Set, int row) {
        if (Set == 0) Debug.Log("Counting 0...");
        int n = 0;

        for (int x = 0; x < MazeWidth; x++) {
            if (Grid[x, row] == Set) n++;
        }

        if (n == 0) Debug.Log("Counted an empty set...");

        return n;
    }

    bool Alone(int[,] Grid, int Set) {
        int n = 0;

        for (int x = 0; x < MazeWidth; x++) {
            for (int y = 0; y < MazeLength; y++) {
                if (Grid[x, y] == Set) {
                    n++;
                    if (n == 2) return false;
                }
            }
        }

        return true;
    }

    bool HasExit(Vector2 node, CardinalDirection d) {
        int x = (int)node.x; int y = (int)node.y;

        return HasExit(x, y, d);
    }

    bool HasExit(int x, int y, CardinalDirection d) {
        if (x < 0 || y < 0 || x >= MazeWidth || y >= MazeLength) return false;

        switch (d) {
            case CardinalDirection.Down:
                return !m_cell[x, y].DownWall;
            case CardinalDirection.Right:
                return !m_cell[x, y].RightWall;
            case CardinalDirection.Up:
                return HasExit(x, y - 1, CardinalDirection.Down);
            case CardinalDirection.Left:
                return HasExit(x - 1, y, CardinalDirection.Right);
        }

        return true;
    }

}
