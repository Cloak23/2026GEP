using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class GameOverShop : MonoBehaviour { 

    [Header("게임 오버 UI")]
    public GameObject gameOverPanel;

    private DataManager data;

    private void Start()
    {
        data = DataManager.Instance;
    }

    public void GoToShop()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        SceneManager.LoadScene("Item");

        Debug.Log(" 상점으로 이동합니다.");
    }

    public void GoToTitle()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        SceneManager.LoadScene("Title");

        Debug.Log(" 메인 메뉴로 이동합니다.");
    }


    public void GameOver()
    {
        Debug.Log("게임 오버.");
        data.PlayerLife += 1;

        // 3. 준비해 둔 게임 오버 UI 띄우기
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }
}