using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using Unity.Collections;

public class DecisionChoice : MonoBehaviour
{
    public ChoiceData choiceData;

    [Header("References")]
    [SerializeField] private Image buildableIcon;
    [SerializeField] private TMP_Text buildableName;
    //[SerializeField] private TMP_Text buildableAmount;
    [SerializeField] private TMP_Text choiceDescription;
    [SerializeField] private Transform environmentHolder;
    [SerializeField] private Transform happinessHolder;
    [SerializeField] private Transform economyHolder;

    private Color32 positiveColor = new Color32(195, 250, 216, 255);
    private Color32 negativeColor = new Color32(245, 201, 196, 255);
    private Color32 disabledColor = new Color32(0, 0, 0, 50);

    public void Initialize(ChoiceData _choiceData, int index)
    {
        choiceData = _choiceData;

        Buildable buildable = PlacementHandler.instance.GetBuildable(choiceData.buildable);
        buildableIcon.sprite = buildable.icon;
        buildableName.text = buildable.name;
        choiceDescription.text = choiceData.description;
        
        //update stat effects
        if(choiceData.stat1 == "Environment" || choiceData.stat2 == "Environment") {
            if(choiceData.stat1 == "Environment") {
                environmentHolder.Find("value").GetComponent<TMP_Text>().text = choiceData.effect1;
                environmentHolder.gameObject.SetActive(true);
            } else {
                environmentHolder.Find("value").GetComponent<TMP_Text>().text = choiceData.effect2;
                environmentHolder.gameObject.SetActive(true);
            }
        }
        if(choiceData.stat1 == "Happiness" || choiceData.stat2 == "Happiness") {
            if(choiceData.stat1 == "Happiness") {
                happinessHolder.Find("value").GetComponent<TMP_Text>().text = choiceData.effect1;
                happinessHolder.gameObject.SetActive(true);
            } else {
                happinessHolder.Find("value").GetComponent<TMP_Text>().text = choiceData.effect2;
                happinessHolder.gameObject.SetActive(true);
            }
        }
        if(choiceData.stat1 == "Economy" || choiceData.stat2 == "Economy") {
            if(choiceData.stat1 == "Economy") {
                economyHolder.Find("value").GetComponent<TMP_Text>().text = choiceData.effect1;
                economyHolder.gameObject.SetActive(true);
            } else {
                economyHolder.Find("value").GetComponent<TMP_Text>().text = choiceData.effect2;
                economyHolder.gameObject.SetActive(true);
            }
        }

        //START ANIMATION
        StartCoroutine(FadeSequence(index));
    }
    private IEnumerator FadeSequence(int index) {
        yield return new WaitForSeconds(index * 2f);

        gameObject.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

        if(choiceData.stat1 == "Environment" || choiceData.stat2 == "Environment") {
            environmentHolder.GetComponent<ContentSizeFitter>().enabled = true;         
        }
        if(choiceData.stat1 == "Happiness" || choiceData.stat2 == "Happiness") {
            happinessHolder.GetComponent<ContentSizeFitter>().enabled = true;        
        }
        if(choiceData.stat1 == "Economy" || choiceData.stat2 == "Economy") {
            economyHolder.GetComponent<ContentSizeFitter>().enabled = true;
        }

        //reset main content size fitter
        environmentHolder.parent.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        yield return null;
        environmentHolder.parent.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.MinSize;
    }

    public void ButtonClick() {
        DecisionHandler.instance.SelectChoice(choiceData);
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
