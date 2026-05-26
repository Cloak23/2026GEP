using System.Collections.Generic;
using UnityEngine;

// path가 실제로 존재하는지 BFS 탐색
public class PathValidator
{
    private static readonly Vector2Int[] FourDirections =
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0)
    };

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

    public bool HasValidPath(MineMapData mapData, bool allowDiagonalMovement = false)
    {
        if (mapData == null || mapData.tiles == null)
        {
            return false;
        }

        if (!mapData.IsInBounds(mapData.start) || !mapData.IsInBounds(mapData.goal))
        {
            return false;
        }

        if (mapData.GetTile(mapData.start).isMine || mapData.GetTile(mapData.goal).isMine)
        {
            return false;
        }

        Vector2Int[] directions = allowDiagonalMovement ? EightDirections : FourDirections;
        bool[,] visited = new bool[mapData.width, mapData.height];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        visited[mapData.start.x, mapData.start.y] = true;
        queue.Enqueue(mapData.start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == mapData.goal)
            {
                return true;
            }

            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int next = current + directions[i];

                if (!mapData.IsInBounds(next) || visited[next.x, next.y])
                {
                    continue;
                }

                TileData nextTile = mapData.tiles[next.x, next.y];

                if (nextTile == null || nextTile.isMine)
                {
                    continue;
                }

                visited[next.x, next.y] = true;
                queue.Enqueue(next);
            }
        }

        return false;
    }
}
