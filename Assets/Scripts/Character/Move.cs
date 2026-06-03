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
    private MineMapRenderer m_Renderer;
    private GameManager game_manager;

    private void Start()
    {
        data = DataManager.Instance;
        m_Renderer = MineMapRenderer.Instance;
        game_manager = GameManager.Instance;
        move_action = (a, b) => { StartCoroutine(MoveAnimation(a, b)); };
        data.e_pos_change.AddListener(move_action);
    }

    private void OnDestroy()
    {
        if (data != null && move_action != null)
        {
            data.e_pos_change.RemoveListener(move_action);
        }
    }

    void Update()
    {
        //move_lock 에 text입력 시 이동 방지 코드 추가
        if (move_lock || AIManager.windowOpen)
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

        //Vector2Int next_grid_pos = data.PlayerPos + dir;

        //int map_width = data.map.width;
        //int map_height = data.map.height;

        //if (next_grid_pos.x < 0 || next_grid_pos.x >= map_width || next_grid_pos.y < 0 || next_grid_pos.y >= map_height)
        //{
        //    Debug.LogWarning("가장자리 도착");
        //    move_lock = false;
        //    return;
        //}

        data.Move(dir);
    }

    IEnumerator MoveAnimation(Vector2Int old_pos, Vector2Int new_pos)
    {
        if (old_pos.Equals(new_pos))
        {
            move_lock = false;
            yield break;
        }

        float distance = Vector2Int.Distance(old_pos, new_pos);
        bool isTeleport = distance > 1.5f;

        Collider2D playerCollider = GetComponent<Collider2D>();

        // 맵을 넘어가는 중에는 지뢰와 충돌하지 않도록 설정
        if (isTeleport && playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        // 이전 위치 변환
        Vector3 old_player_pos = tilemap.GetCellCenterWorld(m_Renderer.ToRoomCell(old_pos));

        // 새로운 위치 변환
        Vector3 new_player_pos = tilemap.GetCellCenterWorld(m_Renderer.ToRoomCell(new_pos));

        // 맵이랑 겹치도록
        old_player_pos.z = 0f;
        new_player_pos.z = 0f;

        yield return StartCoroutine(data.LerpMove(old_player_pos, new_player_pos, transform, anim_duration));

        if (isTeleport && playerCollider != null)
        {
            playerCollider.enabled = true;
        }


        move_lock = false;
    }
}
