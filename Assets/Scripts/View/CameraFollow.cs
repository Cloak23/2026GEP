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
    public float speed = 5f;
    public Vector3 offset;

    private DataManager data;
    private Camera _camera;

    private Vector3 target_pos;

    private void Start()
    {
        data = DataManager.Instance;
        _camera = Camera.main;
        target_pos = _camera.transform.position;
        data.pos_change.AddListener(UpdateCameraPos);
    }

    private void OnDestroy()
    {
        data.pos_change.RemoveListener(UpdateCameraPos);
    }

    public void UpdateCameraPos(Vector2Int old_pos, Vector2Int new_pos)
    {
        Vector3 cell_center_world = tilemap.GetCellCenterWorld(new Vector3Int(new_pos.x, new_pos.y, 0));

        target_pos = cell_center_world + offset;
    }
    private void LateUpdate()
    {
        _camera.transform.position = Vector3.Lerp(_camera.transform.position, target_pos, speed * Time.deltaTime);
    }
}
