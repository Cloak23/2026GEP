using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            SceneManager.UnloadSceneAsync("Roulette");
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
