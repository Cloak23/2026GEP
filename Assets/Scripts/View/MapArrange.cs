using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 2026.05.25
/// 신원영
/// 
/// 맵 데이터를 유니티 오브젝트로 시각화 하는 클래스
/// </summary>

public class MapArrange : MonoBehaviour
{
    [Header("외부 컴포넌트 연결")]
    public Tilemap tilemap;

    [Header("타일 관련 에셋들")]
    public Tile mystery_tile;
    public Tile empty_tile;
    public Tile mine_tile;
    public Tile item_tile;
    public Tile[] num_tile;

    private DataManager data;

    private readonly int[,] DEBUG_MAP_DATA = new int[,]
    {
        {0, 0, 0, 0, 0},
        {0, 1, 1, 1, 0},
        {0, 0, 1, 0, 0},
        {0, 0, 1, 0, 0},
        {1, 1, 0, 1, 1}
    };

    private void Start()
    {
        data = DataManager.Instance;
        data.map = data.IntMapToSlotMap(DEBUG_MAP_DATA);
        MapDataToView(data.map);
    }

    // 맵 데이터 시각화
    void MapDataToView(SlotData[,] map_data)
    {
        tilemap.ClearAllTiles();

        int xSize = map_data.GetLength(0);
        int ySize = map_data.GetLength(1);

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                SlotData slot = map_data[x, y];
                Tile tile = SlotToTile(slot);

                tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            }
        }
    }


    // SlotData를 Tile 형태로 시각화
    public Tile SlotToTile(SlotData slot)
    {
        if(!slot.isVisible)
        {
            return mystery_tile;
        }

        switch(slot.type)
        {
            case RoomType.Empty:
                return empty_tile;
            case RoomType.Mine:
                return mine_tile;
            case RoomType.Item:
                return item_tile;
            case RoomType.Number:
                return num_tile[slot.mine_num];
            default:
                Debug.LogError("Invalid RoomType: " + slot.type);
                return null;
        }
    }
}
