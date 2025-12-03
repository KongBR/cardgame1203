using System.Collections.Generic;
using UnityEngine;

public class StatusHandler : MonoBehaviour
{
    public Card card;

    [System.Serializable]
    public class ActiveStatus
    {
        public CardAbilityType type;
        public int value;           // 효과 수치 (화상 데미지, 출혈 데미지 등)
        public int remainingTurns;  // 남은 턴 수
    }

    public List<ActiveStatus> activeStatuses = new List<ActiveStatus>();

    private void Awake()
    {
        card = GetComponent<Card>();
    }

    // --------------------------------------------------
    // 상태 부여 (타겟 카드에 Burn / Freeze / Bleed 붙일 때 사용)
    // --------------------------------------------------
    public void AddStatus(CardAbilityType type, int value, int duration)
    {
        if (type == CardAbilityType.None)
            return;

        if (duration <= 0)
            duration = 1;

        ActiveStatus status = new ActiveStatus
        {
            type = type,
            value = value,
            remainingTurns = duration
        };

        activeStatuses.Add(status);
    }

    // --------------------------------------------------
    // 공격이 성공적으로 들어간 뒤 능력 처리
    //   attacker.cardSO.abilities 를 보고
    //   - Burn / Freeze / Bleed : 타겟에 상태 부여
    //   - Lifesteal : 공격자 체력 회복
    // --------------------------------------------------
    public void ApplyOnHitAbilities(Card attacker, Card target)
    {
        if (attacker == null || attacker.cardSO == null)
            return;

        StatusHandler targetStatus = null;
        if (target != null)
        {
            targetStatus = target.GetComponent<StatusHandler>();
        }

        foreach (CardAbility ability in attacker.cardSO.abilities)
        {
            switch (ability.type)
            {
                case CardAbilityType.Burn:
                    // 내가 공격한 상대 카드에 화상 부여
                    if (targetStatus != null)
                    {
                        int dur = ability.duration <= 0 ? 1 : ability.duration;
                        targetStatus.AddStatus(CardAbilityType.Burn, ability.value, dur);
                    }
                    break;

                case CardAbilityType.Freeze:
                    // 상대 카드 빙결 (다음 공격 턴에서 공격 못함)
                    if (targetStatus != null)
                    {
                        int dur = ability.duration <= 0 ? 1 : ability.duration;
                        targetStatus.AddStatus(CardAbilityType.Freeze, 0, dur);
                    }
                    break;

                case CardAbilityType.Bleed:
                    // 상대 카드 출혈 (그 카드가 공격할 때마다 자기 체력 손실)
                    if (targetStatus != null)
                    {
                        int dur = ability.duration <= 0 ? 1 : ability.duration;
                        targetStatus.AddStatus(CardAbilityType.Bleed, ability.value, dur);
                    }
                    break;

                case CardAbilityType.Lifesteal:
                    // 흡혈: 공격력만큼 체력 회복
                    int heal = attacker.attackPower;
                    attacker.currentHealth += heal;
                    attacker.UpdateCardDisplay();
                    break;
            }
        }
    }

    // --------------------------------------------------
    // 이 카드의 "자신의 턴 시작"에 호출
    //   - Burn : 매 턴 시작마다 데미지
    //   - Freeze : 턴 수 차감만, 공격 제한은 CanAttackThisTurn에서 체크
    // --------------------------------------------------
    public void ProcessTurnStart()
    {
        for (int i = activeStatuses.Count - 1; i >= 0; i--)
        {
            ActiveStatus s = activeStatuses[i];

            if (s.type == CardAbilityType.Burn)
            {
                // 화상: 턴 시작 시 피해
                card.DamageCard(s.value);
                s.remainingTurns--;

                if (s.remainingTurns <= 0)
                {
                    activeStatuses.RemoveAt(i);
                    continue;
                }
            }
            else if (s.type == CardAbilityType.Freeze)
            {
                // 빙결: 턴 시작 시 턴 수 감소만
                s.remainingTurns--;
                if (s.remainingTurns <= 0)
                {
                    activeStatuses.RemoveAt(i);
                }
            }
        }
    }

    // --------------------------------------------------
    // 이 카드가 "공격을 시도"할 때 호출
    //   - Freeze : 남은 턴 있으면 이번 턴 공격 불가
    //   - Bleed  : 공격할 때마다 자기 체력 손실
    //   반환값: 이번 턴에 공격 가능한지 여부
    // --------------------------------------------------
    public bool CanAttackThisTurn()
    {
        // 얼어 있으면 이번 턴 공격 불가
        if (IsFrozen())
        {
            return false;
        }

        // 출혈: 공격할 때마다 자기 체력 손실
        for (int i = activeStatuses.Count - 1; i >= 0; i--)
        {
            ActiveStatus s = activeStatuses[i];

            if (s.type == CardAbilityType.Bleed)
            {
                card.DamageCard(s.value);
                s.remainingTurns--;

                if (s.remainingTurns <= 0)
                {
                    activeStatuses.RemoveAt(i);
                }
            }
        }

        return true;
    }

    public bool IsFrozen()
    {
        for (int i = 0; i < activeStatuses.Count; i++)
        {
            ActiveStatus s = activeStatuses[i];
            if (s.type == CardAbilityType.Freeze && s.remainingTurns > 0)
            {
                return true;
            }
        }
        return false;
    }
}
