using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemExplain : MonoBehaviour
{
    [TextArea(3, 5)]
    public string description = "아이템 설명을 여기에 적으세요.";

    public GameObject tooltipPanel;  // 캔버스 안의 툴팁 패널
    public TMP_Text tooltipText;     // 툴팁 텍스트

    public Vector3 offset = new Vector3(50f, -50f, 0f);

    private void Start()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }



    private void OnMouseEnter()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
            tooltipText.text = description;
            Vector3 screenPos = Camera.main.WorldToScreenPoint(this.transform.position);
            tooltipPanel.transform.position = screenPos + offset;
        }
    }

    private void OnMouseExit()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }
}
