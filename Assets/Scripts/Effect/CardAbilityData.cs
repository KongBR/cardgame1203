using UnityEngine;

[System.Serializable]
public class CardAbilityData
{
    public CardAbilityType abilityType;

    // 능력의 실제 수치
    public int effectValue = 1;

    // 지속 턴 수
    public int duration = 1;

    // 옛 코드(ability.value)에 대한 호환용 프로퍼티
    public int value
    {
        get => effectValue;
        set => effectValue = value;
    }
}
