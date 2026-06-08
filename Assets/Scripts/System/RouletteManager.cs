using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        roulette_button.onClick.AddListener(RouletteStart);
    }

    void SafeReward()
    {
        DataManager.Instance.PlayerGold += SAFE_GOLD;
        DataManager.Instance.e_roulette_end.Invoke();
        SceneManager.UnloadSceneAsync("Roulette");
    }
    void RouletteStart()
    {
        safe_button.gameObject.SetActive(false);
        roulette_button.gameObject.SetActive(false);
        roulette.SetActive(true);
        roulette_script.ClickSpinButton();
    }
}
