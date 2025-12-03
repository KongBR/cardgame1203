using UnityEngine;
using UnityEngine.UI;

public class MapNode : MonoBehaviour
{
    public int floor;          // 이 노드의 층
    public int indexInFloor;   // 층 내 인덱스
    public Button button;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        // 방문했던 노드면 비활성화
        if (GameManager.instance != null && GameManager.instance.IsVisited(floor, indexInFloor))
        {
            SetInteractable(false);
        }
    }

    private void Start()
    {
        button.onClick.AddListener(OnNodeClicked);
    }

    void OnNodeClicked()
    {
        // MapController가 현재 층/연결 여부로 인터랙션 제어함.
        MapController.instance.EnterNode(this);
    }

    public void SetInteractable(bool canClick)
    {
        if (button != null) button.interactable = canClick;
        // 필요시 색상도 변경
        // var img = GetComponent<Image>(); if (img) img.color = canClick ? enabledColor : disabledColor;
    }
}

