using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevivalEffect : MonoBehaviour
{
    public GameObject revival_effect_prefab; // 부활 이펙트 프리팹 (텍스트나 이미지 포함)
    public Vector3 offset = new Vector3(0.5f, 0.5f, 0f);

    void Start()
    {
        // DataManager의 부활 이벤트를 구독
        DataManager.Instance.e_revival.AddListener(PlayRevivalEffect);
    }

    void PlayRevivalEffect()
    {
        StartCoroutine(Revival_Effect_Routine());
    }

    IEnumerator Revival_Effect_Routine()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 10f);
        Vector3 spawnPos = Camera.main.ScreenToWorldPoint(screenCenter);

        spawnPos.z = 0f;

        GameObject tmp_obj = Instantiate(revival_effect_prefab, spawnPos, Quaternion.identity);

        yield return new WaitForSeconds(1.5f);
        Destroy(tmp_obj);
    }

    private void OnDestroy()
    {
        DataManager.Instance.e_revival.RemoveListener(PlayRevivalEffect);
    }
}
