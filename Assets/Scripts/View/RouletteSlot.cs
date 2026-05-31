using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RouletteSlot : MonoBehaviour
{
    [Header("외부 컴포넌트 연결")]
    public TextMeshProUGUI slot_text;

    public int gold_amount = 10;


    DataManager data;
    private void Start()
    {
        data = DataManager.Instance;
        RandomizeGold();
        SetText();
    }

    public void OnSelected()
    {
        if(data != null)
        {
            data.PlayerGold += gold_amount;
        }
        else
        {
            Debug.Log("No data manager : Gold Plus " +  gold_amount);
        }
    }

    void RandomizeGold()
    {
        gold_amount = Random.Range(-50, 50);
    }

    void SetText()
    {
        slot_text.text = gold_amount + " G";
    }
}
