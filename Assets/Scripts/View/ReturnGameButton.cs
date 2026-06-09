using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReturnGameButton : MonoBehaviour

{
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if(GameManager.Instance != null)
            button.onClick.AddListener(() => { GameManager.Instance.StageStart(); });
    }
}
