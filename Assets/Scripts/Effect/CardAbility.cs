using UnityEngine;

[System.Serializable]
public class CardAbility
{
    // StatusHandler에서 사용하는 필드 이름에 정확히 맞춤
    public CardAbilityType type = CardAbilityType.None;

    // 능력 수치 (화상 데미지, 출혈량, 흡혈량 등)
    public int value = 0;

    // 지속 턴 수
    public int duration = 0;
}
