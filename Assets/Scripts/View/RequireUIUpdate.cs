using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 2026.06.09 신원영
/// 인게임에서 할당량 UI를 업데이트하는 스크립트
/// </summary>

public class RequireUIUpdate : MonoBehaviour
{
    [Header("외부 UI 연결")]
    public TextMeshProUGUI stage_index_text;

    private TextMeshProUGUI require_text;

    private DataManager data;
    void Start()
    {
        data = DataManager.Instance;
        require_text = GetComponent<TextMeshProUGUI>();
        GameManager.Instance.e_stage_start.AddListener(UpdateRequireUI);
    }
    private void OnDisable()
    {
        GameManager.Instance.e_stage_start.RemoveListener(UpdateRequireUI);
    }

    void UpdateRequireUI()
    {
        stage_index_text.text = "Stage : " + (GameManager.Instance.stage_index + 1);
        require_text.text = "Require : " + data.REQUIRE_GOLD_LIST[GameManager.Instance.stage_index];
    }
}
