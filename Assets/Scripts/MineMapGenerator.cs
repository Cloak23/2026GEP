using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MineMapGenerator : MonoBehaviour
{
    [Header("Map")]
    public int width = 16;
    public int height = 16;
    public int floorIndex = 0;
    public int margin = 2;
    public int maxGenerationAttempts = 100;

    [Header("Floor Link")]
    public bool usePreviousGoal = false;
    public Vector2Int previousGoal;
    public int startJitterRange = 2;

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

    public int revivalLevel;
    public int openTileLevel;
    public int AIrequestLevel;
    public MineMapData LastGeneratedMap { get; private set; }

    private IMineCountCalculator mineCountCalculator;
    private PathValidator pathValidator;

    private static readonly Vector2Int[] EightDirections =
    {
        new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
        new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(-1, 1),
        new Vector2Int(0, 1), new Vector2Int(1, 1)
    };

    public void ClearLastGeneratedMap()
    {
        LastGeneratedMap = null;
    }

    private void Awake()
    {
        EnsureServices();
    }

    private void Start()
    {
        if (!generateOnStart) return;
        LastGeneratedMap = GenerateMap();
        if (logDebugMapOnStart) LastGeneratedMap.LogDebugMap();
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
            Vector2Int jitter = new Vector2Int(Random.Range(-startJitterRange, startJitterRange + 1), Random.Range(-startJitterRange, startJitterRange + 1));
            return ClampToPlayableArea(linkedPreviousGoal.Value + jitter);
        }
        int minX = PlayableMinX, maxX = Mathf.Min(PlayableMaxX, Mathf.Max(PlayableMinX, Mathf.FloorToInt(width * 0.33f)));
        int minY = Mathf.Max(PlayableMinY, Mathf.CeilToInt(height * 0.66f)), maxY = PlayableMaxY;
        if (minY > maxY) minY = PlayableMinY;
        return RandomPoint(minX, maxX, minY, maxY);
    }

    private Vector2Int CreateGoalPosition(Vector2Int start)
    {
        if (TryCreateDirectionalGoal(start, out Vector2Int goal)) return goal;
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = PlayableMinX; x <= PlayableMaxX; x++)
            for (int y = PlayableMinY; y <= PlayableMaxY; y++)
                if (ManhattanDistance(start, new Vector2Int(x, y)) >= EffectiveMinStartGoalDistance)
                    candidates.Add(new Vector2Int(x, y));
        return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : FarthestPlayablePointFrom(start);
    }

    private bool TryCreateDirectionalGoal(Vector2Int start, out Vector2Int goal)
    {
        int minDistance = EffectiveMinStartGoalDistance;
        int maxStep = Mathf.Max(width, height);
        for (int i = 0; i < goalDirectionAttempts; i++)
        {
            Vector2Int direction = EightDirections[Random.Range(0, EightDirections.Length)];
            int step = Random.Range(Mathf.Max(1, minDistance / 2), maxStep + 1);
            Vector2Int jitter = new Vector2Int(Random.Range(-goalJitterRange, goalJitterRange + 1), Random.Range(-goalJitterRange, goalJitterRange + 1));
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
            Vector2Int pos = new Vector2Int(Mathf.RoundToInt(Mathf.Lerp(start.x, goal.x, t)), Mathf.RoundToInt(Mathf.Lerp(start.y, goal.y, t)));
            Vector2Int jitter = new Vector2Int(Random.Range(-waypointJitterRange, waypointJitterRange + 1), Random.Range(-waypointJitterRange, waypointJitterRange + 1));
            Vector2Int waypoint = ClampToPlayableArea(pos + jitter);
            if (waypoint != start && waypoint != goal && !waypoints.Contains(waypoint)) waypoints.Add(waypoint);
        }
        return waypoints;
    }

    private List<Vector2Int> CreateSafePath(Vector2Int start, List<Vector2Int> waypoints, Vector2Int goal)
    {
        List<Vector2Int> safePath = new List<Vector2Int>();
        HashSet<Vector2Int> added = new HashSet<Vector2Int>();
        Vector2Int current = start;
        AddPathPosition(safePath, added, current);
        foreach (var wp in waypoints) { AddSegmentPath(safePath, added, current, wp); current = wp; }
        AddSegmentPath(safePath, added, current, goal);
        return safePath;
    }

    private void AddSegmentPath(List<Vector2Int> safePath, HashSet<Vector2Int> added, Vector2Int from, Vector2Int to)
    {
        Vector2Int current = from;
        while (current != to)
        {
            bool moveX = (current.x != to.x) && ((current.y == to.y) || Random.value < 0.5f);
            if (moveX) current.x += Math.Sign(to.x - current.x);
            else current.y += Math.Sign(to.y - current.y);
            AddPathPosition(safePath, added, current);
        }
    }

    private void AddPathPosition(List<Vector2Int> safePath, HashSet<Vector2Int> added, Vector2Int pos)
    {
        if (added.Add(pos)) safePath.Add(pos);
    }

    private void MarkStartGoalAndProtectedPath(MineMapData mapData)
    {
        foreach (var p in mapData.safePath) { var t = mapData.tiles[p.x, p.y]; t.isProtected = true; t.isMine = false; }
        mapData.GetTile(mapData.start).isStart = true;
        mapData.GetTile(mapData.start).isProtected = true;
        mapData.GetTile(mapData.goal).isGoal = true;
        mapData.GetTile(mapData.goal).isProtected = true;
    }

    private void PlaceMines(MineMapData mapData)
    {
        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                TileData tile = mapData.tiles[x, y];
                tile.risk = CalculateProgressRisk(tile.position, mapData.start, mapData.goal);
                if (tile.isProtected) continue;
                float mineChance = Mathf.Clamp01(Mathf.Lerp(startMineChance, goalMineChance, tile.risk) + Random.Range(-mineChanceNoise, mineChanceNoise) + (Mathf.Max(0, mapData.floorIndex) * mineChancePerFloor));
                tile.isMine = Random.value < mineChance;
                if (tile.isMine) tile.tileCoin = Random.Range(6 * minCoinAmount, 6 * maxCoinAmount + 1);
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
                if (tile.isMine || (!allowRewardsOnSafePath && tile.isProtected)) continue;
                if (Random.value < Mathf.Lerp(startCoinChance, goalCoinChance, tile.risk)) tile.tileCoin = Random.Range(minCoinAmount, maxCoinAmount + 1);
                if (Random.value < Mathf.Lerp(startItemChance, goalItemChance, tile.risk)) tile.hasItem = true;
            }
        }
    }

    private float CalculateProgressRisk(Vector2Int p, Vector2Int s, Vector2Int g)
    {
        Vector2 start = new Vector2(s.x, s.y), goal = new Vector2(g.x, g.y), pos = new Vector2(p.x, p.y);
        Vector2 stg = goal - start;
        float lenSq = Vector2.Dot(stg, stg);
        return lenSq <= Mathf.Epsilon ? 0f : Mathf.Clamp01(Vector2.Dot(pos - start, stg) / lenSq);
    }

    private Vector2Int ClampToPlayableArea(Vector2Int p) => new Vector2Int(Mathf.Clamp(p.x, PlayableMinX, PlayableMaxX), Mathf.Clamp(p.y, PlayableMinY, PlayableMaxY));
    private Vector2Int RandomPoint(int minX, int maxX, int minY, int maxY) => new Vector2Int(Random.Range(minX, maxX + 1), Random.Range(minY, maxY + 1));
    private Vector2Int FarthestPlayablePointFrom(Vector2Int s)
    {
        Vector2Int farthest = s; int dist = -1;
        for (int x = PlayableMinX; x <= PlayableMaxX; x++)
            for (int y = PlayableMinY; y <= PlayableMaxY; y++)
            {
                int d = ManhattanDistance(s, new Vector2Int(x, y));
                if (d > dist) { farthest = new Vector2Int(x, y); dist = d; }
            }
        return farthest;
    }
    private int ManhattanDistance(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    private int EffectiveMinStartGoalDistance => minStartGoalDistance;
    private int PlayableMinX => Mathf.Clamp(margin, 0, width - 1);
    private int PlayableMaxX => Mathf.Clamp(width - 1 - margin, PlayableMinX, width - 1);
    private int PlayableMinY => Mathf.Clamp(margin, 0, height - 1);
    private int PlayableMaxY => Mathf.Clamp(height - 1 - margin, PlayableMinY, height - 1);
}