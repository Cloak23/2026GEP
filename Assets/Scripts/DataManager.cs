using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 2026.05.25
/// 신원영
/// 
/// 게임 내의 데이터를 싱글톤으로 접근하는 스크립트
/// </summary>

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    public SlotData[,] map;
    public UnityEvent<Vector2Int, Vector2Int> pos_change = new();
    private Vector2Int player_pos = new Vector2Int(0, 0);

    public Vector2Int PlayerPos
    {
        get => player_pos;
        set
        {
            if (player_pos != value)
            {
                Debug.Log("(" + player_pos.x + " " + player_pos.y + ") (" + value.x + " " + value.y + ")");
                
                Vector2Int old_pos = player_pos;
                player_pos = value;

                pos_change?.Invoke(old_pos, value);
            }
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
    
    // 디버그용
    // 숫자맵을 SlotData 맵으로 변환
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

    public IEnumerator LerpMove(Vector3 old_pos, Vector3 new_pos, Transform transform, float duration)
    {
        float currentTime = 0f;

        transform.position = old_pos;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;

            // 현재 경과 시간을 전체 시간으로 나누어 0~1 사이의 비율(t)을 만듭니다.
            float t = currentTime / duration;

            transform.position = Vector3.Lerp(old_pos, new_pos, t);

            yield return null;
        }

        transform.position = new_pos;
    }
}
