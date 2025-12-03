using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int currentFloor = 0;   // 다음에 선택할 층
    public int maxFloors = 5;      // 총 전투 수

    // ✅ 추가: 맵 시드
    public int mapSeed = -1;
    public bool mapGenerated = false;

    // 층별로 어떤 노드를 선택했는지 (-1=아직)
    public List<int> chosenNodePerFloor = new List<int>();

    // 방문한 노드 집합: "floor:index" 문자열로 키를 만듭니다.
    private HashSet<string> visitedNodeKeys = new HashSet<string>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // ✅ 맵 초기화 (MainMenu → MapScene 시작 시 호출)
    public void InitializeNewRun()
    {
        mapSeed = Random.Range(0, int.MaxValue);
        mapGenerated = false;
        currentFloor = 0;
    }
    public void EnsureInit()
    {
        if (chosenNodePerFloor.Count != maxFloors)
        {
            chosenNodePerFloor.Clear();
            for (int i = 0; i < maxFloors; i++) chosenNodePerFloor.Add(-1);
        }
    }

    public void AdvanceFloor()
    {
        currentFloor = Mathf.Min(currentFloor + 1, maxFloors);
    }

    public bool IsRunFinished()
    {
        return currentFloor >= maxFloors;
    }

    public void SaveChoice(int floor, int indexInFloor)
    {
        EnsureInit();
        if (floor >= 0 && floor < chosenNodePerFloor.Count)
        {
            chosenNodePerFloor[floor] = indexInFloor;
        }
        MarkVisited(floor, indexInFloor);
    }

    public int GetChosenNode(int floor)
    {
        EnsureInit();
        if (floor < 0 || floor >= chosenNodePerFloor.Count) return -1;
        return chosenNodePerFloor[floor];
    }

    public void MarkVisited(int floor, int indexInFloor)
    {
        visitedNodeKeys.Add($"{floor}:{indexInFloor}");
    }

    public bool IsVisited(int floor, int indexInFloor)
    {
        return visitedNodeKeys.Contains($"{floor}:{indexInFloor}");
    }
}
