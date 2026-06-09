using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


/// <summary>
/// 2026.05.26 신원영
/// 게임 전체의 진행 관련 메소드를 모아둔 스크립트 (수정본)
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
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainGame")
        {
            Debug.Log("[GameManager] 씬 로드 감지: 참조 재연결 시작");

            // 1. 현재 씬에 있는 새로운 제너레이터를 찾아서 연결
            m_Generator = FindObjectOfType<MineMapGenerator>();
            m_Renderer = MineMapRenderer.Instance;

            // 2. 이제 맵 생성 시작
            StageStart();
        }
    }

    private IEnumerator DelayedStageStart()
    {
        // 이 짧은 시간 동안 유니티는 씬의 모든 오브젝트(Tilemap 포함)를 확실하게 초기화합니다.
        yield return null;

        m_Generator = FindObjectOfType<MineMapGenerator>();
        m_Renderer = MineMapRenderer.Instance;

        StageStart();
    }

    private void Start()
    {
        Debug.Log("Start 실행됨!");
        InitializeGame();
    }

    private void InitializeGame()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log("[GameManager] 현재 씬 확인: " + currentScene);

        if (currentScene == "MainGame")
        {
            m_Generator = FindObjectOfType<MineMapGenerator>();
            m_Renderer = MineMapRenderer.Instance;

            if (m_Generator != null)
            {
                Debug.Log("[GameManager] MainGame 씬 감지! 맵 생성을 시작합니다.");
                StageStart();
            }
            else
            {
                Debug.LogError("[GameManager] MainGame인데 제너레이터를 못 찾았습니다.");
            }
        }
    }

    public void EndStage()
    {
        e_stage_end.Invoke();
        SceneManager.LoadSceneAsync("Goal", LoadSceneMode.Additive);
    }

    public void StageStart()
    {
        Debug.Log("[GameManager] StageStart 실행됨"); // 이게 찍히는지 확인
        // 생성기가 연결되어 있는지 확인 후 실행
        if (m_Generator != null)
        {
            m_Generator.width = 16;
            m_Generator.height = 16;
            m_Generator.floorIndex = stage_index++;
            m_Renderer.GenerateAndRender();
            StartCoroutine(GeneratorWait());
        }

        else
        {
            Debug.LogError("[GameManager] m_Generator가 null입니다!"); // 이게 찍히면 범인 확정
        }

    }

    IEnumerator GeneratorWait()
    {
        while (true)
        {
            yield return null;
            if (m_Generator != null && m_Generator.LastGeneratedMap.floorIndex == stage_index - 1)
            {
                e_stage_start?.Invoke();
                yield break;
            }
        }
    }
}