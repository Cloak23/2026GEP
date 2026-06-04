using System.Collections.Generic;
using System.Text;
using UnityEngine;

[System.Serializable]
public class MineMapData
{
    public int width;
    public int height;
    public int floorIndex;

    public TileData[,] tiles;

    public Vector2Int start;
    public Vector2Int goal;

    public List<Vector2Int> safePath;
    public List<Vector2Int> waypoints;

    public MineMapData()
    {
        safePath = new List<Vector2Int>();
        waypoints = new List<Vector2Int>();
    }

    public MineMapData(int width, int height, int floorIndex)
    {
        this.width = width;
        this.height = height;
        this.floorIndex = floorIndex;
        tiles = new TileData[width, height];
        safePath = new List<Vector2Int>();
        waypoints = new List<Vector2Int>();
    }

    public bool IsInBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < width && position.y >= 0 && position.y < height;
    }

    public TileData GetTile(Vector2Int position)
    {
        if (!IsInBounds(position))
        {
            return null;
        }

        return tiles[position.x, position.y];
    }

    public string ToDebugString(bool showLegend = true)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Mine Map Debug | Floor: {floorIndex} | Size: {width}x{height}");
        builder.AppendLine($"Start: {start} | Goal: {goal} | SafePath: {safePath?.Count ?? 0}");

        if (showLegend)
        {
            builder.AppendLine("Legend: S Start, G Goal, * Protected Path, M Mine, I Item, C Coin, 1-8 Adjacent Mines, . Empty");
        }

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                builder.Append(GetDebugSymbol(tiles[x, y]));
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }

    public void LogDebugMap(bool showLegend = true)
    {
        Debug.Log(ToDebugString(showLegend));
    }

    private char GetDebugSymbol(TileData tile)
    {
        if (tile == null)
        {
            return '?';
        }

        if (tile.isStart)
        {
            return 'S';
        }

        if (tile.isGoal)
        {
            return 'G';
        }

        if (tile.isMine)
        {
            return 'M';
        }

        if (tile.isProtected)
        {
            return '*';
        }

        if (tile.hasItem)
        {
            return 'I';
        }

        if (tile.tileCoin > 0)
        {
            return 'C';
        }

        if (tile.adjacentMineCount > 0)
        {
            return tile.adjacentMineCount.ToString()[0];
        }

        return '.';
    }


}
