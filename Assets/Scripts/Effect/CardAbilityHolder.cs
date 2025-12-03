using System.Collections.Generic;
using UnityEngine;

public class CardAbilityHolder : MonoBehaviour
{
    [Header("선택한 능력들 (원하면 비워도 됨)")]
    public List<CardAbilitySO> abilities = new List<CardAbilitySO>();

    // 카드가 생성된 후 능력을 적용할 때 사용
    public List<CardAbilitySO> GetAbilities()
    {
        return abilities;
    }
}
