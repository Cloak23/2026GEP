using UnityEngine;
using UnityEngine.Tilemaps;

public class MineMapRenderer : MonoBehaviour
{
    public static MineMapRenderer Instance;

    [Header("Source")]
    public MineMapGenerator mapGenerator;
    public MineMapTileSpriteSet tileSet;

    [Header("Tilemaps")]
    public Tilemap wallTilemap; // 맵 크기에 맞는 벽과 바닥 렌더링
    public Tilemap overlayTilemap; // 지뢰, 숫자 등을 맵 위에 렌더링
    public Tilemap debugTilemap; // 디버깅용

    [Header("Rendering")]
    public bool generateMapBeforeRender = true;
    public bool renderOnStart = true;
    public bool clearBeforeRender = true;
    public bool revealMines = true;
    public bool connectAllAdjacentRooms = true;
    public bool showSafePathDebug = false; // 렌더링시에 체크해줘야 디버깅 safe path 보임
    public bool showSafePathConnections = true;

    public MineMapData CurrentMap { get; private set; }

    public GameObject coverPrefab;       //덮개
    public Transform coverContainer;

    private void Reset()
    {
        mapGenerator = GetComponent<MineMapGenerator>();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (renderOnStart)
        {
            GenerateAndRender();
        }
    }

    [ContextMenu("Generate And Render Map")]
    public void GenerateAndRender()
    {
        if (!ValidateReferences(true))
        {
            return;
        }

        CurrentMap = generateMapBeforeRender
            ? mapGenerator.GenerateMap()
            : mapGenerator.LastGeneratedMap;

        if (CurrentMap == null)
        {
            Debug.LogWarning("MineMapRenderer could not render because there is no generated MineMapData.", this);
            return;
        }

        RenderMap(CurrentMap);
    }

    public void RenderMap(MineMapData mapData)
    {
        if (!ValidateReferences(false) || mapData == null)
        {
            return;
        }

        CurrentMap = mapData;

        if (clearBeforeRender)
        {
            Clear();
        }

        RenderRoomCenters(mapData);
        RenderSeparators(mapData);
        RenderWallJunctions(mapData);
        RenderOverlays(mapData);
        RenderSafePathDebug(mapData);

        SpawnCovers(mapData);

        wallTilemap.CompressBounds();
        overlayTilemap.CompressBounds();

        if (debugTilemap != null)
        {
            debugTilemap.CompressBounds();
        }
    }

    [ContextMenu("Clear Rendered Map")]
    public void Clear()
    {
        if (wallTilemap != null)
        {
            wallTilemap.ClearAllTiles();
        }

        if (overlayTilemap != null)
        {
            overlayTilemap.ClearAllTiles();
        }

        if (debugTilemap != null)
        {
            debugTilemap.ClearAllTiles();
        }

        if (coverContainer != null)
        {
            for (int i = coverContainer.childCount - 1; i >= 0; i--)
            {
                if (Application.isPlaying) Destroy(coverContainer.GetChild(i).gameObject);
                else DestroyImmediate(coverContainer.GetChild(i).gameObject);
            }
        }


    }

    private void RenderRoomCenters(MineMapData mapData)
    {
        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                Vector2Int mapPosition = new Vector2Int(x, y);
                TileBase roomTile = tileSet.GetWallShapeTile(WallMask.None);

                if (roomTile != null)
                {
                    wallTilemap.SetTile(ToRoomCell(mapPosition), roomTile);
                }
            }
        }
    }

    private void RenderSeparators(MineMapData mapData)
    {
        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                Vector2Int from = new Vector2Int(x, y);
                if (x == 0)
                {
                    RenderSeparator(mapData, from, Vector2Int.left);
                }

                if (y == 0)
                {
                    RenderSeparator(mapData, from, Vector2Int.down);
                }

                RenderSeparator(mapData, from, Vector2Int.right);
                RenderSeparator(mapData, from, Vector2Int.up);
            }
        }
    }

    private void RenderSeparator(MineMapData mapData, Vector2Int from, Vector2Int direction)
    {
        Vector2Int to = from + direction;
        Vector3Int separatorCell = ToRoomCell(from) + new Vector3Int(direction.x, direction.y, 0);

        if (ShouldConnect(mapData, from, to))
        {
            wallTilemap.SetTile(separatorCell, tileSet.GetPassageTile(direction));
            return;
        }

        WallMask wallMask = GetClosedSeparatorWallMask(direction);

        wallTilemap.SetTile(separatorCell, tileSet.GetWallShapeTile(wallMask));
    }

    private WallMask GetClosedSeparatorWallMask(Vector2Int direction)
    {
        if (direction.x > 0)
        {
            return WallMask.Right;
        }

        if (direction.x < 0)
        {
            return WallMask.Left;
        }

        if (direction.y > 0)
        {
            return WallMask.Top;
        }

        if (direction.y < 0)
        {
            return WallMask.Bottom;
        }

        return WallMask.None;
    }

    private void RenderWallJunctions(MineMapData mapData)
    {
        int visualWidth = mapData.width * 2 + 1;
        int visualHeight = mapData.height * 2 + 1;

        for (int x = 0; x < visualWidth; x += 2)
        {
            for (int y = 0; y < visualHeight; y += 2)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                WallMask wallMask = CalculateJunctionWallMask(mapData, cell);
                wallTilemap.SetTile(cell, tileSet.GetWallShapeTile(wallMask));
            }
        }
    }

    private WallMask CalculateJunctionWallMask(MineMapData mapData, Vector3Int junctionCell)
    {
        WallMask mask = WallMask.None;

        if (IsSeparatorAt(junctionCell + Vector3Int.up, true, mapData))
        {
            mask |= WallMask.Top;
        }

        if (IsSeparatorAt(junctionCell + Vector3Int.right, false, mapData))
        {
            mask |= WallMask.Right;
        }

        if (IsSeparatorAt(junctionCell + Vector3Int.down, true, mapData))
        {
            mask |= WallMask.Bottom;
        }

        if (IsSeparatorAt(junctionCell + Vector3Int.left, false, mapData))
        {
            mask |= WallMask.Left;
        }

        return mask;
    }

    private bool IsSeparatorAt(Vector3Int separatorCell, bool verticalSeparator, MineMapData mapData)
    {
        int visualWidth = mapData.width * 2 + 1;
        int visualHeight = mapData.height * 2 + 1;

        if (separatorCell.x < 0 || separatorCell.x >= visualWidth || separatorCell.y < 0 || separatorCell.y >= visualHeight)
        {
            return false;
        }

        if (verticalSeparator)
        {
            if (separatorCell.x % 2 != 0 || separatorCell.y % 2 == 0)
            {
                return false;
            }

            return true;
        }

        if (separatorCell.x % 2 == 0 || separatorCell.y % 2 != 0)
        {
            return false;
        }

        return true;
    }

    private void RenderOverlays(MineMapData mapData)
    {
        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                TileData tileData = mapData.tiles[x, y];
                TileBase overlayTile = tileSet.GetOverlayTile(tileData, revealMines);

                if (overlayTile == null)
                {
                    continue;
                }

                overlayTilemap.SetTile(ToRoomCell(new Vector2Int(x, y)), overlayTile);
            }
        }
    }

    private void RenderSafePathDebug(MineMapData mapData)
    {
        if (!showSafePathDebug || debugTilemap == null || tileSet.safePathDebugTile == null || mapData.safePath == null)
        {
            return;
        }

        for (int i = 0; i < mapData.safePath.Count; i++)
        {
            Vector2Int pathPosition = mapData.safePath[i];

            if (!mapData.IsInBounds(pathPosition))
            {
                continue;
            }

            debugTilemap.SetTile(ToRoomCell(pathPosition), tileSet.safePathDebugTile);

            if (!showSafePathConnections || i >= mapData.safePath.Count - 1)
            {
                continue;
            }

            Vector2Int nextPosition = mapData.safePath[i + 1];

            if (!mapData.IsInBounds(nextPosition))
            {
                continue;
            }

            Vector2Int direction = nextPosition - pathPosition;

            if (Mathf.Abs(direction.x) + Mathf.Abs(direction.y) != 1)
            {
                continue;
            }

            Vector3Int connectorCell = ToRoomCell(pathPosition) + new Vector3Int(direction.x, direction.y, 0);
            debugTilemap.SetTile(connectorCell, tileSet.safePathDebugTile);
        }
    }

    private bool ShouldConnect(MineMapData mapData, Vector2Int from, Vector2Int to)
    {
        if (!mapData.IsInBounds(from) || !mapData.IsInBounds(to))
        {
            return false;
        }

        if (!connectAllAdjacentRooms)
        {
            return IsSafePathConnection(mapData, from, to);
        }

        return true;
    }

    private bool IsSafePathConnection(MineMapData mapData, Vector2Int from, Vector2Int to)
    {
        if (mapData.safePath == null || mapData.safePath.Count < 2)
        {
            return false;
        }

        for (int i = 0; i < mapData.safePath.Count - 1; i++)
        {
            Vector2Int a = mapData.safePath[i];
            Vector2Int b = mapData.safePath[i + 1];

            if ((a == from && b == to) || (a == to && b == from))
            {
                return true;
            }
        }

        return false;
    }

    public Vector3Int ToRoomCell(Vector2Int mapPosition)
    {
        return new Vector3Int(mapPosition.x * 2 + 1, mapPosition.y * 2 + 1, 0);
    }

    private bool ValidateReferences(bool requireGenerator)
    {
        if (requireGenerator && mapGenerator == null)
        {
            Debug.LogError("MineMapRenderer requires a MineMapGenerator reference.", this);
            return false;
        }

        if (tileSet == null)
        {
            Debug.LogError("MineMapRenderer requires a MineMapTileSpriteSet reference.", this);
            return false;
        }

        if (wallTilemap == null)
        {
            Debug.LogError("MineMapRenderer requires a wall Tilemap reference.", this);
            return false;
        }

        if (overlayTilemap == null)
        {
            Debug.LogError("MineMapRenderer requires an overlay Tilemap reference.", this);
            return false;
        }

        if (showSafePathDebug && debugTilemap == null)
        {
            Debug.LogError("MineMapRenderer requires a debug Tilemap reference when Show Safe Path Debug is enabled.", this);
            return false;
        }

        return true;
    }


    private void SpawnCovers(MineMapData mapData)
    {
        if (coverPrefab == null) return;

        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                TileData tileData = mapData.tiles[x, y];

                // 시작 지점은 처음부터 보이게 하려면 덮개를 생성하지 않고 바로 열림 처리
                if (tileData.isStart)
                {
                    tileData.isRevealed = true;
                    continue;
                }

                // 타일맵의 그리드 좌표를 실제 화면(월드) 좌표로 변환
                Vector3Int cellPos = ToRoomCell(new Vector2Int(x, y));
                Vector3 worldPos = wallTilemap.GetCellCenterWorld(cellPos);

                // 화면에 덮개 프리팹 생성! (coverContainer 안에 깔끔하게 정리됨)
                GameObject newCover = Instantiate(coverPrefab, worldPos, Quaternion.identity, coverContainer);

                // 방금 만든 TileRevealByTouch 스크립트를 찾아 실제 맵 데이터(주민등록증) 쥐어주기
                TileRevealByTouch revealScript = newCover.GetComponent<TileRevealByTouch>();
                if (revealScript != null)
                {
                    revealScript.SetupLogicalTile(tileData);
                }
            }
        }



    }
}
