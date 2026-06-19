using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Vector2 movement;
    private Animator animator;

    void Start()
    {
        DataManager.Instance.e_pos_change.AddListener((a, b) => UpdateSprite(a, b));
        animator = GetComponent<Animator>();
    }


    void UpdateSprite(Vector2 old_pos, Vector2 new_pos)
    {
        movement = new_pos - old_pos;
        
        animator.SetInteger("Horizontal", (int)(movement.x));
        animator.SetInteger("Vertical", (int)(movement.y));
    }

}