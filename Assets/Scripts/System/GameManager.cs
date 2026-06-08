using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// 2026.05.26 신원영
/// 게임 전체의 진행 관련 메소드를 모아둔 스크립트
/// </summary>


public class GameManager : MonoBehaviour
{
    [Header("외부 컴포넌트 연결")]
    public MineMapGenerator m_Generator;

    public static GameManager Instance;

    public UnityEvent e_stage_start;
    public UnityEvent e_stage_end;
    public UnityEvent e_game_over;

    private DataManager data;
    private MineMapRenderer m_Renderer;

    public int stage_index { get; private set; } = 0;

    private void Awake()
    {
        if(Instance == null)
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
        data = DataManager.Instance;
        m_Renderer = MineMapRenderer.Instance;
        StageStart();
    }


    // 2026.05.26 스테이지 끝나는 함수는 분리했는데, 나중에 할당량 체크할때 수정해야함
    // 2026.06.09 업데이트 완
    public void EndStage()
    {
        // 혹시 몰라서 만든 이벤트
        e_stage_end.Invoke();

        SceneManager.LoadSceneAsync("Goal", LoadSceneMode.Additive);
    }


    public void StageStart()
    {
        m_Generator.width = 16;
        m_Generator.height = 16;
        m_Generator.floorIndex = stage_index++;
        m_Renderer.GenerateAndRender();
        StartCoroutine(GeneratorWait());
    }

    IEnumerator GeneratorWait()
    {
        while (true)
        {
            yield return null;
            if (m_Generator.LastGeneratedMap.floorIndex == stage_index - 1)
            {
                e_stage_start?.Invoke();
                yield break;
            }
        }
    }
}
