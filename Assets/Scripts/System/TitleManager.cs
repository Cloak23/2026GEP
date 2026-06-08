using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
