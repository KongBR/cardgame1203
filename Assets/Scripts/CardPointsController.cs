using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPointsController : MonoBehaviour
{
    public static CardPointsController instance;

    public float timeBetweenAttacks = .25f;

    private void Awake()
    {
        instance = this;
    }

    public CardPlacePoint[] playerCardPoints, enemyCardPoints;

    public void PlayerAttack()
    {
        StartCoroutine(PlayerAttackCo());
    }

    IEnumerator PlayerAttackCo()
    {
        yield return new WaitForSeconds(timeBetweenAttacks);

        for (int i = 0; i < playerCardPoints.Length; i++)
        {
            Card attacker = playerCardPoints[i].activeCard;

            if (attacker != null)
            {
                StatusHandler atkStatus = attacker.GetComponent<StatusHandler>();

                bool canAttack = true;
                if (atkStatus != null)
                {
                    canAttack = atkStatus.CanAttackThisTurn();
                    if (attacker.currentHealth <= 0)
                    {
                        canAttack = false;
                    }
                }

                if (canAttack)
                {
                    Card defender = null;

                    if (enemyCardPoints[i].activeCard != null)
                    {
                        defender = enemyCardPoints[i].activeCard;

                        // 기본 공격 데미지
                        defender.DamageCard(attacker.attackPower);

                        // 능력 적용 (화상/빙결/출혈/흡혈)
                        if (atkStatus != null)
                        {
                            atkStatus.ApplyOnHitAbilities(attacker, defender);
                        }
                    }
                    else
                    {
                        // 적 카드가 없으면 적 전체 체력 공격
                        BattleController.instance.DamageEnemy(attacker.attackPower);

                        if (atkStatus != null)
                        {
                            // 타겟 카드 없을 때도 흡혈은 적용 가능
                            atkStatus.ApplyOnHitAbilities(attacker, null);
                        }
                    }

                    attacker.anim.SetTrigger("Attack");
                    yield return new WaitForSeconds(timeBetweenAttacks);
                }
            }

            if (BattleController.instance.battleEnded == true)
            {
                i = playerCardPoints.Length;
            }
        }

        CheckAssignedCards();

        BattleController.instance.AdvanceTurn();
    }

    public void EnemyAttack()
    {
        StartCoroutine(EnemyAttackCo());
    }

    IEnumerator EnemyAttackCo()
    {
        yield return new WaitForSeconds(timeBetweenAttacks);

        for (int i = 0; i < enemyCardPoints.Length; i++)
        {
            Card attacker = enemyCardPoints[i].activeCard;

            if (attacker != null)
            {
                StatusHandler atkStatus = attacker.GetComponent<StatusHandler>();

                bool canAttack = true;
                if (atkStatus != null)
                {
                    canAttack = atkStatus.CanAttackThisTurn();
                    if (attacker.currentHealth <= 0)
                    {
                        canAttack = false;
                    }
                }

                if (canAttack)
                {
                    Card defender = null;

                    if (playerCardPoints[i].activeCard != null)
                    {
                        defender = playerCardPoints[i].activeCard;

                        defender.DamageCard(attacker.attackPower);

                        if (atkStatus != null)
                        {
                            atkStatus.ApplyOnHitAbilities(attacker, defender);
                        }
                    }
                    else
                    {
                        BattleController.instance.DamagePlayer(attacker.attackPower);

                        if (atkStatus != null)
                        {
                            atkStatus.ApplyOnHitAbilities(attacker, null);
                        }
                    }

                    attacker.anim.SetTrigger("Attack");
                    yield return new WaitForSeconds(timeBetweenAttacks);
                }
            }

            if (BattleController.instance.battleEnded == true)
            {
                i = enemyCardPoints.Length;
            }
        }

        CheckAssignedCards();

        BattleController.instance.AdvanceTurn();
    }

    public void CheckAssignedCards()
    {
        foreach (CardPlacePoint point in enemyCardPoints)
        {
            if (point.activeCard != null)
            {
                if (point.activeCard.currentHealth <= 0)
                {
                    point.activeCard = null;
                }
            }
        }
        foreach (CardPlacePoint point in playerCardPoints)
        {
            if (point.activeCard != null)
            {
                if (point.activeCard.currentHealth <= 0)
                {
                    point.activeCard = null;
                }
            }
        }
    }
}
