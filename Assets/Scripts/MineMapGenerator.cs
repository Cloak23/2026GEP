using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MineMapGenerator : MonoBehaviour
{
    [Header("Map")]
    public int width = 16; // 맵 가로 크기
    public int height = 16; // 맵 세로 크기
    public int floorIndex = 0; // 맵 층 수
    public int margin = 2;
    public int maxGenerationAttempts = 100;

    [Header("Floor Link")]
    public bool usePreviousGoal = false;
    public Vector2Int previousGoal;
    public int startJitterRange = 2; // start 위치 살짝 흔듦

    [Header("Start / Goal")]
    public int minStartGoalDistance = 14;
    public int goalDirectionAttempts = 50;
    public int goalJitterRange = 3;

    [Header("Waypoints")]
    public int minWaypointCount = 1;
    public int maxWaypointCount = 3;
    public int waypointJitterRange = 3;

    [Header("Mines")]
    [Range(0f, 1f)] public float startMineChance = 0.08f;
    [Range(0f, 1f)] public float goalMineChance = 0.28f;
    [Range(0f, 0.25f)] public float mineChanceNoise = 0.04f;
    [Range(0f, 0.05f)] public float mineChancePerFloor = 0.005f;

    [Header("Rewards")]
    public bool allowRewardsOnSafePath = false;
    [Range(0f, 1f)] public float startCoinChance = 0.05f;
    [Range(0f, 1f)] public float goalCoinChance = 0.18f;
    public int minCoinAmount = 1;
    public int maxCoinAmount = 5;
    [Range(0f, 1f)] public float startItemChance = 0.01f;
    [Range(0f, 1f)] public float goalItemChance = 0.05f;
    public string[] itemIds = { "Potion", "Scanner", "Shield" };

    [Header("Debug")]
    public bool generateOnStart = false;
    public bool logDebugMapOnStart = true;

    public MineMapData LastGeneratedMap { get; private set; }

    private IMineCountCalculator mineCountCalculator;
    private PathValidator pathValidator;

    private static readonly Vector2Int[] EightDirections =
    {
        new Vector2Int(-1, -1),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 1),
        new Vector2Int(0, 1),
        new Vector2Int(1, 1)
    };

    private void Awake()
    {
        EnsureServices();
    }

    private void Start()
    {
        if (!generateOnStart)
        {
            return;
        }

        LastGeneratedMap = GenerateMap();

        if (logDebugMapOnStart)
        {
            LastGeneratedMap.LogDebugMap();
        }
    }

    public MineMapData GenerateMap()
    {
        Vector2Int? linkedPreviousGoal = usePreviousGoal ? previousGoal : null;
        return GenerateMap(floorIndex, linkedPreviousGoal);
    }

    public MineMapData GenerateMap(int targetFloorIndex, Vector2Int? linkedPreviousGoal = null)
    {
        EnsureServices();
        ValidateSettings();

        for (int attempt = 1; attempt <= maxGenerationAttempts; attempt++)
        {
            MineMapData mapData = CreateEmptyMap(targetFloorIndex);

            mapData.start = CreateStartPosition(targetFloorIndex, linkedPreviousGoal);
            mapData.goal = CreateGoalPosition(mapData.start);
            mapData.waypoints = CreateWaypoints(mapData.start, mapData.goal);
            mapData.safePath = CreateSafePath(mapData.start, mapData.waypoints, mapData.goal);

            MarkStartGoalAndProtectedPath(mapData);
            PlaceMines(mapData);
            PlaceRewards(mapData);
            mineCountCalculator.Calculate(mapData);

            if (pathValidator.HasValidPath(mapData))
            {
                LastGeneratedMap = mapData;
                return mapData;
            }
        }

        throw new InvalidOperationException($"Failed to generate a valid mine map after {maxGenerationAttempts} attempts.");
    }

    [ContextMenu("Generate Map And Log Debug")]
    public void GenerateMapAndLogDebug()
    {
        MineMapData mapData = GenerateMap();
        mapData.LogDebugMap();
    }

    public void SetMineCountCalculator(IMineCountCalculator calculator)
    {
        mineCountCalculator = calculator ?? new MineCountCalculator();
    }

    private void EnsureServices()
    {
        mineCountCalculator ??= new MineCountCalculator();
        pathValidator ??= new PathValidator();
    }

    private void ValidateSettings()
    {
        width = Mathf.Max(4, width);
        height = Mathf.Max(4, height);
        margin = Mathf.Clamp(margin, 0, Mathf.Min(width, height) / 2 - 1);
        maxGenerationAttempts = Mathf.Max(1, maxGenerationAttempts);
        minStartGoalDistance = Mathf.Max(1, minStartGoalDistance);
        goalDirectionAttempts = Mathf.Max(1, goalDirectionAttempts);
        minWaypointCount = Mathf.Max(0, minWaypointCount);
        maxWaypointCount = Mathf.Max(minWaypointCount, maxWaypointCount);
        minCoinAmount = Mathf.Max(0, minCoinAmount);
        maxCoinAmount = Mathf.Max(minCoinAmount, maxCoinAmount);
    }

    private MineMapData CreateEmptyMap(int targetFloorIndex)
    {
        MineMapData mapData = new MineMapData(width, height, targetFloorIndex);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                mapData.tiles[x, y] = new TileData(new Vector2Int(x, y));
            }
        }

        return mapData;
    }

    private Vector2Int CreateStartPosition(int targetFloorIndex, Vector2Int? linkedPreviousGoal)
    {
        if (targetFloorIndex > 0 && linkedPreviousGoal.HasValue)
        {
            Vector2Int jitter = new Vector2Int(
                Random.Range(-startJitterRange, startJitterRange + 1),
                Random.Range(-startJitterRange, startJitterRange + 1));

            return ClampToPlayableArea(linkedPreviousGoal.Value + jitter);
        }

        int minX = PlayableMinX;
        int maxX = Mathf.Min(PlayableMaxX, Mathf.Max(PlayableMinX, Mathf.FloorToInt(width * 0.33f)));
        int minY = Mathf.Max(PlayableMinY, Mathf.CeilToInt(height * 0.66f));
        int maxY = PlayableMaxY;

        if (minY > maxY)
        {
            minY = PlayableMinY;
        }

        return RandomPoint(minX, maxX, minY, maxY);
    }

    private Vector2Int CreateGoalPosition(Vector2Int start)
    {
        if (TryCreateDirectionalGoal(start, out Vector2Int goal))
        {
            return goal;
        }

        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int x = PlayableMinX; x <= PlayableMaxX; x++)
        {
            for (int y = PlayableMinY; y <= PlayableMaxY; y++)
            {
                Vector2Int position = new Vector2Int(x, y);

                if (ManhattanDistance(start, position) >= EffectiveMinStartGoalDistance)
                {
                    candidates.Add(position);
                }
            }
        }

        if (candidates.Count > 0)
        {
            return candidates[Random.Range(0, candidates.Count)];
        }

        return FarthestPlayablePointFrom(start);
    }

    private bool TryCreateDirectionalGoal(Vector2Int start, out Vector2Int goal)
    {
        int minDistance = EffectiveMinStartGoalDistance;
        int maxStep = Mathf.Max(width, height);

        for (int i = 0; i < goalDirectionAttempts; i++)
        {
            Vector2Int direction = EightDirections[Random.Range(0, EightDirections.Length)];
            int step = Random.Range(Mathf.Max(1, minDistance / 2), maxStep + 1);
            Vector2Int jitter = new Vector2Int(
                Random.Range(-goalJitterRange, goalJitterRange + 1),
                Random.Range(-goalJitterRange, goalJitterRange + 1));

            Vector2Int candidate = ClampToPlayableArea(start + direction * step + jitter);

            if (candidate != start && ManhattanDistance(start, candidate) >= minDistance)
            {
                goal = candidate;
                return true;
            }
        }

        goal = default;
        return false;
    }

    private List<Vector2Int> CreateWaypoints(Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> waypoints = new List<Vector2Int>();
        int waypointCount = Random.Range(minWaypointCount, maxWaypointCount + 1);

        for (int i = 1; i <= waypointCount; i++)
        {
            float t = i / (float)(waypointCount + 1);
            int x = Mathf.RoundToInt(Mathf.Lerp(start.x, goal.x, t));
            int y = Mathf.RoundToInt(Mathf.Lerp(start.y, goal.y, t));

            Vector2Int jitter = new Vector2Int(
                Random.Range(-waypointJitterRange, waypointJitterRange + 1),
                Random.Range(-waypointJitterRange, waypointJitterRange + 1));

            Vector2Int waypoint = ClampToPlayableArea(new Vector2Int(x, y) + jitter);

            if (waypoint != start && waypoint != goal && !waypoints.Contains(waypoint))
            {
                waypoints.Add(waypoint);
            }
        }

        return waypoints;
    }

    private List<Vector2Int> CreateSafePath(Vector2Int start, List<Vector2Int> waypoints, Vector2Int goal)
    {
        List<Vector2Int> safePath = new List<Vector2Int>();
        HashSet<Vector2Int> added = new HashSet<Vector2Int>();

        Vector2Int current = start;
        AddPathPosition(safePath, added, current);

        for (int i = 0; i < waypoints.Count; i++)
        {
            AddSegmentPath(safePath, added, current, waypoints[i]);
            current = waypoints[i];
        }

        AddSegmentPath(safePath, added, current, goal);
        return safePath;
    }

    private void AddSegmentPath(List<Vector2Int> safePath, HashSet<Vector2Int> added, Vector2Int from, Vector2Int to)
    {
        Vector2Int current = from;

        while (current != to)
        {
            bool canMoveX = current.x != to.x;
            bool canMoveY = current.y != to.y;
            bool moveX = canMoveX && (!canMoveY || Random.value < 0.5f);

            if (moveX)
            {
                current.x += Math.Sign(to.x - current.x);
            }
            else if (canMoveY)
            {
                current.y += Math.Sign(to.y - current.y);
            }

            AddPathPosition(safePath, added, current);
        }
    }

    private void AddPathPosition(List<Vector2Int> safePath, HashSet<Vector2Int> added, Vector2Int position)
    {
        if (added.Add(position))
        {
            safePath.Add(position);
        }
    }

    private void MarkStartGoalAndProtectedPath(MineMapData mapData)
    {
        for (int i = 0; i < mapData.safePath.Count; i++)
        {
            Vector2Int position = mapData.safePath[i];
            TileData tile = mapData.tiles[position.x, position.y];
            tile.isProtected = true;
            tile.isMine = false;
        }

        TileData startTile = mapData.GetTile(mapData.start);
        startTile.isStart = true;
        startTile.isProtected = true;

        TileData goalTile = mapData.GetTile(mapData.goal);
        goalTile.isGoal = true;
        goalTile.isProtected = true;
    }

    private void PlaceMines(MineMapData mapData)
    {
        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                TileData tile = mapData.tiles[x, y];
                tile.risk = CalculateProgressRisk(tile.position, mapData.start, mapData.goal);

                if (tile.isProtected)
                {
                    continue;
                }

                float mineChance = Mathf.Lerp(startMineChance, goalMineChance, tile.risk);
                mineChance += Random.Range(-mineChanceNoise, mineChanceNoise);
                mineChance += Mathf.Max(0, mapData.floorIndex) * mineChancePerFloor;
                mineChance = Mathf.Clamp01(mineChance);

                tile.isMine = Random.value < mineChance;
                if (tile.isMine)
                {
                    tile.tileCoin = Random.Range(6 * minCoinAmount, 6 * maxCoinAmount + 1);
                }
            }
        }
    }

    private void PlaceRewards(MineMapData mapData)
    {
        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                TileData tile = mapData.tiles[x, y];

                if (tile.isMine || (!allowRewardsOnSafePath && tile.isProtected))
                {
                    continue;
                }

                float coinChance = Mathf.Lerp(startCoinChance, goalCoinChance, tile.risk);

                if (Random.value < coinChance)
                {
                    tile.tileCoin = Random.Range(minCoinAmount, maxCoinAmount + 1);
                }

                float itemChance = Mathf.Lerp(startItemChance, goalItemChance, tile.risk);

                if (Random.value < itemChance)
                {
                    tile.hasItem = true;
                    //tile.itemId = itemIds != null && itemIds.Length > 0
                    //    ? itemIds[Random.Range(0, itemIds.Length)]
                    //    : "DefaultItem";
                }
            }
        }
    }

    private float CalculateProgressRisk(Vector2Int position, Vector2Int start, Vector2Int goal)
    {
        Vector2 startPoint = new Vector2(start.x, start.y);
        Vector2 goalPoint = new Vector2(goal.x, goal.y);
        Vector2 positionPoint = new Vector2(position.x, position.y);
        Vector2 startToGoal = goalPoint - startPoint;
        float lengthSquared = Vector2.Dot(startToGoal, startToGoal);

        if (lengthSquared <= Mathf.Epsilon)
        {
            return 0f;
        }

        float progress = Vector2.Dot(positionPoint - startPoint, startToGoal) / lengthSquared;
        return Mathf.Clamp01(progress);
    }

    private Vector2Int ClampToPlayableArea(Vector2Int position)
    {
        return new Vector2Int(
            Mathf.Clamp(position.x, PlayableMinX, PlayableMaxX),
            Mathf.Clamp(position.y, PlayableMinY, PlayableMaxY));
    }

    private Vector2Int RandomPoint(int minX, int maxX, int minY, int maxY)
    {
        minX = Mathf.Clamp(minX, PlayableMinX, PlayableMaxX);
        maxX = Mathf.Clamp(maxX, minX, PlayableMaxX);
        minY = Mathf.Clamp(minY, PlayableMinY, PlayableMaxY);
        maxY = Mathf.Clamp(maxY, minY, PlayableMaxY);

        return new Vector2Int(Random.Range(minX, maxX + 1), Random.Range(minY, maxY + 1));
    }

    private Vector2Int FarthestPlayablePointFrom(Vector2Int start)
    {
        Vector2Int farthest = start;
        int farthestDistance = -1;

        for (int x = PlayableMinX; x <= PlayableMaxX; x++)
        {
            for (int y = PlayableMinY; y <= PlayableMaxY; y++)
            {
                Vector2Int candidate = new Vector2Int(x, y);
                int distance = ManhattanDistance(start, candidate);

                if (distance > farthestDistance)
                {
                    farthest = candidate;
                    farthestDistance = distance;
                }
            }
        }

        return farthest;
    }

    private int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private int EffectiveMinStartGoalDistance
    {
        get
        {
            int maxPossibleDistance = Mathf.Max(1, (PlayableMaxX - PlayableMinX) + (PlayableMaxY - PlayableMinY));
            return Mathf.Min(minStartGoalDistance, maxPossibleDistance);
        }
    }



    private int PlayableMinX => Mathf.Clamp(margin, 0, width - 1);
    private int PlayableMaxX => Mathf.Clamp(width - 1 - margin, PlayableMinX, width - 1);
    private int PlayableMinY => Mathf.Clamp(margin, 0, height - 1);
    private int PlayableMaxY => Mathf.Clamp(height - 1 - margin, PlayableMinY, height - 1);
}
