using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardsHandler : MonoBehaviour
{
    public static CardsHandler instance;
    private List<Card> allCards = new List<Card>();

    [Header("Cards")]
    private List<GameCard> deckCards = new List<GameCard>();
    public List<Card> pileCards = new List<Card>();

    [Header("Deck Settings")]
    [SerializeField] private float maxAngle;
    [SerializeField] private float radius;
    [SerializeField] private float defaultY;
    [SerializeField] private float heightScale;
    [HideInInspector] public bool canPass = true;
    [HideInInspector] public bool canRedraw = true;

    [Header("References")]
    [SerializeField] private Transform deckHolder;
    [SerializeField] private Transform pileHolder;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private CanvasGroup useCardScreen;
    private GameCard focusedGameCard;
    private CardPrefab tempCardPrefab;

    void Awake() {
        instance = this;
        allCards = Resources.LoadAll<Card>("Cards").ToList();
    }

    void Start()
    {
        CreatePile();
        CreateDeck();
        ArrangeCards();
    }

    //-------------------------------------------------- CARD USAGE --------------------------------------------------

    public void UseCard() {
        Card card = focusedGameCard.card;

        if(card.cardType == CardType.Buildable) {
            PlacementHandler.instance.EnterPlacementMode(card);
        }

        HideCardScreen(false);
    }
    public void PassCard() {
        HideCardScreen(true);
        StartCoroutine(ShuffleCards());
    }

    public void ShowCardScreen(GameObject cardObject) {
        focusedGameCard = GetGameCardFromCardObject(cardObject);

        //HIDING ACTUAL CARD IN DECK

        focusedGameCard.cardObject.SetActive(false);
        focusedGameCard.cardObject.GetComponent<CardPrefab>().ButtonExit();

        //CREATING FAKE TEMP CARD FOR ANIMATION

        tempCardPrefab = Instantiate(cardPrefab, useCardScreen.transform.parent).GetComponent<CardPrefab>();
        tempCardPrefab.transform.SetPositionAndRotation(cardObject.transform.position, cardObject.transform.rotation);
        tempCardPrefab.Initialize(focusedGameCard.card);
        tempCardPrefab.GetComponent<EventTrigger>().enabled = false;
        tempCardPrefab.ButtonExit();

        //ANIMATION

        tempCardPrefab.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, 0.5f);
        tempCardPrefab.transform.DORotate(Vector3.zero, 0.5f);
        tempCardPrefab.transform.DOScale(2f, 0.5f);
        
        //SHOWING SCREEN

        useCardScreen.gameObject.SetActive(true);
        useCardScreen.DOFade(1f, 0.5f);

        //REVEALING CARD

        tempCardPrefab.ShowCard();
    }
    private void HideCardScreen(bool pass) {
        useCardScreen.DOFade(0f, 0.5f).OnComplete(() => {
            useCardScreen.gameObject.SetActive(false);
        });

        if(pass) {

            tempCardPrefab.transform.DOMove(deckHolder.position, 0.5f);
            tempCardPrefab.transform.DOScale(1f, 0.5f).OnComplete(() => {
                Destroy(tempCardPrefab.gameObject);
                focusedGameCard.cardObject.SetActive(true);
            });

            tempCardPrefab.HideCard();

        } else {

            tempCardPrefab.transform.DOScale(0f, 0.5f).OnComplete(() => {
                Destroy(tempCardPrefab.gameObject);
                RemoveCard(focusedGameCard.cardObject);

                AddCard(pileCards[0]);
                pileCards.RemoveAt(0);
                CreatePile();
            });

        }
    }

    //-------------------------------------------------- CARD METHODS --------------------------------------------------

    private void CreatePile() {
        while(pileCards.Count < 5) {
            Card card = allCards[Random.Range(0, allCards.Count)];
            
            while(deckCards.Any(x => x.card == card)) {
                card = allCards[Random.Range(0, allCards.Count)];
            }

            pileCards.Add(card);
        }
    }

    public void CreateDeck() {
        List<Card> cards = pileCards;
        pileCards.Clear();
        CreatePile();

        for(int i = 0; i < 5; i++) {
            AddCard(cards[i]);
        }
    }

    private void ClearDeck() {
        foreach(GameCard card in deckCards) {
            card.cardObject.GetComponent<CardPrefab>().DestroyCard();
            
            float randomTime = Random.Range(0.5f, 1f);
            card.cardObject.transform.DORotate(Vector3.zero, 1f);
            card.cardObject.transform.DOMove(pileHolder.position, randomTime).OnComplete(() => {
                Destroy(card.cardObject);
            });
        }
        deckCards.Clear();
    }

    public void ArrangeCards() {
        if(deckCards.Count == 0) {
            return;
        }

        for(int i = 0; i < deckCards.Count; i++) {
            (Vector2 pos, Vector3 rot) cardTransform = GetCardTransform(i);

            deckCards[i].cardObject.GetComponent<RectTransform>().DOAnchorPos(cardTransform.pos, 1f);
            deckCards[i].cardObject.transform.DOLocalRotate(cardTransform.rot, 1f);

            deckCards[i].cardObject.GetComponent<CardPrefab>().defaultAnchorPos = cardTransform.pos;
            deckCards[i].cardObject.GetComponent<CardPrefab>().defaultRotationZ = cardTransform.rot.z;
        }
    }

    private (Vector2 pos, Vector3 rot) GetCardTransform(int index) {
        int totalCards = deckCards.Count;
        if(totalCards == 1) {
            return (new Vector2(0, defaultY), Vector3.zero);
        }

        float angleStep = maxAngle / Mathf.Max(1, totalCards - 1);
        float startAngle = -maxAngle / 2f;
        float angle = startAngle + (index * angleStep);
        float t = totalCards > 1 ? index / (float)(totalCards - 1) : 0f;

        float xPos = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
        float yPos = defaultY + Mathf.Sin(t * Mathf.PI) * heightScale;
        Quaternion rotation = Quaternion.Euler(0, 0, -angle);

        return (new Vector2(xPos, yPos), rotation.eulerAngles);
    }

    private IEnumerator ShuffleCards() {

        for(int i = 0; i < deckCards.Count; i++) {
            deckCards[i].cardObject.GetComponent<EventTrigger>().enabled = false;
            deckCards[i].cardObject.GetComponent<CardPrefab>().ButtonExit();
            deckCards[i].cardObject.transform.DOMove(deckHolder.transform.position, 0.5f);
            deckCards[i].cardObject.transform.DORotate(Vector3.zero, 0.5f);
        }

        yield return new WaitForSeconds(0.5f);

        deckCards = deckCards.OrderBy(x => Random.Range(0f, 1f)).ToList();

        for(int i = 0; i < deckCards.Count; i++) {
            deckCards[i].cardObject.transform.SetSiblingIndex(i);
        }

        ArrangeCards();

        yield return new WaitForSeconds(1f);

        for(int i = 0; i < deckCards.Count; i++) {
            deckCards[i].cardObject.GetComponent<EventTrigger>().enabled = true;
        }
    }

    public void UpdateDeck() {
        ArrangeCards();
    }

    public void AddCard(Card card) {

        GameObject cardObject = Instantiate(cardPrefab, deckHolder);
        cardObject.transform.position = pileHolder.transform.position;
        GameCard gameCard = new GameCard(card, cardObject);

        deckCards.Add(gameCard);
        cardObject.GetComponent<CardPrefab>().Initialize(card);

        UpdateDeck();
    }

    public void RemoveCard(GameObject cardObject) {
        GameCard targetGameCard = GetGameCardFromCardObject(cardObject);

        deckCards.Remove(targetGameCard);
        Destroy(cardObject);

        UpdateDeck();
    }

    public void RedrawDeck() {
        canRedraw = false;

        ClearDeck();
        Invoke(nameof(CreateDeck), 2f);
    }

    //-------------------------------------------------- UTILITY METHODS --------------------------------------------------

    private GameCard GetGameCardFromCardObject(GameObject cardObject) {
        return deckCards.FirstOrDefault(x => x.cardObject == cardObject);
    }
}

public class GameCard {
    public Card card;
    public GameObject cardObject;

    public GameCard(Card card, GameObject cardObject)
    {
        this.card = card;
        this.cardObject = cardObject;
    }
}
