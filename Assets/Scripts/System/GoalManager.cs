using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoalManager : MonoBehaviour
{
    [Header("UI 연결")]
    public Button next_button;
    public TextMeshProUGUI now_gold_text;
    public TextMeshProUGUI require_gold_text;
    public TextMeshProUGUI result_text;


    private DataManager data;
    private GameManager m_game;
    private int require_gold;
    private int idx;

    public AudioClip SuccessSound;
    public AudioClip FailSound;

    private void Start()
    {
        data = DataManager.Instance;
        m_game = GameManager.Instance;
        idx = m_game.stage_index - 1;

        now_gold_text.text = data.PlayerGold.ToString();

        if (data.REQUIRE_GOLD_LIST.Count < idx)
        {
            Debug.LogError("스테이지 인덱스 오류");
            return;
        }
        require_gold = data.REQUIRE_GOLD_LIST[idx];

        next_button.onClick.RemoveAllListeners();

        require_gold_text.text = require_gold.ToString();
        if (require_gold <= data.PlayerGold)
        {
            result_text.text = "할당량 무사히 통과";
            now_gold_text.color = Color.green;
            next_button.onClick.AddListener(Pass);
            BGMManager.Instance.PlaySFX(SuccessSound);
        }
        else
        {
            result_text.text = "할당량 통과 실패";
            now_gold_text.color = Color.red;
            next_button.onClick.AddListener(Fail);
            BGMManager.Instance.PlaySFX(FailSound);
        }
    }


    void Pass()
    {
        data.PlayerGold -= require_gold;
        data.PlayerLife += 5;
        m_game.StageStart();
        SceneManager.UnloadSceneAsync("Goal");
    }

    void Fail()
    {
        data.PlayerLife += 1;
        SceneManager.UnloadSceneAsync("Goal");
        SceneManager.LoadScene("GameOver");
    }
}
