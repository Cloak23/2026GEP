using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 2026.06.07 신원영
/// 
/// 타이틀 버튼을 관리하는 스크립트.
/// </summary>

public class TitleManager : MonoBehaviour
{
    public Button start_button;
    public Button exit_button;
    void Start()
    {
        start_button.onClick.AddListener(GameStart);
        exit_button.onClick.AddListener(GameExit);
    }

    void GameStart()
    {
        SceneManager.LoadScene("MainGame");
    }
    void GameExit()
    {
        Application.Quit();
    }
}
