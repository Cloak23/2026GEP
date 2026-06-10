using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public SpriteRenderer spriteRenderer;
    public Sprite spriteFront;
    public Sprite spriteBack;
    public Sprite spriteSide;

    private Vector2 movement;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // WASD 입력 받기
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        UpdateSprite();
    }


    void UpdateSprite()
    {
        if (movement.y > 0) // 위쪽(W)
        {
            spriteRenderer.sprite = spriteBack;
            spriteRenderer.flipX = false;
        }
        else if (movement.y < 0) // 아래쪽(S)
        {
            spriteRenderer.sprite = spriteFront;
            spriteRenderer.flipX = false;
        }
        else if (movement.x > 0) // 오른쪽(D)
        {
            spriteRenderer.sprite = spriteSide;
            spriteRenderer.flipX = false; 
        }
        else if (movement.x < 0) // 왼쪽(A)
        {
            spriteRenderer.sprite = spriteSide;
            spriteRenderer.flipX = true; 
        }
    }
}