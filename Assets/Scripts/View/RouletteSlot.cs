using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 2026.05.31 신원영
/// 룰렛의 각 슬롯에 랜덤한 값을 부여하는 스크립트. 
/// 선택되면 골드를 변화시키고
/// RouletteView를 안거친 채로 바로 나간다.
/// </summary>


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
            data.e_roulette_end.Invoke();
            //SceneManager.UnloadSceneAsync("Roulette");
        }
        else
        {
            Debug.Log("No data manager : Gold Plus " +  gold_amount);
        }
    }

    void RandomizeGold()
    {
        gold_amount = Random.Range(data.ROULETTE_MIN_GOLD, data.ROULETTE_MAX_GOLD);
    }

    void SetText()
    {
        slot_text.text = gold_amount + " G";
    }
}
