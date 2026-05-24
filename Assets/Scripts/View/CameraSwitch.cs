using System.Collections;
using UnityEngine;

/// <summary>
/// 2026.05.25
/// 신원영
/// 
/// tab을 눌러 카메라를 줌인-줌아웃으로 변경하는 스크립트
/// </summary>


public class CameraSwitch : MonoBehaviour
{
    [SerializeField]
    private float zoom_in_size = 1.8f;
    [SerializeField]
    private float zoom_out_size = 10f;
    [SerializeField]
    private float animation_duration = 0.5f;

    private bool is_zoomed_in = true;

    private Camera main_camera;

    private void OnEnable()
    {
        main_camera = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            is_zoomed_in = !is_zoomed_in;
            StartCoroutine(CameraZoom(is_zoomed_in));
        }
    }

    IEnumerator CameraZoom(bool zoom_in)    
    {
        float currentTime = 0f;
        float start_size, end_size;

        if (zoom_in)
        {
            start_size = zoom_out_size;
            end_size = zoom_in_size;
        }
        else
        {
            start_size = zoom_in_size;
            end_size = zoom_out_size;
        }

        main_camera.orthographicSize = start_size;

        while (currentTime < animation_duration)
        {
            currentTime += Time.deltaTime;

            // 현재 경과 시간을 전체 시간으로 나누어 0~1 사이의 비율(t)을 만듭니다.
            float t = currentTime / animation_duration;

            main_camera.orthographicSize = Mathf.Lerp(start_size, end_size, t);

            yield return null;
        }

        main_camera.orthographicSize = end_size;
    }
}
