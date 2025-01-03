using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using Unity.Collections;

public class DecisionChoice : MonoBehaviour
{
    public Decision decision;

    [Header("References")]
    [SerializeField] private TMP_Text decisionName;
    [SerializeField] private Image buildableIcon;
    [SerializeField] private TMP_Text buildableAmount;
    [SerializeField] private TMP_Text buildableDescription;
    [SerializeField] private Transform environmentBarHolder;
    [SerializeField] private Transform happinessBarHolder;
    [SerializeField] private Transform economyBarHolder;

    [SerializeField] private GameObject statBarPrefab;
    private Color32 positiveColor = new Color32(195, 250, 216, 255);
    private Color32 negativeColor = new Color32(245, 201, 196, 255);
    private Color32 disabledColor = new Color32(0, 0, 0, 50);

    public void Initialize(Decision _decision, int index)
    {
        decision = _decision;

        decisionName.text = decision.name;
        buildableIcon.sprite = decision.buildable.icon;
        buildableAmount.text = decision.amount.ToString();
        buildableDescription.text = decision.buildable.description;
        
        //CREATE EFFECT BARS
        CreateEffectBar(environmentBarHolder, decision.buildable.environmentEffect);
        CreateEffectBar(happinessBarHolder, decision.buildable.happinessEffect);
        CreateEffectBar(economyBarHolder, decision.buildable.economyEffect);

        //START ANIMATION
        StartCoroutine(FadeSequence(index));
    }
    private void CreateEffectBar(Transform parent, int value) {
        for(int i = -3; i <= 3; i++) {
            Image bar = Instantiate(statBarPrefab, parent).GetComponent<Image>();
            Color32 color = disabledColor;

            if(i == 0) {
                color = new Color32(255, 255, 255, 255);
            } else if(i < 0 && i >= value) {
                color = negativeColor;
            } else if(i > 0 && i <= value) {
                color = positiveColor;
            }

            bar.color = color;
        }
    }
    private IEnumerator FadeSequence(int index) {
        yield return new WaitForSeconds(index * 2f);

        gameObject.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

        //SET HOLDERS TO ACTIVE TO REFRESH CONTEN SIZE FITTER

        float _delay = 0.5f;
        float _animationTime = 0.5f;

        yield return new WaitForSeconds(_delay);
        environmentBarHolder.parent.GetComponent<ContentSizeFitter>().enabled = true;
        environmentBarHolder.parent.GetComponent<CanvasGroup>().DOFade(1, _animationTime);

        yield return new WaitForSeconds(_delay);
        happinessBarHolder.parent.GetComponent<ContentSizeFitter>().enabled = true;
        happinessBarHolder.parent.GetComponent<CanvasGroup>().DOFade(1, _animationTime);

        yield return new WaitForSeconds(_delay);
        economyBarHolder.parent.GetComponent<ContentSizeFitter>().enabled = true;
        economyBarHolder.parent.GetComponent<CanvasGroup>().DOFade(1, _animationTime);
    }

    public void ButtonClick() {
        DecisionHandler.instance.SelectChoice(decision);
    }
    public void ButtonEnter() {
        transform.DOScale(Vector3.one * 1.05f, 0.5f);
    }
    public void ButtonExit() {
        transform.DOScale(Vector3.one, 0.5f);
    }
    public void ButtonDown() {
        transform.DOScale(Vector3.one * 0.95f, 0.5f);
    }
}
