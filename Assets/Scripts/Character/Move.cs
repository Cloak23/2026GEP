using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

/// <summary>
/// 2026.05.25
/// 신원영
/// 
/// 이동 입력 받는 스크립트
/// </summary>

public class Move : MonoBehaviour
{
    [Header("외부 컴포넌트 연결")]
    public Tilemap tilemap;

    [Header("플레이어 스탯")]
    public float anim_duration;

    private DataManager data;
    private bool move_lock;
    private UnityAction<Vector2Int, Vector2Int> move_action;

    private void Start()
    {
        data = DataManager.Instance;
        move_action = (a, b) => { StartCoroutine(MoveAnimation(a, b)); };
        data.pos_change.AddListener(move_action);
        transform.position = tilemap.GetCellCenterWorld(new Vector3Int(data.PlayerPos.x, data.PlayerPos.y, 0));
    }

    private void OnDestroy()
    {
        if (data != null && move_action != null)
        {
            data.pos_change.RemoveListener(move_action);
        }
    }

    void Update()
    {
        if (move_lock)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            MoveGrid(Vector2Int.up);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            MoveGrid(Vector2Int.down);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            MoveGrid(Vector2Int.left);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            MoveGrid(Vector2Int.right);
        }
    }

    public void MoveGrid(Vector2Int dir)
    {
        move_lock = true;

        Debug.Log("Player Pos : " + data.PlayerPos.x + ", " + data.PlayerPos.y);

        Vector2Int next_grid_pos = data.PlayerPos + dir;

        int map_width = data.map.GetLength(0);
        int map_height = data.map.GetLength(1);

        if (next_grid_pos.x < 0 || next_grid_pos.x >= map_width || next_grid_pos.y < 0 || next_grid_pos.y >= map_height)
        {
            Debug.LogWarning("가장자리 도착");
            move_lock = false;
            return;
        }

        data.Move(dir);
    }

    IEnumerator MoveAnimation(Vector2Int old_pos, Vector2Int new_pos)
    {
        // 이전 위치 변환
        Vector3 old_player_pos = tilemap.GetCellCenterWorld(new Vector3Int(old_pos.x, old_pos.y, 0));

        // 새로운 위치 변환
        Vector3 new_player_pos = tilemap.GetCellCenterWorld(new Vector3Int(new_pos.x, new_pos.y, 0));

        // 맵이랑 겹치도록
        old_player_pos.z = 0f;
        new_player_pos.z = 0f;

        yield return StartCoroutine(data.LerpMove(old_player_pos, new_player_pos, transform, anim_duration));

        move_lock = false;
    }
}
