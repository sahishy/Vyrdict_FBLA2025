using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class CardPrefab : MonoBehaviour, Animatable
{
    [Header("Card")]
    [SerializeField] private Card card;

    private bool animating = true;

    [Header("References")]
    [SerializeField] private GameObject back;
    [SerializeField] private Image mainIcon;
    [SerializeField] private TMP_Text description;
    [SerializeField] private Transform raritiesHolder;
    [SerializeField] private Transform typeIconsHolder;

    [HideInInspector] public Vector2 defaultAnchorPos;
    [HideInInspector] public float defaultRotationZ;

    //-------------------------------------------------- CARD --------------------------------------------------

    public void Initialize(Card _card, bool displayTypeIconOnBack = false) {
        card = _card;

        if(displayTypeIconOnBack) {
            mainIcon.sprite = CardsHandler.instance.GetCardTypeIcon(card.cardType);
        }

        description.text = card.description;

        for(int i = 0; i < raritiesHolder.childCount; i++) {
            raritiesHolder.GetChild(i).GetComponent<TMP_Text>().text = card.cardRarity.ToString().ToUpper();
            raritiesHolder.GetChild(i).GetComponent<TMP_Text>().color = CardsHandler.instance.GetCardRarityColor(card.cardRarity);
            typeIconsHolder.GetChild(i).GetComponent<Image>().sprite = CardsHandler.instance.GetCardTypeIcon(card.cardType);
        }


        Invoke(nameof(AnimationEnd), 1f);
    }

    private void AnimationEnd() {
        animating = false;
    }

    public void DestroyCard() {
        ButtonExit();
        animating = true;
    }

    public void UseCard() {
        ShowCard();
        GameHandler.instance.currentFocusedAnimatable = null;
    }

    private void ShowCard() {
        transform.localRotation = Quaternion.Euler(0, 180, 0);
        transform.DOLocalRotate(new Vector3(0, 0, 0), 1f).OnUpdate(() => {
            if(transform.localEulerAngles.y <= 90 && back.activeSelf) {
                back.SetActive(false);
            }
        });
    }

    //-------------------------------------------------- BUTTON EVENTS --------------------------------------------------

    public void ButtonClick() {
        CardsHandler.instance.TryUseCard(gameObject);
    }

    public void ButtonEnter() {
        if(animating) {
            return;
        }

        GameHandler.instance.currentFocusedAnimatable = this;

        transform.GetComponent<RectTransform>().DOAnchorPos(defaultAnchorPos + new Vector2(0, 15), 0.2f);
        transform.DOScale(1.1f, 0.2f);

    }
    public void ButtonExit() {
        if(animating) {
            return;
        }

        transform.GetComponent<RectTransform>().DOAnchorPos(defaultAnchorPos, 0.2f);
        transform.DOScale(1f, 0.2f);
    }

    public void AnimatableExit()
    {
        transform.GetComponent<RectTransform>().DOAnchorPos(defaultAnchorPos, 0.2f);
        transform.DOScale(1f, 0.2f);
    }

}
