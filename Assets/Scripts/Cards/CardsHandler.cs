using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardsHandler : MonoBehaviour
{
    public static CardsHandler instance;
    private List<Card> allCards = new List<Card>();

    [Header("Cards")]
    private List<GameCard> deckCards = new List<GameCard>();
    public List<GameCard> pileCards = new List<GameCard>();
    [HideInInspector] public bool canUseCard = false;
    [HideInInspector] public bool activeDeck = false;

    [Header("Deck")]
    [SerializeField] private float maxAngle;
    [SerializeField] private float radius;
    [SerializeField] private float defaultY;
    [SerializeField] private float heightScale;
    [HideInInspector] public bool canPass = true;
    [HideInInspector] public bool canPlay = false;

    [Header("Draw")]
    [SerializeField] private int redraws;
    [SerializeField] private int totalPileCards;
    private bool drawingCard = false;

    [Header("References")]
    [SerializeField] private Transform deckHolder;
    [SerializeField] private Transform pileHolder;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private CanvasGroup drawCardScreen;
    [SerializeField] private TMP_Text drawCardMainText;
    [SerializeField] private TMP_Text drawCardRarityText;
    [SerializeField] private TMP_Text keepButtonText;
    [SerializeField] private TMP_Text redrawButtonText;
    [SerializeField] private List<Transform> buttonTransforms;
    [SerializeField] private List<Sprite> typeIcons;
    [SerializeField] private List<Color> rarityColors;
    private GameCard focusedGameCard;

    void Awake() {
        instance = this;
        allCards = Resources.LoadAll<Card>("Cards").ToList();
    }

    void Start()
    {
        ArrangeCards();
    }

    //-------------------------------------------------- CARD USAGE --------------------------------------------------

    public void TryUseCard(GameObject cardObject) {
        if(!canUseCard) {
            return;
        }
        canUseCard = false;

        focusedGameCard = GetGameCardFromCardObject(cardObject);

        UseCardAnimation();

        Invoke(nameof(UseCard), 4f);
    }

    private void UseCard() {

        Card card = focusedGameCard.card;

        if(card.cardType == CardType.Buildable) {

            PlacementHandler.instance.EnterPlacementMode(card);

        } else if(card.cardType == CardType.Upgrade) {

            PlacementHandler.instance.EnterPlacementMode(card);
            
        } else if(card.cardType == CardType.Convert) {

            StatsHandler.instance.ChangeStat(card.statFrom, -card.statFromValue);
            StatsHandler.instance.ChangeStat(card.statTo, card.statToValue);

        } else if(card.cardType == CardType.Boost) {

            if(card.instantBoost) {
                StatsHandler.instance.ChangeStat(card.boostedStat, card.boostValue);    
            } else {
                StatsHandler.instance.CreateConstantStatChange(card.boostedStat, card.boostValue, card.boostLength);
            }

        } else if(card.cardType == CardType.Ability) {


            
        } else if(card.cardType == CardType.Factor) {



        } else if(card.cardType == CardType.Strike) {

            StatsHandler.instance.ChangeStat(card.strikedStat, -card.strikeValue);
            
        } else if(card.cardType == CardType.Crisis) {


            
        }

        Invoke(nameof(UseCardCooldown), 2f);
    }
    private void UseCardCooldown() {
        canUseCard = true;
    }

    private void UseCardAnimation() {
        GameObject cardObject = focusedGameCard.cardObject;

        cardObject.GetComponent<EventTrigger>().enabled = false;
        cardObject.GetComponent<CardPrefab>().ButtonExit();

        cardObject.transform.SetParent(deckHolder.transform.parent, true);
        
        cardObject.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, 1f).SetEase(Ease.OutExpo);
        cardObject.transform.DOLocalRotate(Vector3.zero, 1f).SetEase(Ease.OutExpo);
        cardObject.transform.DOScale(2f, 1f).SetEase(Ease.OutExpo).OnComplete(() => {

            float readTime = 2f;
            cardObject.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, 400), 1f).SetEase(Ease.InExpo).SetDelay(readTime);
            cardObject.transform.DOLocalRotate(new Vector3(-90, 0, 0), 1f).SetEase(Ease.InExpo).SetDelay(readTime);
            cardObject.transform.DOScale(0f, 1f).SetEase(Ease.InExpo).SetDelay(readTime).OnComplete(() => {

                RemoveCard(cardObject);

            });

        });

        cardObject.GetComponent<CardPrefab>().UseCard();
        cardObject.transform.Find("bar").GetComponent<Image>().DOFillAmount(0f, 3f).SetEase(Ease.Linear);
    }

    public IEnumerator EnterDrawPhase() {

        canUseCard = false;

        drawCardScreen.gameObject.SetActive(true);
        drawCardScreen.DOFade(1f, 1f);

        drawCardMainText.DOFade(1f, 0.5f);
        keepButtonText.transform.parent.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);
        redrawButtonText.transform.parent.GetComponent<CanvasGroup>().DOFade(1f, 0.5f);

        yield return new WaitForSeconds(1f);

        CreatePile();

        DrawCard();

    }

    float animationSpeed = 0.5f;
    public IEnumerator ExitDrawPhase() {

        DOTween.Kill(drawCardRarityText.gameObject);
        drawCardRarityText.DOFade(0f, 0.5f);
        drawCardMainText.DOFade(0f, 0.5f);
        keepButtonText.transform.parent.GetComponent<CanvasGroup>().DOFade(0f, 0.5f);
        redrawButtonText.transform.parent.GetComponent<CanvasGroup>().DOFade(0f, 0.5f);

        yield return new WaitForSeconds(1f);

        deckHolder.GetComponent<RectTransform>().DOAnchorPosY(375, 0.5f).SetEase(Ease.OutBack);
        deckHolder.DOScale(1.5f, 0.5f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(2f);
        
        foreach(GameCard gameCard in deckCards) {
            GameObject cardObject = gameCard.cardObject;

            cardObject.GetComponent<EventTrigger>().enabled = false;
            cardObject.GetComponent<CardPrefab>().ButtonExit();

            cardObject.transform.Find("RarityDisplay").GetComponent<Image>().DOFade(0f, animationSpeed);
            cardObject.transform.Find("Back").Find("MainIcon").GetComponent<Image>().DOFade(0f, animationSpeed).OnComplete(() => {
                cardObject.transform.Find("Back").Find("MainIcon").GetComponent<Image>().sprite = GetCardTypeIcon(CardType.None);
                cardObject.transform.Find("Back").Find("MainIcon").GetComponent<Image>().DOFade(1f, animationSpeed);
            });

            // //if the card is a risk card, set the icon to a random card type icon from the deck
            // if(gameCard.card.cardRarity == CardRarity.Risk) {
            //     List<Card> nonRiskCards = deckCards.Where(x => x.card.cardRarity != CardRarity.Risk).ToList().ConvertAll(x => x.card);
            //     Card randomCard = nonRiskCards[Random.Range(0, nonRiskCards.Count)];
            //     CardType randomType = (randomCard.cardType == CardType.Boost || randomCard.cardType == CardType.Strike) ? CardType.Boost : randomCard.cardType;

            //     cardObject.transform.Find("Back").Find("MainIcon").GetComponent<Image>().DOFade(0f, animationSpeed).OnComplete(() => {
            //         cardObject.transform.Find("Back").Find("MainIcon").GetComponent<Image>().sprite = GetCardTypeIcon(randomType);
            //         cardObject.transform.Find("Back").Find("MainIcon").GetComponent<Image>().DOFade(1f, animationSpeed);
            //     });
            // }

        }

        yield return new WaitForSeconds(animationSpeed);

        //SHUFFLE

        foreach(GameCard gameCard in deckCards) {
            GameObject cardObject = gameCard.cardObject;
            cardObject.transform.DOLocalMove(Vector3.zero, animationSpeed).SetEase(Ease.OutBack);
            cardObject.transform.DORotate(Vector3.zero, animationSpeed).SetEase(Ease.OutBack);
        }

        yield return new WaitForSeconds(animationSpeed);

        for(int i = deckCards.Count - 1; i >= 0; i--) {
            GameObject cardObject = deckCards[i].cardObject;
            //int offset = (Random.Range(0f, 1f) > 0.5f ? 1 : -1) * 100;
            int offset = (i % 2 == 0 ? 1 : -1) * 100;
            Vector3 targetPos = Vector3.zero + new Vector3(offset, 0, 0);
            cardObject.transform.DOLocalMove(targetPos, animationSpeed).SetEase(Ease.OutBack);

            yield return new WaitForSeconds(0.1f);
        }

        deckCards = deckCards.OrderBy(x => Random.Range(0f, 1f)).ToList();
        for(int i = 0; i < deckCards.Count; i++) {
            deckCards[i].cardObject.transform.DOLocalMove(Vector3.zero, animationSpeed).SetEase(Ease.OutBack);
        }

        yield return new WaitForSeconds(animationSpeed);

        for(int i = 0; i < deckCards.Count; i++) {
            deckCards[i].cardObject.transform.SetSiblingIndex(i);

            deckCards[i].cardObject.GetComponent<EventTrigger>().enabled = true;
        }

        ArrangeCards();

        yield return new WaitForSeconds(animationSpeed);

        deckHolder.GetComponent<RectTransform>().DOAnchorPosY(25, animationSpeed).SetEase(Ease.OutBack);
        deckHolder.DOScale(1f, animationSpeed).SetEase(Ease.OutBack);

        drawCardScreen.DOFade(0f, 1f).OnComplete(() => {
            drawCardScreen.gameObject.SetActive(false); 
        });
        
        //GAME HANDLER RESUME GAMEPLAY
        GameHandler.instance.ToggleTimeFreeze(false);

        canUseCard = true;

    }

    public void DrawCard() {

        drawingCard = true;

        focusedGameCard = pileCards.Last();

        drawCardRarityText.DOFade(0f, 0.2f).SetId(drawCardRarityText.gameObject).OnComplete(() => {
            drawCardRarityText.text = focusedGameCard.card.cardRarity.ToString();
            drawCardRarityText.color = GetCardRarityColor(focusedGameCard.card.cardRarity);
            drawCardRarityText.alpha = 0f;
            drawCardRarityText.DOFade(1f, 0.2f).SetId(drawCardRarityText.gameObject);
        });

        focusedGameCard.cardObject.transform.SetParent(drawCardScreen.transform, true);
        focusedGameCard.cardObject.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, 0.5f);
        focusedGameCard.cardObject.transform.DORotate(Vector3.zero, 0.5f);
        focusedGameCard.cardObject.transform.DOScale(Vector3.one * 1.5f, 0.5f).OnComplete(() => {
            drawingCard = false;
        });

        focusedGameCard.cardObject.transform.Find("Back").Find("MainIcon").GetComponent<Image>().DOFade(0f, 0.2f).OnComplete(() => {
            focusedGameCard.cardObject.transform.Find("Back").Find("MainIcon").GetComponent<Image>().sprite = GetCardTypeIcon(focusedGameCard.card.cardType);
            focusedGameCard.cardObject.transform.Find("Back").Find("MainIcon").GetComponent<Image>().DOFade(1f, 0.2f);
        });

    }

    public void KeepCard() {

        if(drawingCard) {
            return;
        }

        AddCard(focusedGameCard.card, focusedGameCard.cardObject);
        focusedGameCard.cardObject.transform.Find("RarityDisplay").gameObject.SetActive(true);
        focusedGameCard.cardObject.transform.Find("RarityDisplay").GetComponent<Image>().color = GetCardRarityColor(focusedGameCard.card.cardRarity);

        pileCards.Remove(pileCards.Last());

        if(pileCards.Count > 0 && deckCards.Count < 5) {

            DrawCard();

        } else {

            StartCoroutine(ExitDrawPhase());

        }

        keepButtonText.text = $"Keep ({deckCards.Count}/5)";

    }

    public void RedrawCard() {

        if(redraws >= 2 || drawingCard) {
            return;
        }

        GameObject temp = pileCards.Last().cardObject;
        temp.transform.DOScale(0f, 0.5f).OnComplete(() => {
            Destroy(temp);
        });
        pileCards.Remove(pileCards.Last());
        
        DrawCard();
        
        redraws++;
        redrawButtonText.text = $"Redraw ({redraws}/2)";
        redrawButtonText.transform.parent.GetComponent<CanvasGroup>().DOFade(redraws >= 2 ? 0.5f : 1f, 0.2f);

    }

    //-------------------------------------------------- CARD METHODS --------------------------------------------------

    private void CreatePile() {
        List<Card> randomCards = GetRandomCards();

        for(int i = 0; i < randomCards.Count; i++) {
            Card card = randomCards[i];

            GameObject cardObject = Instantiate(cardPrefab, pileHolder);
            cardObject.GetComponent<EventTrigger>().enabled = false;
            cardObject.GetComponent<CardPrefab>().Initialize(card);
            cardObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -(40 - pileCards.Count * 5));
            cardObject.transform.localRotation = Quaternion.Euler(50, 10, 0);

            pileCards.Add(new GameCard(card, cardObject));
        }
    }

    private List<Card> GetRandomCards() {
        List<Card> currentCards = new List<Card>();
        int randomRiskCards = Random.Range(2, 4);

        CardRarity rarity = CardRarity.None;
        for(int i = 0; i < totalPileCards; i++) {

            if(currentCards.Where(x => x.cardRarity == CardRarity.Risk).Count() < randomRiskCards) {

                rarity = CardRarity.Risk;

            } else {

                int randomNumber = Random.Range(0, 100);
                if(randomNumber > 60) {
                    rarity = CardRarity.Common;
                } else if(randomNumber > 40) {
                    rarity = CardRarity.Uncommon;
                } else if(randomNumber > 25) {
                    rarity = CardRarity.Rare;
                } else if(randomNumber > 10) {
                    rarity = CardRarity.Epic;
                } else {
                    rarity = CardRarity.Legendary;
                }

            }

            List<Card> focusedCards = allCards.Where(x => x.cardRarity == rarity).ToList();
            Card focusedCard = focusedCards[Random.Range(0, focusedCards.Count)];
            
            //if the card is a buildable, then give a higher chance of a helpful buildable being given instead of a random one
            if(focusedCard.cardType == CardType.Buildable) {

                bool giveHelpfulBuildable = Random.Range(0, 2) == 0;

                if(giveHelpfulBuildable) {

                    Dictionary<Stat, int> statChanges = new Dictionary<Stat, int> {
                        { Stat.Materials, StatsHandler.instance.GetTotalStatChange(Stat.Materials) },
                        { Stat.Food, StatsHandler.instance.GetTotalStatChange(Stat.Food) },
                        { Stat.Gold, StatsHandler.instance.GetTotalStatChange(Stat.Gold) }
                    };
                    Stat lowestStat = statChanges.OrderBy(x => x.Value).First().Key;
                    List<Card> helpfulBuildableCards = focusedCards.Where(x => x.cardType == CardType.Buildable && x.buildable.buildableFocus == lowestStat).ToList();
 
                    //set the focused card to a random card in the possible buildable cards
                    if(helpfulBuildableCards.Count == 0) {
                        List<Card> cards = allCards.Where(x => x.cardType == CardType.Buildable && x.buildable.buildableFocus == lowestStat).ToList();
                        focusedCard = cards[Random.Range(0, cards.Count)];
                    } else {
                        focusedCard = helpfulBuildableCards[Random.Range(0, helpfulBuildableCards.Count)];
                    }

                }

                
                
            }
            //if the card is an upgrade, then make sure the prerequisite exists, otherwise choose another random upgrade card
            else if(focusedCard.cardType == CardType.Upgrade) {
                
                List<Card> focusedUpgradeCards = focusedCards.Where(x => x.cardType == CardType.Upgrade).ToList();
                Buildable requiredBuildable = PlacementHandler.instance.GetPreviousUpgrade(focusedCard.upgrade);
                List<Buildable> placedBuildables = PlacementHandler.instance.GetPlacedBuildables().ConvertAll(x => x.buildable);

                //if the prerequisite isn't met then change the card
                if(!placedBuildables.Contains(requiredBuildable)) {
                    //filter the possible upgrade cards to only have required buildables that are already placed

                    focusedUpgradeCards = focusedUpgradeCards.Where(x => placedBuildables.Contains(PlacementHandler.instance.GetPreviousUpgrade(x.upgrade))).ToList();
                    
                    //set the focused card to a random card in the possible upgrade cards
                    if(focusedUpgradeCards.Count == 0) {
                        List<Card> cards = allCards.Where(x => placedBuildables.Contains(PlacementHandler.instance.GetPreviousUpgrade(x.upgrade))).ToList();
                        focusedCard = cards[Random.Range(0, cards.Count)];
                    } else {
                        focusedCard = focusedUpgradeCards[Random.Range(0, focusedUpgradeCards.Count)];
                    }

                }

            }

            currentCards.Add(focusedCard);

        }

        currentCards = currentCards.OrderBy(x => Random.Range(0f, 1f)).ToList();

        return currentCards;
        
    }

    public void CreateDeck() {
        List<GameCard> cards = pileCards.Take(5).ToList();
        pileCards.Clear();
        CreatePile();

        for(int i = 0; i < 5; i++) {
            AddCard(cards[i].card, cards[i].cardObject);
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

            deckCards[i].cardObject.GetComponent<RectTransform>().DOAnchorPos(cardTransform.pos, 1f).SetEase(Ease.OutBack);
            deckCards[i].cardObject.transform.DOLocalRotate(cardTransform.rot, 1f).SetEase(Ease.OutBack);

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

    public void AddCard(Card card, GameObject pileCardObject) {

        GameObject cardObject = pileCardObject;
        cardObject.transform.SetParent(deckHolder, true);
        cardObject.transform.DOScale(1f, 1f).OnComplete(() => {
            cardObject.GetComponent<EventTrigger>().enabled = true;
        });
        
        GameCard gameCard = new GameCard(card, cardObject);

        deckCards.Add(gameCard);
        cardObject.GetComponent<CardPrefab>().Initialize(card);

        ArrangeCards();
    }

    public void RemoveCard(GameObject cardObject) {
        GameCard targetGameCard = GetGameCardFromCardObject(cardObject);

        deckCards.Remove(targetGameCard);
        Destroy(cardObject);

        ArrangeCards();
    }

    //-------------------------------------------------- UTILITY METHODS --------------------------------------------------

    private GameCard GetGameCardFromCardObject(GameObject cardObject) {
        return deckCards.FirstOrDefault(x => x.cardObject == cardObject);
    }

    public Sprite GetCardTypeIcon(CardType type) {
        List<CardType> cardTypes = new List<CardType>((CardType[])System.Enum.GetValues(typeof(CardType)));
        return typeIcons[cardTypes.IndexOf(type)];
    }

    public Color GetCardRarityColor(CardRarity rarity) {
        List<CardRarity> cardRarities = new List<CardRarity>((CardRarity[])System.Enum.GetValues(typeof(CardRarity)));
        return rarityColors[cardRarities.IndexOf(rarity)];
    }

    //-------------------------------------------------- UI --------------------------------------------------

    public void ToggleDeckVisibility(bool value) {
        activeDeck = value;

        if(activeDeck) {
            deckHolder.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, 25), 0.5f).SetEase(Ease.OutBack);
        } else {
            deckHolder.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, -75), 0.5f).SetEase(Ease.InBack);
        }
    }

    public void ButtonDown(int index) {
        buttonTransforms[index].DOScale(Vector3.one * 0.9f, 0.2f);
    }
    public void ButtonUp(int index) {
        buttonTransforms[index].DOScale(Vector3.one, 0.2f);
    }
    public void ButtonEnter(int index) {
        buttonTransforms[index].DOScale(Vector3.one * 1.1f, 0.2f);

        int rotationZ = index == 0 ? 5 : -5;
        //tempCardObject.transform.DORotate(new Vector3(0, 0, rotationZ), 1f);
    }
    public void ButtonExit(int index) {
        buttonTransforms[index].DOScale(Vector3.one, 0.2f);

        //tempCardObject.transform.DORotate(Vector3.zero, 1f);
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