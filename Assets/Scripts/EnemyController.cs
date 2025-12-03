using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public static EnemyController instance;

    private void Awake()
    {
        instance = this;
    }

    public List<CardScriptableObject> deckToUse = new List<CardScriptableObject>();
    private List<CardScriptableObject> activeCards = new List<CardScriptableObject>();

    public Card cardToSpawn;
    public Transform cardSpawnPoint;

    public enum AIType { placeFromDeck, handRandomPlace, handDefensive, handAttacking }
    public AIType enemyAIType;

    private List<CardScriptableObject> cardsInHand = new List<CardScriptableObject>();
    public int startHandSize;

    // ===================== 화이트 위치 관련 추가 ========================
    // whitewitch_card에 사용할 카드 SO (화이트 위치 카드의 CardScriptableObject)
    public CardScriptableObject whiteWitchCardSO;

    // 보스 체력이 이 값 이하가 되면 화이트 위치를 소환
    public int whiteWitchHealthThreshold = 30;

    // 전투 중 단 한 번만 내도록 플래그
    private bool whiteWitchPlayed = false;
    // ================================================================

    void Start()
    {
        SetupDeck();

        if (enemyAIType != AIType.placeFromDeck)
        {
            SetupHand();
        }
    }

    void Update()
    {

    }

    public void SetupDeck()
    {
        activeCards.Clear();

        List<CardScriptableObject> tempDeck = new List<CardScriptableObject>();
        tempDeck.AddRange(deckToUse);

        int iterations = 0;
        while (tempDeck.Count > 0 && iterations < 500)
        {
            int selected = Random.Range(0, tempDeck.Count);
            activeCards.Add(tempDeck[selected]);
            tempDeck.RemoveAt(selected);

            iterations++;
        }
    }

    public void StartAction()
    {
        StartCoroutine(EnemyActionCo());
    }

    IEnumerator EnemyActionCo()
    {
        if (activeCards.Count == 0)
        {
            SetupDeck();
        }

        yield return new WaitForSeconds(.5f);

        // ===================== 화이트 위치 소환 체크 추가 =====================
        TrySummonWhiteWitchCardOnce();
        // =====================================================================

        if (enemyAIType != AIType.placeFromDeck)
        {
            for (int i = 0; i < BattleController.instance.cardsToDrawPerTurn; i++)
            {
                cardsInHand.Add(activeCards[0]);
                activeCards.RemoveAt(0);

                if (activeCards.Count == 0)
                {
                    SetupDeck();
                }
            }
        }

        List<CardPlacePoint> cardPoints = new List<CardPlacePoint>();
        cardPoints.AddRange(CardPointsController.instance.enemyCardPoints);

        int randomPoint = Random.Range(0, cardPoints.Count);
        CardPlacePoint selectedPoint = cardPoints[randomPoint];

        if (enemyAIType == AIType.placeFromDeck || enemyAIType == AIType.handRandomPlace)
        {

            cardPoints.Remove(selectedPoint);

            while (selectedPoint.activeCard != null && cardPoints.Count > 0)
            {
                randomPoint = Random.Range(0, cardPoints.Count);
                selectedPoint = cardPoints[randomPoint];
                cardPoints.RemoveAt(randomPoint);
            }
        }

        CardScriptableObject selectedCard = null;
        int iterations2 = 0;
        List<CardPlacePoint> preferredPoints = new List<CardPlacePoint>();
        List<CardPlacePoint> secondaryPoints = new List<CardPlacePoint>();


        switch (enemyAIType)
        {
            case AIType.placeFromDeck:

                if (selectedPoint.activeCard == null)
                {
                    Card newCard = Instantiate(cardToSpawn, cardSpawnPoint.position, cardSpawnPoint.rotation);
                    newCard.cardSO = activeCards[0];
                    activeCards.RemoveAt(0);
                    newCard.SetupCard();
                    newCard.MoveToPoint(selectedPoint.transform.position, selectedPoint.transform.rotation);

                    selectedPoint.activeCard = newCard;
                    newCard.assignedPlace = selectedPoint;
                }

                break;

            case AIType.handRandomPlace:

                selectedCard = SelectedCardToPlay();


                iterations2 = 50;
                while (selectedCard != null && iterations2 > 0 && selectedPoint.activeCard == null)
                {
                    PlayCard(selectedCard, selectedPoint);

                    //check if we should try play another card
                    selectedCard = SelectedCardToPlay();

                    iterations2--;

                    yield return new WaitForSeconds(CardPointsController.instance.timeBetweenAttacks);

                    while (selectedPoint.activeCard != null && cardPoints.Count > 0)
                    {
                        randomPoint = Random.Range(0, cardPoints.Count);
                        selectedPoint = cardPoints[randomPoint];
                        cardPoints.RemoveAt(randomPoint);
                    }
                }


                break;

            case AIType.handDefensive:

                selectedCard = SelectedCardToPlay();

                preferredPoints.Clear();
                secondaryPoints.Clear();

                for (int i = 0; i < cardPoints.Count; i++)
                {
                    if (cardPoints[i].activeCard == null)
                    {
                        if (CardPointsController.instance.playerCardPoints[i].activeCard == null)
                        {
                            preferredPoints.Add(cardPoints[i]);
                        }
                        else
                        {
                            secondaryPoints.Add(cardPoints[i]);
                        }
                    }
                }


                iterations2 = 50;
                while (selectedCard != null && iterations2 > 0 && preferredPoints.Count + secondaryPoints.Count > 0)
                {
                    //pick a point to use
                    if (preferredPoints.Count > 0)
                    {
                        int selectPoint = Random.Range(0, preferredPoints.Count);
                        selectedPoint = preferredPoints[selectPoint];

                        preferredPoints.RemoveAt(selectPoint);
                    }
                    else
                    {
                        int selectPoint = Random.Range(0, secondaryPoints.Count);
                        selectedPoint = secondaryPoints[selectPoint];

                        secondaryPoints.RemoveAt(selectPoint);
                    }

                    PlayCard(selectedCard, selectedPoint);

                    //check if we should try play  another
                    selectedCard = SelectedCardToPlay();

                    iterations2--;

                    yield return new WaitForSeconds(CardPointsController.instance.timeBetweenAttacks);

                }

                break;

            case AIType.handAttacking:

                selectedCard = SelectedCardToPlay();

                preferredPoints.Clear();
                secondaryPoints.Clear();

                for (int i = 0; i < cardPoints.Count; i++)
                {
                    if (cardPoints[i].activeCard == null)
                    {
                        if (CardPointsController.instance.playerCardPoints[i].activeCard == null)
                        {
                            preferredPoints.Add(cardPoints[i]);
                        }
                        else
                        {
                            secondaryPoints.Add(cardPoints[i]);
                        }
                    }
                }


                iterations2 = 50;
                while (selectedCard != null && iterations2 > 0 && preferredPoints.Count + secondaryPoints.Count > 0)
                {
                    //pick a point to use
                    if (preferredPoints.Count > 0)
                    {
                        int selectPoint = Random.Range(0, preferredPoints.Count);
                        selectedPoint = preferredPoints[selectPoint];

                        preferredPoints.RemoveAt(selectPoint);
                    }
                    else
                    {
                        int selectPoint = Random.Range(0, secondaryPoints.Count);
                        selectedPoint = secondaryPoints[selectPoint];

                        secondaryPoints.RemoveAt(selectPoint);
                    }

                    PlayCard(selectedCard, selectedPoint);

                    //check if we should try play  another
                    selectedCard = SelectedCardToPlay();

                    iterations2--;

                    yield return new WaitForSeconds(CardPointsController.instance.timeBetweenAttacks);

                }

                break;
        }
        yield return new WaitForSeconds(0.5f);

        BattleController.instance.AdvanceTurn();
    }

    void SetupHand()
    {
        for (int i = 0; i < startHandSize; i++)
        {
            if (activeCards.Count == 0)
            {
                SetupDeck();
            }

            cardsInHand.Add(activeCards[0]);
            activeCards.RemoveAt(0);
        }
    }

    public void PlayCard(CardScriptableObject cardSO, CardPlacePoint placePoint)
    {
        Card newCard = Instantiate(cardToSpawn, cardSpawnPoint.position, cardSpawnPoint.rotation);
        newCard.cardSO = cardSO;

        newCard.SetupCard();
        newCard.MoveToPoint(placePoint.transform.position, placePoint.transform.rotation);

        placePoint.activeCard = newCard;
        newCard.assignedPlace = placePoint;

        cardsInHand.Remove(cardSO);

        BattleController.instance.SpendEnemyMana(cardSO.manaCost);

        AudioManager.instansce.PlaySFX(4);
    }

    CardScriptableObject SelectedCardToPlay()
    {
        CardScriptableObject cardToPlay = null;

        List<CardScriptableObject> cardsToPlay = new List<CardScriptableObject>();
        foreach (CardScriptableObject card in cardsInHand)
        {
            if (card.manaCost <= BattleController.instance.enemyMana)
            {
                cardsToPlay.Add(card);
            }
        }

        if (cardsToPlay.Count > 0)
        {
            int selected = Random.Range(0, cardsToPlay.Count);

            cardToPlay = cardsToPlay[selected];
        }

        return cardToPlay;
    }

    // ====================== 화이트 위치 소환 로직 =========================
    private void TrySummonWhiteWitchCardOnce()
    {
        if (whiteWitchPlayed) return;
        if (!BattleController.instance.isBossBattle) return;

        // 체력 조건 충족해야 함
        if (BattleController.instance.enemyHealth > whiteWitchHealthThreshold)
            return;

        // white witch 카드 없으면 소환 불가
        if (whiteWitchCardSO == null)
        {
            Debug.LogError("White Witch SO not assigned in Inspector!");
            return;
        }

        CardPlacePoint[] enemyPoints = CardPointsController.instance.enemyCardPoints;

        if (enemyPoints == null || enemyPoints.Length == 0)
            return;

        int centerIndex = enemyPoints.Length / 2;

        int chosenIndex = -1;

        // 가운데부터 양쪽으로 빈칸 탐색
        for (int offset = 0; offset < enemyPoints.Length; offset++)
        {
            int left = centerIndex - offset;
            int right = centerIndex + offset;

            if (left >= 0 && enemyPoints[left].activeCard == null)
            {
                chosenIndex = left;
                break;
            }

            if (right < enemyPoints.Length && enemyPoints[right].activeCard == null)
            {
                chosenIndex = right;
                break;
            }
        }

        if (chosenIndex == -1)
        {
            Debug.Log("WhiteWitch: No available slot.");
            return;
        }

        CardPlacePoint targetPoint = enemyPoints[chosenIndex];

        // 소환
        Card newCard = Instantiate(cardToSpawn,
            cardSpawnPoint.position, cardSpawnPoint.rotation);

        newCard.cardSO = whiteWitchCardSO;
        newCard.SetupCard();
        newCard.MoveToPoint(targetPoint.transform.position, targetPoint.transform.rotation);

        targetPoint.activeCard = newCard;
        newCard.assignedPlace = targetPoint;
        newCard.inHand = false;

        whiteWitchPlayed = true;

        AudioManager.instansce.PlaySFX(4);
        Debug.Log("White Witch Summoned!");
    }

}
