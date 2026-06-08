using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "MineMapTileSpriteSet", menuName = "Mine Map/Tile Set")]
public class MineMapTileSpriteSet : ScriptableObject
{
    [Header("Wall Shape Tiles")]
    [Tooltip("No wall lines in this cell.")]
    public TileBase noWallTile;
    [Tooltip("Passage cell between left and right rooms. Use the horizontal corridor tile.")]
    public TileBase passageHorizontalTile;
    [Tooltip("Passage cell between top and bottom rooms. Use the vertical corridor tile.")]
    public TileBase passageVerticalTile;
    public TileBase wallTopTile;
    public TileBase wallRightTile;
    public TileBase wallBottomTile;
    public TileBase wallLeftTile;

    [Header("Two-Way Wall Shapes")]
    public TileBase wallTopRightTile;
    public TileBase wallRightBottomTile;
    public TileBase wallBottomLeftTile;
    public TileBase wallLeftTopTile;
    [Tooltip("Walls on Top+Bottom. This is usually the horizontal corridor tile, because the path opens Left+Right.")]
    public TileBase wallTopBottomTile;
    [Tooltip("Walls on Left+Right. This is usually the vertical corridor tile, because the path opens Top+Bottom.")]
    public TileBase wallLeftRightTile;

    [Header("Three-Way Wall Shapes")]
    [Tooltip("Walls on Left+Right+Bottom. This is the ㅜ shaped wall tile.")]
    public TileBase wallLeftRightBottomTile;
    [Tooltip("Walls on Top+Right+Bottom. This is the ㅏ shaped wall tile.")]
    public TileBase wallTopRightBottomTile;
    [Tooltip("Walls on Left+Top+Right. This is the ㅗ shaped wall tile.")]
    public TileBase wallLeftTopRightTile;
    [Tooltip("Walls on Top+Bottom+Left. This is the ㅓ shaped wall tile.")]
    public TileBase wallTopBottomLeftTile;

    [Header("Four-Way Wall Shape")]
    public TileBase wallAllTile;

    [Header("Special Tiles")]
    public TileBase startTile;
    public TileBase goalTile;
    public TileBase mineTile;
    public TileBase hiddenMineTile;

    [Header("Cover")]
    [Tooltip("Tile used by MineMapRenderer to hide unrevealed map cells.")]
    public TileBase coverTile;

    [Header("Debug")]
    public TileBase safePathDebugTile;

    [Header("Numbers")]
    [Tooltip("Index 0 is optional. Index 1-8 should match adjacent mine counts.")]
    public TileBase[] numberTiles = new TileBase[9];

    [Header("Currency")]
    public TileBase coinTile;
    public List<AmountTileEntry> coinAmountTiles = new List<AmountTileEntry>();

    [Header("Items")]
    public TileBase defaultItemTile;
    public List<ItemTileEntry> itemTiles = new List<ItemTileEntry>();

    [Header("Fallback")]
    public TileBase missingTile;

    public TileBase GetWallShapeTile(WallMask wallMask)
    {
        TileBase tile = wallMask switch
        {
            WallMask.None => noWallTile,
            WallMask.Top => wallTopTile,
            WallMask.Right => wallRightTile,
            WallMask.Bottom => wallBottomTile,
            WallMask.Left => wallLeftTile,
            WallMask.Top | WallMask.Right => wallTopRightTile,
            WallMask.Right | WallMask.Bottom => wallRightBottomTile,
            WallMask.Bottom | WallMask.Left => wallBottomLeftTile,
            WallMask.Left | WallMask.Top => wallLeftTopTile,
            WallMask.Top | WallMask.Bottom => wallTopBottomTile,
            WallMask.Left | WallMask.Right => wallLeftRightTile,
            WallMask.Left | WallMask.Right | WallMask.Bottom => wallLeftRightBottomTile,
            WallMask.Top | WallMask.Right | WallMask.Bottom => wallTopRightBottomTile,
            WallMask.Left | WallMask.Top | WallMask.Right => wallLeftTopRightTile,
            WallMask.Top | WallMask.Bottom | WallMask.Left => wallTopBottomLeftTile,
            WallMask.Top | WallMask.Right | WallMask.Bottom | WallMask.Left => wallAllTile,
            _ => null
        };

        return GetFallback(tile);
    }

    public TileBase GetPassageTile(Vector2Int direction)
    {
        if (direction.x != 0)
        {
            return GetFallback(passageHorizontalTile);
        }

        if (direction.y != 0)
        {
            return GetFallback(passageVerticalTile);
        }

        return GetFallback(noWallTile);
    }

    public TileBase GetOverlayTile(TileData tile, bool revealMines = true)
    {
        if (tile == null)
        {
            return null;
        }

        if (tile.isStart)
        {
            return startTile;
        }

        if (tile.isGoal)
        {
            return goalTile;
        }

        if (tile.isMine)
        {
            return revealMines ? mineTile : hiddenMineTile;
        }

        if (tile.adjacentMineCount > 0)
        {
            return GetNumberTile(tile.adjacentMineCount);
        }

        if (tile.hasItem)
        {
            return GetItemTile(tile.itemId);
        }

        if (tile.tileCoin > 0)
        {
            return GetCoinTile(tile.tileCoin);
        }


        return null;
    }

    public TileBase GetNumberTile(int adjacentMineCount)
    {
        if (numberTiles == null || numberTiles.Length == 0)
        {
            return missingTile;
        }

        int index = Mathf.Clamp(adjacentMineCount, 0, numberTiles.Length - 1);
        return GetFallback(numberTiles[index]);
    }

    public TileBase GetCoinTile(int amount)
    {
        for (int i = 0; i < coinAmountTiles.Count; i++)
        {
            AmountTileEntry entry = coinAmountTiles[i];

            if (entry != null && amount >= entry.minAmount && amount <= entry.maxAmount && entry.tile != null)
            {
                return entry.tile;
            }
        }

        return GetFallback(coinTile);
    }

    public TileBase GetItemTile(string itemId)
    {
        if (!string.IsNullOrEmpty(itemId))
        {
            for (int i = 0; i < itemTiles.Count; i++)
            {
                ItemTileEntry entry = itemTiles[i];

                if (entry != null && entry.itemId == itemId && entry.tile != null)
                {
                    return entry.tile;
                }
            }
        }

        return GetFallback(defaultItemTile);
    }

    private TileBase GetFallback(TileBase tile)
    {
        return tile != null ? tile : missingTile;
    }
}

[System.Flags]
public enum WallMask
{
    None = 0,
    Top = 1,
    Right = 2,
    Bottom = 4,
    Left = 8
}

[System.Serializable]
public class AmountTileEntry
{
    public int minAmount = 1;
    public int maxAmount = 1;
    public TileBase tile;
}

[System.Serializable]
public class ItemTileEntry
{
    public string itemId;
    public TileBase tile;
}
