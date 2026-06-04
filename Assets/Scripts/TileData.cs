using UnityEngine;

[System.Serializable]
public class TileData
{
    public Vector2Int position;

    public bool isMine = false;
    public bool isProtected = false; // 지뢰가 설치되면 안 되는 칸
    public bool isStart = false;
    public bool isGoal = false;
    public bool isRevealed = false;

    public bool isFlagged = false;

    public int adjacentMineCount = 0;

    public int tileCoin = 0;
    public bool hasItem = false;
    public string itemId = "";

    public float risk = 0f;

    public TileData()
    {
    }

    public TileData(Vector2Int position)
    {
        this.position = position;
    }
}
