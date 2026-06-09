using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 2026.06.07 신원영
/// RouletteView 외의 것들을 이용하려는 스크립트.
/// 선택지 버튼을 관리하여 룰렛을 시작하는 것과 안전한 선택지로 10G를 받아가도록 한다.
/// </summary>

public class RouletteManager : MonoBehaviour
{
    public Button safe_button;
    public Button roulette_button;
    public GameObject roulette;
    public RouletteView roulette_script;
    public int SAFE_GOLD = 10;

    private void Start()
    {
        safe_button.onClick.RemoveAllListeners();
        roulette_button.onClick.RemoveAllListeners();

        safe_button.onClick.AddListener(SafeReward);
        //roulette_button.onClick.AddListener(RouletteStart);
        roulette_button.onClick.AddListener(() => StartCoroutine(RouletteSequenceRoutine()));
    }

    void SafeReward()
    {
        DataManager.Instance.PlayerGold += SAFE_GOLD;
        DataManager.Instance.e_roulette_end.Invoke();
        SceneManager.UnloadSceneAsync("Roulette");
    }
    /*void RouletteStart()
    {
        safe_button.gameObject.SetActive(false);
        roulette_button.gameObject.SetActive(false);
        roulette.SetActive(true);
        roulette_script.ClickSpinButton();
    }*/

    private IEnumerator RouletteSequenceRoutine()
    {
        safe_button.gameObject.SetActive(false);
        roulette_button.gameObject.SetActive(false);
        roulette.SetActive(true);

        int totalSpins = 1 + DataManager.Instance.current_roulette_bonus_count;

        for (int i = 0; i < totalSpins; i++)
        {
            // 1. 여기서 슬롯을 생성 (루프 내에서 1회씩)
            roulette_script.ArrangeRoulette();

            // 2. 룰렛 회전 (완료까지 기다림)
            int targetIndex = Random.Range(0, 16); // 슬롯 개수만큼 타겟 설정
            yield return StartCoroutine(roulette_script.SpinRoulette(targetIndex));

            // 3. 결과 반영 후 잠깐 대기 (결과 확인 시간)
            yield return new WaitForSeconds(0.5f);
        }

        DataManager.Instance.e_roulette_end.Invoke();
        SceneManager.UnloadSceneAsync("Roulette");
    }

}
