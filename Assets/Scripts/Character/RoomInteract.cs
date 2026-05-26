using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 2026.05.26
/// 신원영
/// 
/// 방과 상호작용하는 스크립트. 임시로 룸체크 기능은 비활성화함.
/// </summary>

public class RoomInteract : MonoBehaviour
{
    private DataManager data;
    private GameManager game_manager;
    void Start()
    {
        data = DataManager.Instance;
        game_manager = GameManager.Instance;
        // 숨겨진 방을 드러내는 기능은 각 칸별 랜더링 기능 추가되면 적용
        //data.e_pos_change.AddListener(RoomCheck);
    }
    public void RoomCheck(Vector2Int old_pos, Vector2Int new_pos)
    {
        // 공개 안된 칸만 처리
        if (data.map.GetTile(new_pos).isRevealed) return;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            InteractRoom();
        }
    }

    private void InteractRoom()
    {
        if(data.PlayerPos == null) return;

        TileData tile_data = data.map.GetTile(data.PlayerPos);

        if (tile_data.isMine)
        {
            data.PlayerGold -= tile_data.tileCoin;
        }
        else if (tile_data.isGoal)
        {
            game_manager.EndStage();
        }
        else if (tile_data.hasItem)
        {
            // 2026.05.26 아이템 추가는 기능 나중에 추가
        }
        else
        {
            Debug.Log("(" + tile_data.position.x + ", " + tile_data.position.y + ") : Coin Get " + tile_data.tileCoin);
            data.PlayerGold += tile_data.tileCoin;
        }
        
    }
}
