using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapController : MonoBehaviour
{
    public static MapController instance;

    [Header("Map Settings")]
    public int floors = 5;                 // 총 층(전투) 수
    public int minNodesPerFloor = 3;       // 층당 최소 노드 수
    public int maxNodesPerFloor = 5;       // 층당 최대 노드 수
    public float xSpacing = 200f;          // 가로 간격
    public float ySpacing = 250f;          // 세로 간격(층 간 간격)

    [Header("Prefabs / References")]
    public GameObject nodePrefab;          // 버튼+MapNode 스크립트가 붙은 프리팹
    public GameObject linePrefab;          // Image로 만든 얇은 막대 프리팹
    public RectTransform mapParent;        // 노드/라인을 담을 부모(캔버스 하위)

    private readonly List<List<MapNode>> nodesByFloor = new List<List<MapNode>>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // 0) 필수 참조 누락 방지
        if (mapParent == null) { Debug.LogError("[MapController] mapParent 연결 필요"); return; }
        if (nodePrefab == null) { Debug.LogError("[MapController] nodePrefab 연결 필요"); return; }
        if (linePrefab == null) { Debug.LogError("[MapController] linePrefab 연결 필요"); return; }

        // 1) GameManager 보장
        if (GameManager.instance == null)
        {
            var go = new GameObject("GameManager");
            go.AddComponent<GameManager>(); // Awake에서 instance + DontDestroyOnLoad
        }
        var gm = GameManager.instance;
        gm.maxFloors = floors;
        gm.EnsureInit();

        // 2) 맵 생성 + 연결선 생성
        GenerateMap();
        DrawConnections();

        // 3) 현재 층에서 가능한 노드만 활성화
        RefreshInteractable();
    }

    // -----------------------------
    // A) 맵 생성 (층마다 노드 프리팹 생성)
    // -----------------------------
    private void GenerateMap()
    {

        // GameManager 시드 설정
        
            if (GameManager.instance.mapSeed == -1)
                GameManager.instance.mapSeed = Random.Range(0, int.MaxValue);

            Random.InitState(GameManager.instance.mapSeed);
            GameManager.instance.mapGenerated = true;
        

        // 이제부터는 같은 시드로 계속 같은 맵이 생성됨
        // (보스 클리어 또는 패배 시까지 유지)


        // 맵 초기화
        for (int i = mapParent.childCount - 1; i >= 0; i--)
            Destroy(mapParent.GetChild(i).gameObject);

        nodesByFloor.Clear();

        for (int f = 0; f < floors; f++)
        {
            // 마지막 층(보스층)은 1개의 노드로 고정
            int nodeCount = (f == floors - 1)
                ? 1
                : Mathf.Clamp(Random.Range(minNodesPerFloor, maxNodesPerFloor + 1), 1, 99);

            var floorList = new List<MapNode>(nodeCount);

            for (int i = 0; i < nodeCount; i++)
            {
                GameObject nodeObj = Instantiate(nodePrefab, mapParent);
                RectTransform rt = nodeObj.GetComponent<RectTransform>();

                float x = (i - (nodeCount - 1) * 0.5f) * xSpacing;
                float y = -f * ySpacing;
                rt.anchoredPosition = new Vector2(x, y);

                MapNode node = nodeObj.GetComponent<MapNode>();
                node.floor = f;
                node.indexInFloor = i;

                // 마지막 층 노드는 빨간색으로 표시
                if (f == floors - 1)
                {
                    var txt = nodeObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (txt != null)
                        txt.text = "BOSS";
                    UnityEngine.UI.Image img = node.GetComponent<UnityEngine.UI.Image>();
                    if (img != null)
                        img.color = Color.red;


                }

                floorList.Add(node);
            }

            nodesByFloor.Add(floorList);
        }
    }

    // ----------------------------------------
    // B) 층간 연결선 그리기 (단순 랜덤 1~2개 연결)
    // ----------------------------------------
    private void DrawConnections()
    {
        for (int f = 0; f < nodesByFloor.Count - 1; f++)
        {
            var upper = nodesByFloor[f];
            var lower = nodesByFloor[f + 1];

            // 모든 upper 노드 → 모든 lower 노드 연결
            foreach (var from in upper)
            {
                foreach (var to in lower)
                {
                    DrawLine(from.GetComponent<RectTransform>(), to.GetComponent<RectTransform>());
                }
            }
        }
    }


    private void DrawLine(RectTransform a, RectTransform b)
    {
        var lineObj = Instantiate(linePrefab, mapParent);
        var rt = lineObj.GetComponent<RectTransform>();

        Vector2 pa = a.anchoredPosition;
        Vector2 pb = b.anchoredPosition;

        Vector2 mid = (pa + pb) * 0.5f;
        rt.anchoredPosition = mid;

        float length = Vector2.Distance(pa, pb);
        rt.sizeDelta = new Vector2(length, rt.sizeDelta.y);

        Vector2 dir = pb - pa;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rt.rotation = Quaternion.Euler(0, 0, angle);

        lineObj.transform.SetAsFirstSibling();
    }

    // -----------------------------------------------------
    // C) 노드 버튼 활성화 제어 + 씬 이동(전투 진입) 처리
    // -----------------------------------------------------
    public void RefreshInteractable()
    {
        var gm = GameManager.instance;
        int totalFloors = nodesByFloor.Count;

        // 노드가 아직 없으면 리턴
        if (totalFloors == 0) return;

        // 모든 버튼 잠금
        foreach (var floorList in nodesByFloor)
            foreach (var n in floorList)
                n.SetInteractable(false);

        // 모든 층 완료 시 끝
        if (gm.IsRunFinished()) return;

        // 현재 층 범위 체크
        if (gm.currentFloor < 0 || gm.currentFloor >= totalFloors)
        {
            Debug.LogWarning($"[MapController] currentFloor {gm.currentFloor} out of range");
            return;
        }

        // 지난 층은 잠금(이미 전체 잠금했으므로 표시만 변경하고 싶다면 여기서 처리)
        for (int f = 0; f < gm.currentFloor; f++)
        {
            var list = nodesByFloor[f];
            foreach (var n in list) n.SetInteractable(false);
        }

        // 시작 층: 방문하지 않은 노드 전부 활성화
        if (gm.currentFloor == 0)
        {
            foreach (var n in nodesByFloor[0])
                if (!gm.IsVisited(0, n.indexInFloor)) n.SetInteractable(true);

            return;
        }

        // 그 외 층:
        int prev = gm.currentFloor - 1;
        int prevChosen = gm.GetChosenNode(prev);

        // 아직 이전 층에서 선택 기록이 없다면(비정상 상태) -> 현재 층 전체 활성화(방문한 곳 제외)로 폴백
        if (prevChosen < 0 || prevChosen >= nodesByFloor[prev].Count)
        {
            EnableWholeCurrentFloorIfSafe(gm.currentFloor);
            return;
        }

        // [간단 버전] 연결 테이블을 저장하지 않는 구조이므로,
        // 현재 층 전체를 후보로 활성화(이미 방문한 노드는 제외)
        EnableWholeCurrentFloorIfSafe(gm.currentFloor);
    }

    private void EnableWholeCurrentFloorIfSafe(int floor)
    {
        var gm = GameManager.instance;
        if (floor < 0 || floor >= nodesByFloor.Count) return;

        foreach (var n in nodesByFloor[floor])
        {
            if (!gm.IsVisited(floor, n.indexInFloor))
                n.SetInteractable(true);
        }
    }

    // 노드 클릭 시 호출 (MapNode에서 버튼 클릭으로 연결됨)
    public void EnterNode(MapNode node)
    {
        var gm = GameManager.instance;

        // 현재 층 노드만 입장 허용
        if (node.floor != gm.currentFloor) return;

        // 이미 방문한 노드는 금지
        if (gm.IsVisited(node.floor, node.indexInFloor))
        {
            node.SetInteractable(false);
            return;
        }

        // 선택/방문 기록 저장
        gm.SaveChoice(node.floor, node.indexInFloor);

        // 마지막 층이면 보스 배틀로 이동
        if (node.floor == 4)   // floors가 5일 때 마지막층은 4
        {
            SceneManager.LoadScene("BossBattle");
            return;
        }

        // 그 외에는 일반 배틀로 랜덤 진입
        Random.InitState(System.DateTime.Now.Millisecond + UnityEngine.Time.frameCount);
        string randomBattle = "Battle " + Random.Range(1, 4);
        SceneManager.LoadScene(randomBattle);
    }

    // 전투 끝나고 MapScene으로 복귀했을 때 호출하고 싶다면 사용
    public void OnReturnedFromBattle()
    {
        RefreshInteractable();
    }
}
