using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance.e_game_over.Invoke();
    }
}
