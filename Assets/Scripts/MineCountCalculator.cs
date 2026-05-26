using UnityEngine;

public interface IMineCountCalculator
{
    void Calculate(MineMapData mapData);
}

public class MineCountCalculator : IMineCountCalculator
{
    private static readonly Vector2Int[] Directions =
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

    public void Calculate(MineMapData mapData)
    {
        if (mapData == null || mapData.tiles == null)
        {
            return;
        }

        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                TileData tile = mapData.tiles[x, y];
                tile.adjacentMineCount = tile.isMine ? 0 : CountAdjacentMines(mapData, new Vector2Int(x, y));
            }
        }
    }

    private int CountAdjacentMines(MineMapData mapData, Vector2Int position)
    {
        int count = 0;

        for (int i = 0; i < Directions.Length; i++)
        {
            Vector2Int next = position + Directions[i];

            if (!mapData.IsInBounds(next))
            {
                continue;
            }

            TileData neighbor = mapData.tiles[next.x, next.y];

            if (neighbor != null && neighbor.isMine)
            {
                count++;
            }
        }

        return count;
    }
}
