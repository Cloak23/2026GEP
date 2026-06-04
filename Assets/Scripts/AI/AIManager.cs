using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class AIManager : MonoBehaviour
{
    [Header("통신 설정")]
    [SerializeField] private string gasURL = "https://script.google.com/macros/s/AKfycbylrtr2dxEZeMNNqgOW975AA0g2jR1V-ZrwlSRHafO8Ue55QZKH8vlwzKoVm4_ikYcQ/exec";
    [SerializeField] private MineMapGenerator mapGenerator;

    [Header("UI 요소 연결")]
    public GameObject aiWindowPanel;     
    public TMP_InputField queryInputField;
    public TMP_Text responseText;
    public Button askButton;              // 질문 전송 버튼
    public Button closeButton;            // 창 닫기 버튼

    private bool isWaitingForAi = false;
    private float lastRequestTime = -999f;

    public static bool windowOpen = false;

    [SerializeField] private float cooldownDuration = 10f;

    // 구조체 정의 
    [System.Serializable] public class RevealedTileParam { public int x; public int y; public bool isRevealed; public bool isFlagged; public int adjacentMines; }
    [System.Serializable] public class ClosedCandidateParam { public int x; public int y; public bool isRevealed; public bool isFlagged; }
    [System.Serializable] public class CompressedMapPacket { public int mapWidth; public int mapHeight; public int currentFloor; public Vector2Int startPosition; public List<RevealedTileParam> revealedClues; public List<ClosedCandidateParam> closedCandidates; }
    [System.Serializable] public class GasEnvelopeDto { public CompressedMapPacket mapData; public string playerQuery; }



    private void Start()
    {
        // 버튼 연동
        if (askButton != null) askButton.onClick.AddListener(RequestAiHint);
        if (closeButton != null) closeButton.onClick.AddListener(CloseAiWindow);

        if (queryInputField != null)
        {
            queryInputField.text = "안전한 곳으로 이동하려면 어떻게 해야할까?";
        }

        if (responseText != null)
        {
            responseText.text = "AI가 당신의 질문을 기다리고 있습니다.";
        }

    }

    // 질문하기 버튼
    public void OpenAiWindow()
    {
        if (aiWindowPanel != null)
        {
            aiWindowPanel.SetActive(true);
            windowOpen = true; 
        }
    }

    // 창 닫기 버튼
    public void CloseAiWindow()
    {
        if (aiWindowPanel != null)
        {
            aiWindowPanel.SetActive(false);
            windowOpen = false; 
        }
    }

    // 맵 데이터 수집 함수 
    private CompressedMapPacket GetCompressedMapData(MineMapData currentMap)
    {
        CompressedMapPacket packet = new CompressedMapPacket
        {
            mapWidth = currentMap.width,
            mapHeight = currentMap.height,
            currentFloor = currentMap.floorIndex,
            startPosition = DataManager.Instance.PlayerPos,
            revealedClues = new List<RevealedTileParam>(),
            closedCandidates = new List<ClosedCandidateParam>()
        };

        HashSet<Vector2Int> addedCandidates = new HashSet<Vector2Int>();
        Vector2Int[] directions = { new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1) };

        for (int x = 0; x < currentMap.width; x++)
        {
            for (int y = 0; y < currentMap.height; y++)
            {
                TileData tile = currentMap.tiles[x, y];
                if (tile == null) continue;

                if (tile.isRevealed)
                {
                    packet.revealedClues.Add(new RevealedTileParam { x = x, y = y, isRevealed = true, isFlagged = false, adjacentMines = tile.adjacentMineCount });
                    foreach (Vector2Int dir in directions)
                    {
                        Vector2Int nextPos = new Vector2Int(x + dir.x, y + dir.y);
                        if (currentMap.IsInBounds(nextPos) && !addedCandidates.Contains(nextPos))
                        {
                            TileData nextTile = currentMap.GetTile(nextPos);
                            if (nextTile != null && !nextTile.isRevealed)
                            {
                                packet.closedCandidates.Add(new ClosedCandidateParam { x = nextPos.x, y = nextPos.y, isRevealed = false, isFlagged = false });
                                addedCandidates.Add(nextPos);
                            }
                        }
                    }
                }
            }
        }
        return packet;
    }

    // AI 요청 시작 함수
    public void RequestAiHint()
    {
        if (isWaitingForAi) return;
        if (Time.time - lastRequestTime < cooldownDuration)
        {
            responseText.text = "AI가 준비 중입니다.";
            return;
        }

        MineMapData currentRuntimeMap = mapGenerator.LastGeneratedMap;
        if (currentRuntimeMap == null || currentRuntimeMap.tiles == null)
        {
            responseText.text = "에러: 맵 데이터가 존재하지 않습니다.";
            return;
        }

        // 입력 박스의 텍스트를 가져와서 전송합니다.
        string currentQuery = queryInputField != null ? queryInputField.text : "도와줘";

        CompressedMapPacket compressedData = GetCompressedMapData(currentRuntimeMap);
        StartCoroutine(SendToGasCoroutine(compressedData, currentQuery));
    }

    private IEnumerator SendToGasCoroutine(CompressedMapPacket mapPacket, string query)
    {
        isWaitingForAi = true;
        lastRequestTime = Time.time;

        if (askButton != null) askButton.interactable = false; // 연타 방지
        responseText.text = "AI가 연산을 진행중입니다.";

        GasEnvelopeDto envelope = new GasEnvelopeDto { mapData = mapPacket, playerQuery = query };
        string requestJson = JsonUtility.ToJson(envelope);

        using (UnityWebRequest www = new UnityWebRequest(gasURL, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(requestJson);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.timeout = 60;

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                responseText.text = www.downloadHandler.text;
            }
            else
            {
                responseText.text = "AI와의 연동이 끊어졌습니다: " + www.error;
            }
        }

        isWaitingForAi = false;
        if (askButton != null) askButton.interactable = true; // 버튼 다시 활성화
    }
}