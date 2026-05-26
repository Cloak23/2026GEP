using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GoldUIUpdate : MonoBehaviour
{
    DataManager data;
    GameManager game_manager;
    TextMeshProUGUI gold_text;
    void Start()
    {
        data = DataManager.Instance;
        game_manager = GameManager.Instance;
        
        gold_text = GetComponent<TextMeshProUGUI>();

        data.e_gold_change.AddListener(UpdateGoldUI);
    }

    void UpdateGoldUI(int old_gold, int new_gold)
    {
        gold_text.text = "Gold : " + data.PlayerGold.ToString("D3");
    }
}
