using TMPro;
using UnityEngine;
using DG.Tweening;

public class CardPrefab : MonoBehaviour, Animatable
{
    [Header("Card")]
    [SerializeField] private Card card;

    private bool animating = true;

    [Header("References")]
    [SerializeField] private GameObject back;
    [SerializeField] private TMP_Text description;

    [HideInInspector] public Vector2 defaultAnchorPos;
    [HideInInspector] public float defaultRotationZ;

    //-------------------------------------------------- CARD --------------------------------------------------

    public void Initialize(Card _card) {
        card = _card;

        description.text = card.description;



        Invoke(nameof(AnimationEnd), 1f);
    }

    private void AnimationEnd() {
        animating = false;
    }

    public void DestroyCard() {
        ButtonExit();
        animating = true;
    }

    public void ShowCard() {
        transform.localRotation = Quaternion.Euler(0, 180, -defaultRotationZ);
        transform.DOLocalRotate(new Vector3(0, 0, defaultRotationZ), 1f).OnUpdate(() => {
            if(transform.localEulerAngles.y <= 90 && back.activeSelf) {
                back.SetActive(false);
            }
        });
    }

    public void HideCard() {
        transform.DOLocalRotate(new Vector3(0, 180, -defaultRotationZ), 1f).OnUpdate(() => {
            if(transform.localEulerAngles.y >= 90 && !back.activeSelf) {
                back.SetActive(true);
            }
        });
    }

    //-------------------------------------------------- BUTTON EVENTS --------------------------------------------------

    public void ButtonClick() {
        CardsHandler.instance.ShowCardScreen(gameObject);
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
