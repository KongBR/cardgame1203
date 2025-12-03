using System.Collections.Generic;
using UnityEngine;

public enum StatusType { Burn, Freeze, Bleed, Taunt }

public class StatusManager : MonoBehaviour
{
    private class ActiveStatus
    {
        public StatusType type;
        public int value;
        public int remainingTurns;
    }

    private List<ActiveStatus> statuses = new List<ActiveStatus>();
    private Card card;

    void Start()
    {
        card = GetComponent<Card>();
    }

    public void ApplyStatus(StatusType type, int value, int duration)
    {
        statuses.Add(new ActiveStatus { type = type, value = value, remainingTurns = duration });
        // UI 표시 가능 (아이콘)
    }

    public void ProcessStatuses()
    {
        List<ActiveStatus> expired = new List<ActiveStatus>();

        foreach (var s in statuses)
        {
            switch (s.type)
            {
                case StatusType.Burn:
                    card.DamageCard(s.value);
                    break;

                case StatusType.Bleed:
                    card.DamageCard(s.value);
                    break;

                case StatusType.Freeze:
                    // 구현 가능: 이 턴 행동 불가
                    break;
            }

            s.remainingTurns--;
            if (s.remainingTurns <= 0)
                expired.Add(s);
        }

        foreach (var e in expired)
            statuses.Remove(e);
    }
}
