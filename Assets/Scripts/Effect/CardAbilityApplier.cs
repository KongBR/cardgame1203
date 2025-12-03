using System.Collections.Generic;
using UnityEngine;

public class CardAbilityApplier : MonoBehaviour
{
    public List<CardAbilityData> abilities = new List<CardAbilityData>();

    void Start()
    {
        StatusHandler handler = GetComponent<StatusHandler>();
        if (handler == null)
            handler = gameObject.AddComponent<StatusHandler>();

        // 능력은 공격 시 적용 → 여기선 저장만 해둠
    }
}
