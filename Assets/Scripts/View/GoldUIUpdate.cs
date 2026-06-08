using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 2026.06.07 신원영
/// 
/// 골드 변화에 반응해서 나오는 UI 이펙트를 담당하는 스크립트.
/// 골드의 출처에 따라 UI의 이미지를 변화시킨다.
/// </summary>

public class GoldUIUpdate : MonoBehaviour
{
    DataManager data;
    GameManager game_manager;
    TextMeshProUGUI gold_text;
    public GameObject gold_effect_prefab;
    public Sprite mine_sprite;
    public Sprite roulette_sprite;
    public Vector3Int offset = new Vector3Int(1, 1, 0);

    void Start()
    {
        data = DataManager.Instance;
        game_manager = GameManager.Instance;
        
        gold_text = GetComponent<TextMeshProUGUI>();

        data.e_gold_change.AddListener(UpdateGoldUI);
        data.e_gold_change.AddListener((old_gold, new_gold) => { StartCoroutine(Gold_Earn_Effect(old_gold, new_gold)); });
    }

    void UpdateGoldUI(int old_gold, int new_gold)
    {
        gold_text.text = "Gold : " + data.PlayerGold.ToString("D3");
    }

    IEnumerator Gold_Earn_Effect(int old_gold, int new_gold)
    {
        GameObject tmp_obj = Instantiate(gold_effect_prefab, MineMapRenderer.Instance.ToRoomCell(data.PlayerPos) + offset, Quaternion.identity);
        tmp_obj.GetComponentInChildren<TextMeshProUGUI>().text = new_gold - old_gold > 0 ? "+ " + (new_gold - old_gold).ToString() + " G" : (new_gold - old_gold).ToString() + " G";
        if (data.map.GetTile(data.PlayerPos).isMine)
        {
            tmp_obj.GetComponentInChildren<Image>().sprite = mine_sprite;
        }
        else if (data.map.GetTile(data.PlayerPos).hasItem)
        {
            tmp_obj.GetComponentInChildren<Image>().sprite = roulette_sprite;
        }
        yield return new WaitForSeconds(1f);
        Destroy(tmp_obj);
    }
}
