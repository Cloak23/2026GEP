using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RouletteView : MonoBehaviour
{
    [Header("·ê·¿ L->U->R->D ¼øÀ¸·Î")]
    public List<GameObject> roulette_lines;
    public GameObject highlightCursor;

    private List<RouletteSlot> rouletteSlots = new List<RouletteSlot>();

    [Header("·ê·¿ ½½·Ô ÇÁ¸®ÆÕ")]
    public GameObject slot_prefab;

    [Header("·ê·¿ ¼¼ÆÃ°ª")]
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

        // ÃÖ¼Ò ¹ÙÄû
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
