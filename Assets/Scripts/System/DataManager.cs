using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// 2026.05.25
/// 신원영
/// 
/// 게임 내의 데이터를 싱글톤으로 접근하는 스크립트
/// 
/// update 2026.05.26 : map generator와 합병으로 DataManager 내부의 map 삭제. 맵 데이터는 Renderer에서 사용하기로 함.
/// </summary>

public class DataManager : MonoBehaviour
{
    [Header("외부 컴포넌트 연결")]
    public MineMapGenerator m_Generator;

    [Header("수치 변경")]
    public int PLAYER_INIT_GOLD = 10;
    public int ROULETTE_MAX_GOLD = 70;
    public int ROULETTE_MIN_GOLD = -20;
    public List<int> REQUIRE_GOLD_LIST;

    [Header("상점 업그레이드 레벨")]
    public int level_revival = 0;   // 부활 레벨
    public int level_roulette_bonus = 0;  // 타일 오픈 레벨
    public int level_AIrequest = 0; // AI 요청 레벨

    public int current_revival_count = 0;
    public int current_roulette_bonus_count = 0;
    public int current_AIrequest_count = 0;

    public static DataManager Instance { get; private set; }

    public UnityEvent<Vector2Int, Vector2Int> e_pos_change = new();
    public UnityEvent<int, int> e_gold_change = new();
    public UnityEvent e_roulette_start = new();
    public UnityEvent e_roulette_end = new();
    public UnityEvent e_revival;

    private int player_gold = 10;
    private Vector2Int player_pos = new Vector2Int(0, 0);
    private MineMapRenderer m_Renderer;
    private GameManager game_manager;

    public MineMapData map
    {
        get => m_Renderer.CurrentMap;
        private set
        {
            map = value;
        }
    }

    public int PlayerGold
    {
        get => player_gold;
        set
        {
            Debug.Log("Player Gold Change " + player_gold + " => " + value);

            int old_gold = player_gold;
            player_gold = value;

            e_gold_change?.Invoke(old_gold, value);

            if (player_gold <= 0)
            {
                GameOver();
            }

        }
    }

    private int player_life = 0;
    public int PlayerLife 
    {
        get => player_life;
        set
        {
            player_life = value;
        }
    }

    public void AddLife(int amount)
    {
        player_life += amount;
    }


    public Vector2Int PlayerPos
    {
        get => player_pos;
        set
        {
            if (player_pos != value)
            {
                Debug.Log("(" + player_pos.x + " " + player_pos.y + ") (" + value.x + " " + value.y + ")");

                if (m_Renderer.CurrentMap.IsInBounds(value))
                {
                    Vector2Int old_pos = player_pos;
                    player_pos = value;

                    e_pos_change?.Invoke(old_pos, value);
                }
                else
                {
                    Debug.LogError("Bound Error : 플레이어가 영역을 벗어남");
                    e_pos_change?.Invoke(player_pos, player_pos);
                }
            }
        }
    }

    private void Start()
    {
        m_Renderer = MineMapRenderer.Instance;
        game_manager = GameManager.Instance;
        game_manager.e_stage_start.AddListener(InitStage);
    }

    public void InitStage()
    {
        if (m_Generator == null) m_Generator = FindObjectOfType<MineMapGenerator>();

        if (m_Generator != null)
        {
            current_revival_count = level_revival;
            current_roulette_bonus_count = level_roulette_bonus;
            current_AIrequest_count = level_AIrequest;
        }

        PlayerPos = map.start;
        if(game_manager.stage_index == 0)
        {
            PlayerGold = PLAYER_INIT_GOLD;
        }
    }

    public void Move(Vector2Int dir)
    {
        PlayerPos += dir;
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

    public IEnumerator LerpMove(Vector3 old_pos, Vector3 new_pos, Transform transform, float duration)
    {
        float currentTime = 0f;

        transform.position = old_pos;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;

            // 현재 경과 시간을 전체 시간으로 나누어 0~1 사이의 비율(t)을 만듬
            float t = currentTime / duration;

            transform.position = Vector3.Lerp(old_pos, new_pos, t);

            yield return null;
        }

        transform.position = new_pos;
    }

    private void GameOver()
    {

        if (current_revival_count > 0)
        {
            current_revival_count--;
            PlayerGold = 10;
            e_revival?.Invoke();
            return;
        }

        AddLife(1);
        SceneManager.LoadScene("GameOver");
    }

    public void UseAIQuestion()
    {
        if (DataManager.Instance.current_AIrequest_count > 0)
        {
            DataManager.Instance.current_AIrequest_count--; // 토큰 차감

        }
    }



    // 디버그용
    // 숫자맵을 SlotData 맵으로 변환
    // Generator와 합친 이후론 사용 X
    public SlotData[,] IntMapToSlotMap(int[,] input_map)
    {
        int width = input_map.GetLength(0);
        int height = input_map.GetLength(1);
        SlotData[,] slot_map = new SlotData[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                int value = input_map[i, j];
                RoomType type;
                switch (value)
                {
                    case 0:
                        type = RoomType.Empty;
                        break;
                    case 1:
                        type = RoomType.Mine;
                        break;
                    case 2:
                        type = RoomType.Item;
                        break;
                    default:
                        Debug.LogError("Invalid map value: " + value + " at (" + i + ", " + j + ")");
                        type = RoomType.Empty;
                        break;
                }
                slot_map[i, j] = new SlotData(i, j, type);
            }
        }
        return slot_map;
    }

}
