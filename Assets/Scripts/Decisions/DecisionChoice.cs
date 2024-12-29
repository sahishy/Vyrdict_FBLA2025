using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DecisionChoice : MonoBehaviour
{
    public Decision decision;

    [Header("References")]
    [SerializeField] private TMP_Text decisionName;
    [SerializeField] private Image decisionIcon;
    [SerializeField] private TMP_Text decisionAmount;
    [SerializeField] private TMP_Text decisionDescription;
    [SerializeField] private TMP_Text environmentEffect;
    [SerializeField] private TMP_Text communityEffect;
    [SerializeField] private TMP_Text economyEffect;

    public void Initialize(Decision _decision)
    {
        decision = _decision;

        decisionName.text = decision.buildable.name;
        decisionIcon.sprite = decision.buildable.icon;
        decisionAmount.text = decision.amount.ToString();
        decisionDescription.text = decision.buildable.description;

        environmentEffect.text = IntToEffect(decision.buildable.environmentEffect);
        communityEffect.text = IntToEffect(decision.buildable.communityEffect);
        economyEffect.text = IntToEffect(decision.buildable.economyEffect);

    }
    private string IntToEffect(int number) {
        string result = "";
        if(number < 0) {
            for(int i = 0; i < Math.Abs(number); i++) {
                result += "-";
            }
        } else if(number == 0) {
            result = "o";
        } else {
            for(int i = 0; i < number; i++) {
                result += "+";
            }
        }
        return result;
    }

    public void ButtonClick() {
        DecisionHandler.instance.SelectChoice(decision);
    }
    public void ButtonEnter() {
        transform.DOScale(Vector3.one * 1.2f, 0.2f);
    }
    public void ButtonExit() {
        transform.DOScale(Vector3.one, 0.2f);
    }
}
