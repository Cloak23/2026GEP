using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TileRevealByTouch : MonoBehaviour
{
    [Header("타일을 가리는 덮개 오브젝트")]
    public GameObject coverSquare;

    public bool isRevealed = false;
    private TileData myLogicalTile;

    //  타일 생성 시 바로 열리는 것 방지
    private bool isReady = false;

    public void SetupLogicalTile(TileData logicalTile)
    {
        myLogicalTile = logicalTile;

        if (myLogicalTile != null && (myLogicalTile.isStart || myLogicalTile.isRevealed))
        {
            RevealTile();
        }
    }

    private void Start()
    {
        if (!isRevealed && coverSquare != null)
        {
            coverSquare.SetActive(true);
        }

        // 타일이 생성되고 딱 0.1초 뒤에 실행
        Invoke("EnableTouch", 0.1f);
    }

    private void EnableTouch()
    {
        isReady = true; 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isReady) return;

        if (collision.CompareTag("Player"))
        {
            if (!isRevealed)
            {
                RevealTile();
            }
        }
    }

    public void RevealTile()
    {
        isRevealed = true;
        ActiveTile();

        if (coverSquare != null)
        {
            coverSquare.SetActive(false);
        }

        if (myLogicalTile != null)
        {
            myLogicalTile.isRevealed = true;
        }
    }

    public void ActiveTile()
    {
        if (myLogicalTile.isMine)
        {
            
        }
        else if (myLogicalTile.isGoal)
        {
            SceneManager.LoadSceneAsync("Goal", LoadSceneMode.Additive);
        }
        else if (myLogicalTile.hasItem)
        {
            DataManager.Instance.e_roulette_start.Invoke();
            SceneManager.LoadSceneAsync("Roulette", LoadSceneMode.Additive);
        }
    }
}