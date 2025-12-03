using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    public static BattleController instance;

    private void Awake()
    {
        instance = this;
    }

    public int startingMana = 4, maxMana = 12;
    public int playerMana, enemyMana;
    private int currentPlayerMaxMana, currentEnemyMaxMana;

    public int startingCardsAmount = 5;
    public int cardsToDrawPerTurn = 2;

    public enum TurnOrder { playerActive, playerCardAttacks, enemyActive, enemyCardAttacts }
    public TurnOrder currentPhase;

    public Transform discardPoint;

    public int playerHealth, enemyHealth;
    public bool battleEnded;
    public float resultScreenDelyTime = 5f;

    [Range(0f, 1f)]
    public float playerFirstChance = .5f;

    public bool isBossBattle = false;

    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // 보스전 체크
        if (currentScene == "BossBattle")
            isBossBattle = true;

        currentPlayerMaxMana = startingMana;
        FillPlayerManan();

        DeckController.instance.DrawMultipleCards(startingCardsAmount);
        UIController.instance.SetPlayerHealthText(playerHealth);

        UIController.instance.SetEnemyHealthText(enemyHealth);

        currentEnemyMaxMana = startingMana;
        FillEnemyManan();

        if (Random.value > playerFirstChance)
        {
            currentPhase = TurnOrder.playerCardAttacks;
            AdvanceTurn();
        }

        if (AudioManager.instansce != null)
        {
            if (isBossBattle)
                AudioManager.instansce.PlayBossBGM();
            else
                AudioManager.instansce.PlayBGM();
        }
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            AdvanceTurn();
        }
    }

    public void SpendPlayerMana(int amountToSpend)
    {
        playerMana -= amountToSpend;
        if (playerMana < 0) playerMana = 0;
        UIController.instance.SetPlayerManaText(playerMana);
    }

    public void FillPlayerManan()
    {
        playerMana = currentPlayerMaxMana;
        UIController.instance.SetPlayerManaText(playerMana);
    }

    public void SpendEnemyMana(int amountToSpend)
    {
        enemyMana -= amountToSpend;
        if (enemyMana < 0) enemyMana = 0;
        UIController.instance.SetEnemyManaText(enemyMana);
    }

    public void FillEnemyManan()
    {
        enemyMana = currentEnemyMaxMana;
        UIController.instance.SetEnemyManaText(enemyMana);
    }


public void AdvanceTurn()
    {
        if (battleEnded) return;

        currentPhase++;

        if ((int)currentPhase >= System.Enum.GetValues(typeof(TurnOrder)).Length)
        {
            currentPhase = 0;
        }

        switch (currentPhase)
        {
            case TurnOrder.playerActive:
                // 플레이어 카드들의 턴 시작 효과 
                foreach (var point in CardPointsController.instance.playerCardPoints)
                {
                    if (point.activeCard != null)
                    {
                        var sh = point.activeCard.GetComponent<StatusHandler>();
                        if (sh != null)
                        {
                            sh.ProcessTurnStart();
                        }
                    }
                }

                UIController.instance.endTurnButton.SetActive(true);
                UIController.instance.drawCardButton.SetActive(true);

                if (currentPlayerMaxMana < maxMana)
                    currentPlayerMaxMana++;

                FillPlayerManan();
                DeckController.instance.DrawMultipleCards(cardsToDrawPerTurn);
                break;

            case TurnOrder.playerCardAttacks:
                CardPointsController.instance.PlayerAttack();
                break;

            case TurnOrder.enemyActive:
                // 적 카드들의 턴 시작 효과
                foreach (var point in CardPointsController.instance.enemyCardPoints)
                {
                    if (point.activeCard != null)
                    {
                        var sh = point.activeCard.GetComponent<StatusHandler>();
                        if (sh != null)
                        {
                            sh.ProcessTurnStart();
                        }
                    }
                }

                if (currentEnemyMaxMana < maxMana)
                    currentEnemyMaxMana++;

                FillEnemyManan();

                if (EnemyController.instance != null)
                    EnemyController.instance.StartAction();
                else
                    Debug.LogWarning("[BattleController] EnemyController가 존재하지 않음!");
                break;

            case TurnOrder.enemyCardAttacts:
                CardPointsController.instance.EnemyAttack();
                break;
        }
    }

    public void EndPlayerTurn()
    {
        UIController.instance.endTurnButton.SetActive(false);
        UIController.instance.drawCardButton.SetActive(false);
        AdvanceTurn();
    }

    public void DamagePlayer(int damageAmount)
    {
        if (playerHealth > 0 || !battleEnded)
        {
            playerHealth -= damageAmount;
            if (playerHealth <= 0)
            {
                playerHealth = 0;
                EndBattle();
            }

            UIController.instance.SetPlayerHealthText(playerHealth);
            UIDamageindicator damageClone = Instantiate(UIController.instance.playerDamage, UIController.instance.playerDamage.transform.parent);
            damageClone.damageText.text = damageAmount.ToString();
            damageClone.gameObject.SetActive(true);

            if (AudioManager.instansce != null)
                AudioManager.instansce.PlaySFX(6);
        }
    }

    public void DamageEnemy(int damageAmount)
    {
        if (enemyHealth > 0 || !battleEnded)
        {
            enemyHealth -= damageAmount;
            if (enemyHealth <= 0)
            {
                enemyHealth = 0;
                EndBattle();
            }

            UIController.instance.SetEnemyHealthText(enemyHealth);
            UIDamageindicator damageClone = Instantiate(UIController.instance.enemyDamage, UIController.instance.enemyDamage.transform.parent);
            damageClone.damageText.text = damageAmount.ToString();
            damageClone.gameObject.SetActive(true);

            if (AudioManager.instansce != null)
                AudioManager.instansce.PlaySFX(5);
        }
    }

    void EndBattle()
    {
        battleEnded = true;
        HandController.instance.EmptyHand();

        // 이미지 끄기 (즉시 표시 X)
        UIController.instance.winImage.SetActive(false);
        UIController.instance.loseImage.SetActive(false);
        UIController.instance.bossImage.SetActive(false);

        if (enemyHealth <= 0)
        {
            // 단지 텍스트만 설정
            UIController.instance.battleResultText.text =
                isBossBattle ? "BOSS DEFEATED!" : "YOU WON!";

            foreach (CardPlacePoint point in CardPointsController.instance.enemyCardPoints)
            {
                if (point.activeCard != null)
                    point.activeCard.MoveToPoint(discardPoint.position, point.activeCard.transform.rotation);
            }
        }
        else
        {
            UIController.instance.battleResultText.text = "YOU LOSE!";

            foreach (CardPlacePoint point in CardPointsController.instance.playerCardPoints)
            {
                if (point.activeCard != null)
                    point.activeCard.MoveToPoint(discardPoint.position, point.activeCard.transform.rotation);
            }
        }

        StartCoroutine(ShowResultCo());
    }



    IEnumerator ShowResultCo()
    {
        // 원하는 시간 동안 대기
        yield return new WaitForSeconds(resultScreenDelyTime);

        // 패널 활성화
        if (UIController.instance != null && UIController.instance.battleEndScreen != null)
            UIController.instance.battleEndScreen.SetActive(true);

        Debug.Log("[BattleController] 결과창 표시 완료. 플레이어 입력 대기 중...");

        // 여기서 이미지 선택 후 표시
        if (UIController.instance.battleResultText.text == "YOU WON!")
        {
            UIController.instance.winImage.SetActive(true);
        }
        else if (UIController.instance.battleResultText.text == "YOU LOSE!")
        {
            UIController.instance.loseImage.SetActive(true);
        }
        else if (UIController.instance.battleResultText.text == "BOSS DEFEATED!")
        {
            UIController.instance.bossImage.SetActive(true);
        }

        // 자동 이동 삭제 → 사용자가 버튼 누를 때 이동
        yield break;
    }


    public void OnBattleResultConfirm()
    {
        if (playerHealth > 0)
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.AdvanceFloor();

                if (isBossBattle || GameManager.instance.IsRunFinished())
                {
                    GameManager.instance.InitializeNewRun();
                    SceneManager.LoadScene("MainMenu");
                }
                else
                {
                    SceneManager.LoadScene("MapScene");
                }
            }
        }
        else
        {
            if (GameManager.instance != null)
                GameManager.instance.InitializeNewRun();

            SceneManager.LoadScene("MainMenu");
        }
    }
}
