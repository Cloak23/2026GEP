using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 2026.06.09 신원영
/// 인게임에서 할당량 UI를 업데이트하는 스크립트
/// </summary>

public class RequireUIUpdate : MonoBehaviour
{
    [Header("외부 UI 연결")]
    public TextMeshProUGUI stage_index_text;
    public Image fill_image;
    public TextMeshProUGUI require_text;

    private DataManager data;
    void Start()
    {
        data = DataManager.Instance;
        GameManager.Instance.e_stage_start.AddListener(UpdateStageUI);
        data.e_gold_change.AddListener(UpdateRequireUI);
    }
    private void OnDisable()
    {
        GameManager.Instance.e_stage_start.RemoveListener(UpdateStageUI);
    }

    void UpdateStageUI()
    {
        stage_index_text.text = "" + (GameManager.Instance.stage_index + 1);
    }

    void UpdateRequireUI(int a, int b)
    {
        fill_image.fillAmount = (float)data.PlayerGold / (float)data.REQUIRE_GOLD_LIST[GameManager.Instance.stage_index];
        if(fill_image.fillAmount == 1)
        {
            require_text.color = Color.green;
        }
        else
        {
            require_text.color = Color.gray;
        }
        require_text.text = data.REQUIRE_GOLD_LIST[GameManager.Instance.stage_index].ToString();
    }

}
