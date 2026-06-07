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

    [Header("깃발 프리팹")]
    public GameObject flag;

    private DataManager data;
    private bool move_lock; 
    private bool isRouletteActive = false;
    private UnityAction<Vector2Int, Vector2Int> move_action;
    private MineMapRenderer m_Renderer;
    private GameManager game_manager;

    private Dictionary<Vector2Int, GameObject> spawnedflag = new Dictionary<Vector2Int, GameObject>();

    private void Start()
    {
        data = DataManager.Instance;
        m_Renderer = MineMapRenderer.Instance;
        game_manager = GameManager.Instance;
        move_action = (a, b) => { StartCoroutine(MoveAnimation(a, b)); };
        data.e_pos_change.AddListener(move_action);
        data.e_roulette_start.AddListener(() => { isRouletteActive = true; });
        data.e_roulette_end.AddListener(() => { isRouletteActive = false; });
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
        if (move_lock || AIManager.windowOpen || isRouletteActive)
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            ToggleFlagWithMouse();
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
        //깃발 유무 확인
        Vector2Int nextPos = data.PlayerPos + dir;

        if (data.map != null &&
            nextPos.x >= 0 && nextPos.x < data.map.width &&
            nextPos.y >= 0 && nextPos.y < data.map.height)
        {
            var targetTile = data.map.tiles[nextPos.x, nextPos.y];

            // 깃발 꽂혀있다면 종료(return)
            if (targetTile != null && targetTile.isFlagged)
            {
                Debug.Log("깃발이 꽂힌 곳으로는 이동할 수 없습니다.");
                return;
            }
        }



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




    private void ToggleFlagWithMouse()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector3Int cellPos = tilemap.WorldToCell(mouseWorldPos);

        if (data.map != null)
        {
            Vector2Int gridPos = new Vector2Int(-1, -1);

            // 화면 좌표 -> 배열 좌표 역추적
            for (int x = 0; x < data.map.width; x++)
            {
                for (int y = 0; y < data.map.height; y++)
                {
                    if (m_Renderer.ToRoomCell(new Vector2Int(x, y)) == cellPos)
                    {
                        gridPos = new Vector2Int(x, y);
                        break;
                    }
                }
                if (gridPos.x != -1) break;
            }

            if (gridPos.x != -1)
            {
                var clickedTile = data.map.tiles[gridPos.x, gridPos.y];

                // 열리지 않은 타일에만 깃발
                if (clickedTile != null && !clickedTile.isRevealed)
                {
                    clickedTile.isFlagged = !clickedTile.isFlagged;

                    if (clickedTile.isFlagged)
                    {
                        // 깃발 꽂기
                        if (flag != null)
                        {
                            Vector3 spawnPos = tilemap.GetCellCenterWorld(cellPos);
                            spawnPos.z = -1f;

                            GameObject newFlag = Instantiate(flag, spawnPos, Quaternion.identity);
                            spawnedflag.Add(gridPos, newFlag); // 보관함에 저장
                        }
                        else
                        {
                            Debug.LogWarning("인스펙터에 Flag Prefab이 연결되지 않았습니다!");
                        }
                    }
                    else
                    {
                        // 깃빨 뽑기
                        if (spawnedflag.ContainsKey(gridPos))
                        {
                            Destroy(spawnedflag[gridPos]);
                            spawnedflag.Remove(gridPos); // 보관함에서 제거
                        }
                    }
                    Debug.Log($"[{gridPos.x}, {gridPos.y}] 깃발 상태: {clickedTile.isFlagged}");
                }
            }
        }

    }




}
