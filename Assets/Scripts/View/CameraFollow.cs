using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 2026.05.25
/// 신원영
/// 
/// 카메라가 플레이어를 따라가는 스크립트
/// </summary>

public class CameraFollow : MonoBehaviour
{
    [Header("외부 컴포넌트 연결")]
    public Tilemap tilemap;

    [Header("카메라 스탯")]
    public float speed = 2f;
    public Vector3 offset = new Vector3(0, 0, -10);

    private DataManager data;
    private Camera _camera;

    private Vector3 target_pos;
    private MineMapRenderer m_Renderer;

    private void Start()
    {
        data = DataManager.Instance;
        m_Renderer = MineMapRenderer.Instance;
        _camera = Camera.main;
        target_pos = _camera.transform.position;
        data.e_pos_change.AddListener(UpdateCameraPos);
        UpdateCameraPos(data.PlayerPos, data.PlayerPos);
    }

    private void OnDestroy()
    {
        data.e_pos_change.RemoveListener(UpdateCameraPos);
    }

    public void UpdateCameraPos(Vector2Int old_pos, Vector2Int new_pos)
    {
        Vector3 cell_center_world = tilemap.GetCellCenterWorld(m_Renderer.ToRoomCell(new_pos));

        target_pos = cell_center_world + offset;
    }
    private void LateUpdate()
    {
        _camera.transform.position = Vector3.Lerp(_camera.transform.position, target_pos, speed * Time.deltaTime);
    }
}
