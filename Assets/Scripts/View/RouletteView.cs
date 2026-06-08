using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 2026.05.31 신원영
/// 룰렛의 형태를 잡는 스크립트. 
/// 룰렛의 시작 메소드가 있다.
/// 
/// 코루틴으로 룰렛을 돌릴 수 있으며, 처음에 몇 칸을 움직일지 랜덤으로 선정한 다음 그 횟수만큼
/// 움직임을 진행한다.
/// 이후 움직임이 15번 밖에 남지 않으면 처음 속도값에서 마지막 속도값까지 Lerp로 점점 비율로 느려지면서 룰렛이 멈춘다.
/// </summary>


public class RouletteView : MonoBehaviour
{
    [Header("룰렛 L->U->R->D 순으로")]
    public List<GameObject> roulette_lines;
    public GameObject highlightCursor;

    private List<RouletteSlot> rouletteSlots = new List<RouletteSlot>();

    [Header("룰렛 슬롯 프리팹")]
    public GameObject slot_prefab;

    [Header("룰렛 세팅값")]
    public float initialDelay = 0.05f;
    public float maxDelay = 0.6f;
    public int MAX_SLOTS_PER_LINE = 4;

    private bool isSpinning = false;


    public void ArrangeRoulette()
    {
        rouletteSlots.Clear();

        foreach (var line in roulette_lines)
        {
            for (int i = 0; i < MAX_SLOTS_PER_LINE; i++)
            {
                var tmp_slot = Instantiate(slot_prefab, line.transform);

                rouletteSlots.Add(tmp_slot.GetComponent<RouletteSlot>());
            }
        }

        Debug.Log("Arrange Fin : " + rouletteSlots.Count);
    }

    public void ClickSpinButton()
    {
        ArrangeRoulette();
        Debug.Log("Click : " + rouletteSlots.Count);
        if (!isSpinning && rouletteSlots.Count > 0)
        {
            int finalWinningIndex = Random.Range(0, rouletteSlots.Count);

            StartCoroutine(SpinRoulette(finalWinningIndex));
        }
    }

    private IEnumerator SpinRoulette(int targetIndex)
    {
        isSpinning = true;

        // 최소 바퀴
        int minLaps = Random.Range(2, 5);
        int totalSteps = (rouletteSlots.Count * minLaps) + targetIndex + 1;

        int currentStep = 0;
        float currentDelay = initialDelay;

        while (currentStep < totalSteps)
        {
            int activeSlotIndex = currentStep % rouletteSlots.Count;
            Debug.Log(currentStep);

            if (highlightCursor != null && rouletteSlots[activeSlotIndex] != null)
            {
                highlightCursor.transform.position = rouletteSlots[activeSlotIndex].transform.position;
            }

            int remainingSteps = totalSteps - currentStep;
            if (remainingSteps <= 15)
            {
                float progress = (15f - remainingSteps) / 15f;
                currentDelay = Mathf.Lerp(initialDelay, maxDelay, progress);
            }

            currentStep++;
            yield return new WaitForSeconds(currentDelay);
        }

        isSpinning = false;
        OnRouletteFinished(targetIndex);
    }
    private void OnRouletteFinished(int winningIndex)
    {
        RouletteSlot wonSlot = rouletteSlots[winningIndex];

        wonSlot.OnSelected();
    }
}
