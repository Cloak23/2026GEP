using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ShopUIManager : MonoBehaviour
{
    //public int currentlife = 100; //죽은 횟수 누적되어야함
    public TMP_Text lifetext;

    public Color activelevel = new Color(1f, 0.8f, 0f);
    public Color levelinactive = Color.white;

    [System.Serializable]
    public class Items { 
        public int currentlevel = 0;
        public int[] upgradecosts;

        public Button lvbutton;
        public TMP_Text needLife;
        public SpriteRenderer[] levelIndicate;
    }
    //item들 추가 쉽도록 class화

    public Items revival;
    public Items opentile;
    public Items AIrequest;

    private void Start() {
        revival.currentlevel = DataManager.Instance.level_revival;
        opentile.currentlevel = DataManager.Instance.level_opentile;
        AIrequest.currentlevel = DataManager.Instance.level_AIrequest;


        revival.lvbutton.onClick.AddListener(() => TryUpgrade(revival));
        opentile.lvbutton.onClick.AddListener(() => TryUpgrade(opentile));
        AIrequest.lvbutton.onClick.AddListener(() => TryUpgrade(AIrequest));

        UpdateAllUI();
    }


    private void TryUpgrade(Items item)
    {
        if (item.currentlevel >= item.levelIndicate.Length || item.currentlevel >= item.upgradecosts.Length)
        {
            return;
        }

        int cost = item.upgradecosts[item.currentlevel];

        if (DataManager.Instance.PlayerLife >= cost)
        {
            DataManager.Instance.PlayerLife -= cost; 
            item.currentlevel++;

            if (item == revival) DataManager.Instance.level_revival = item.currentlevel;
            else if (item == opentile) DataManager.Instance.level_opentile = item.currentlevel;
            else if (item == AIrequest) DataManager.Instance.level_AIrequest = item.currentlevel;


            UpdateAllUI(); 
        }
    }

    private void UpdateAllUI()
    {
        if (lifetext != null)
        {
            lifetext.text = "LIFE : " + DataManager.Instance.PlayerLife.ToString();
        }

        RefreshItemUI(revival);
        RefreshItemUI(opentile);
        RefreshItemUI(AIrequest);
    }

    private void RefreshItemUI(Items item)
    {
        for (int i = 0; i < item.levelIndicate.Length; i++)
        {
            if (i < item.currentlevel)
            {
                item.levelIndicate[i].color = activelevel;
            }
            else
            {
                item.levelIndicate[i].color = levelinactive;
            }
        }

        if (item.currentlevel < item.upgradecosts.Length && item.currentlevel < item.levelIndicate.Length)
        {
            int nextCost = item.upgradecosts[item.currentlevel];
            item.needLife.text = "NEED LIFE : " + nextCost;

            item.lvbutton.interactable = (DataManager.Instance.PlayerLife >= nextCost);
        }
        else
        {
            item.needLife.text = "MAX LEVEL";
            item.lvbutton.interactable = false; 
        }
    }

    public void ReturnToGame()
    {
        SceneManager.LoadScene("RandomMapGenTest");
        Debug.Log(" 다시 게임으로 입장 합니다!");
    }


}
