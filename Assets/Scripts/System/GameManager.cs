using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [Header("외부 컴포넌트 연결")]
    public MineMapGenerator m_Generator;

    public static GameManager Instance;
    
    //수정, new Unity
    public UnityEvent e_stage_start = new UnityEvent();


    private DataManager data;
    private MineMapRenderer m_Renderer;

    private int stage_index = 1;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        data = DataManager.Instance;
        m_Renderer = FindObjectOfType<MineMapRenderer>();
        m_Generator = FindObjectOfType<MineMapGenerator>();

        if (m_Generator == null)
        {
            m_Generator = FindObjectOfType<MineMapGenerator>();
        }

        StageStart();

    }


    // 2026.05.26 스테이지 끝나는 함수는 분리했는데, 나중에 할당량 체크할때 수정해야함
    public void EndStage()
    {
        StageStart();
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
