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

    public void ClearStageIndex()
    {
        stage_index = 0;
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
            m_Generator = FindObjectOfType<MineMapGenerator>();
            m_Renderer = MineMapRenderer.Instance;

            StageStart();
        }
    }

    private void Start()
    {
        Invoke("InitializeGame", 0.1f);
        e_game_over.AddListener(ClearStageIndex);
        e_stage_start.AddListener(ResetPlayerFlags);
    }

    private void InitializeGame()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "MainGame")
        {
            m_Generator = FindObjectOfType<MineMapGenerator>();
            m_Renderer = MineMapRenderer.Instance;

            if (m_Generator != null)
            {
                StageStart();
            }
        }
    }

    public void EndStage()
    {
        stage_index++;
        e_stage_end.Invoke();
        SceneManager.LoadSceneAsync("Goal", LoadSceneMode.Additive);
    }

    public void StageStart()
    {
        if (m_Generator != null)
        {
            m_Generator.width = 16;
            m_Generator.height = 16;
            m_Generator.floorIndex = stage_index;
            m_Generator.ClearLastGeneratedMap();
            m_Renderer.GenerateAndRender();
            StartCoroutine(GeneratorWait());
        }

    }

    IEnumerator GeneratorWait()
    {
        while (true)
        {
            yield return null;
            if (m_Generator != null && m_Generator.LastGeneratedMap != null)
            {
                e_stage_start?.Invoke();
                yield break;
            }
        }
    }


    private void ResetPlayerFlags()
    {
        Move playerMove = FindObjectOfType<Move>();
        if (playerMove != null)
        {
            playerMove.ClearAllFlags();
        }
    }



}