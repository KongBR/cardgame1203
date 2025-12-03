using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public CardScriptableObject cardSO;

    public List<CardAbility> cardAbilities = new List<CardAbility>();

    public bool isPlayer;

    public int currentHealth;
    public int attackPower;
    public int manaCost;

    public TMP_Text healthText, attackText, costText, nameText, actionDescriptionText, loreText;

    public Image characterArt, bgArt;

    private Vector3 targetPoint;
    private Quaternion targetRot;
    public float moveSpeed = 5f, rotateSpeed = 540f;

    public bool inHand;
    public int handPositon;

    private HandController theHC;

    private bool isSelected;
    public Collider theCo1;

    public LayerMask whatIsDesktop, whatIsPlacement;
    private bool justPressed;

    public CardPlacePoint assignedPlace;

    public Animator anim;

    private StatusHandler statusHandler;

    bool hasLanded = false;

    void OnCardLanded()
    {
        if (hasLanded) return;
        hasLanded = true;

        // 강한 카드만 효과 발동
        if (attackPower >= 6 || currentHealth >= 6)
        {
            StartCoroutine(HeavyCardImpactEffect());
        }
    }

    IEnumerator HeavyCardImpactEffect()
    {
        CameraShake.Shake(0.25f, 0.35f); // 화면 흔들림

        // 카드 튀기기
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 1.2f;
        yield return new WaitForSeconds(0.05f);
        transform.localScale = originalScale;

        // 충격음 재생
        if (AudioManager.instansce != null)
            AudioManager.instansce.PlaySFX(3); // 효과음

        yield return null;
    }

    void Start()
    {
        if (targetPoint == Vector3.zero)
        {
            targetPoint = transform.position;
            targetRot = transform.rotation;
        }

        SetupCard();

        theHC = FindObjectOfType<HandController>();
        theCo1 = GetComponent<Collider>();

        // StatusHandler 등록
        statusHandler = gameObject.AddComponent<StatusHandler>();

        ApplyHeavyCardDropEffect();
    }

    public void SetupCard()
    {
        currentHealth = cardSO.currentHealth;
        attackPower = cardSO.attackPower;
        manaCost = cardSO.manaCost;

        UpdateCardDisplay();

        nameText.text = cardSO.cardName;
        actionDescriptionText.text = cardSO.actionDescription;
        loreText.text = cardSO.cardLore;

        characterArt.sprite = cardSO.characterSprite;
        bgArt.sprite = cardSO.bgSprite;

        // CardSO → CardAbilities 복사 (등록만 하고 Status에는 넣지 않음)
        cardAbilities.Clear();
        if (cardSO.abilities != null)
            cardAbilities.AddRange(cardSO.abilities);
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPoint, moveSpeed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);

        if (isSelected && BattleController.instance.battleEnded == false && Time.timeScale != 0f)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, whatIsDesktop))
            {
                MoveToPoint(hit.point + new Vector3(0f, 2f, 0f), Quaternion.identity);
            }

            if (Input.GetMouseButtonDown(1))
                ReturnToHand();

            if (Input.GetMouseButtonDown(0) && justPressed == false)
            {
                if (Physics.Raycast(ray, out hit, 100f, whatIsPlacement) &&
                    BattleController.instance.currentPhase == BattleController.TurnOrder.playerActive)
                {
                    CardPlacePoint selectedPoint = hit.collider.GetComponent<CardPlacePoint>();

                    if (selectedPoint.activeCard == null && selectedPoint.isPlayerPoint)
                    {
                        if (BattleController.instance.playerMana >= manaCost)
                        {
                            selectedPoint.activeCard = this;
                            assignedPlace = selectedPoint;

                            MoveToPoint(selectedPoint.transform.position, Quaternion.identity);

                            inHand = false;
                            isSelected = false;
                            theCo1.enabled = false;

                            theHC.RemoveCardFromHnad(this);

                            BattleController.instance.SpendPlayerMana(manaCost);
                            AudioManager.instansce.PlaySFX(4);
                        }
                        else
                        {
                            ReturnToHand();
                            UIController.instance.ShowManaWarning();
                        }
                    }
                    else
                    {
                        ReturnToHand();
                    }
                }
                else
                {
                    ReturnToHand();
                }
            }
        }

        justPressed = false;

        if (!inHand && assignedPlace != null)
        {
            float dist = Vector3.Distance(transform.position, assignedPlace.transform.position);
            if (dist < 0.1f)
            {
                OnCardLanded();
            }
        }

    }

    public void MoveToPoint(Vector3 pointToMoveTo, Quaternion rotToWatch)
    {
        targetPoint = pointToMoveTo;
        targetRot = rotToWatch;
    }

    private void OnMouseOver()
    {
        if (inHand && isPlayer)
            MoveToPoint(theHC.cardPositions[handPositon] + new Vector3(0f, 1f, .5f), Quaternion.identity);
    }

    private void OnMouseExit()
    {
        if (inHand && isPlayer)
            MoveToPoint(theHC.cardPositions[handPositon], theHC.minPos.rotation);
    }

    private void OnMouseDown()
    {
        if (inHand && BattleController.instance.currentPhase == BattleController.TurnOrder.playerActive && isPlayer)
        {
            isSelected = true;
            theCo1.enabled = false;
            justPressed = true;
        }
    }

    public void ReturnToHand()
    {
        isSelected = false;
        theCo1.enabled = true;
        MoveToPoint(theHC.cardPositions[handPositon], theHC.minPos.rotation);
    }

    public void DamageCard(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;

            if (assignedPlace != null)
                assignedPlace.activeCard = null;

            MoveToPoint(BattleController.instance.discardPoint.position,
                        BattleController.instance.discardPoint.rotation);

            anim.SetTrigger("Jump");

            Destroy(gameObject, 0.5f);

            AudioManager.instansce.PlaySFX(2);
        }
        else
        {
            AudioManager.instansce.PlaySFX(1);
            anim.SetTrigger("Hurt");
        }

        UpdateCardDisplay();
    }

    public void UpdateCardDisplay()
    {
        healthText.text = currentHealth.ToString();
        attackText.text = attackPower.ToString();
        costText.text = manaCost.ToString();
    }

    public StatusHandler GetStatusHandler()
    {
        return statusHandler;
    }

    void ApplyHeavyCardDropEffect()
    {
        if (attackPower >= 6 || currentHealth >= 6)
        {
            moveSpeed = 10f;          // 더 빠르게 내려옴
            rotateSpeed = 800f;       // 더 강하게 회전하며 내려오도록
        }
    }

}
