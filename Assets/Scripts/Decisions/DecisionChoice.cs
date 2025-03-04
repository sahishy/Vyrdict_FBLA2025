using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using Unity.Collections;

public class DecisionChoice : MonoBehaviour
{
    public NormalChoiceData choiceData;

    [Header("References")]
    [SerializeField] private TMP_Text choiceDescription;
    [SerializeField] private Transform suppliesHolder;
    [SerializeField] private Transform foodHolder;
    [SerializeField] private Transform goldHolder;
    [SerializeField] private Transform fateHolder;

    private Color32 positiveColor = new Color32(195, 250, 216, 255);
    private Color32 negativeColor = new Color32(245, 201, 196, 255);
    private Color32 disabledColor = new Color32(0, 0, 0, 50);

    public void Initialize(NormalChoiceData _choiceData, int index)
    {
        choiceData = _choiceData;

        choiceDescription.text = _choiceData.description;
        
        //update stat effects
        if(choiceData.stat1 == "supplies" || choiceData.stat2 == "supplies") {
            if(choiceData.stat1 == "supplies") {
                suppliesHolder.Find("value").GetComponent<TMP_Text>().text = choiceData.effect1.ToString();
                suppliesHolder.gameObject.SetActive(true);
            } else {
                suppliesHolder.Find("value").GetComponent<TMP_Text>().text = choiceData.effect2.ToString();
                suppliesHolder.gameObject.SetActive(true);
            }
        }
        if(choiceData.stat1 == "food" || choiceData.stat2 == "food") {
            if(choiceData.stat1 == "food") {
                foodHolder.Find("value").GetComponent<TMP_Text>().text = choiceData.effect1.ToString();
                foodHolder.gameObject.SetActive(true);
            } else {
                foodHolder.Find("value").GetComponent<TMP_Text>().text = choiceData.effect2.ToString();
                foodHolder.gameObject.SetActive(true);
            }
        }
        if(choiceData.stat1 == "gold" || choiceData.stat2 == "gold") {
            if(choiceData.stat1 == "gold") {
                goldHolder.Find("value").GetComponent<TMP_Text>().text = choiceData.effect1.ToString();
                goldHolder.gameObject.SetActive(true);
            } else {
                goldHolder.Find("value").GetComponent<TMP_Text>().text = choiceData.effect2.ToString();
                goldHolder.gameObject.SetActive(true);
            }
        }
        fateHolder.Find("value").GetComponent<TMP_Text>().text = choiceData.fateEffect.ToString();

        //START ANIMATION
        StartCoroutine(FadeSequence(index));
    }
    private IEnumerator FadeSequence(int index) {
        yield return new WaitForSeconds(index * 2f);

        gameObject.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

        if(choiceData.stat1 == "Supplies" || choiceData.stat2 == "Supplies") {
            suppliesHolder.GetComponent<ContentSizeFitter>().enabled = true;         
        }
        if(choiceData.stat1 == "Food" || choiceData.stat2 == "Food") {
            foodHolder.GetComponent<ContentSizeFitter>().enabled = true;        
        }
        if(choiceData.stat1 == "Gold" || choiceData.stat2 == "Gold") {
            goldHolder.GetComponent<ContentSizeFitter>().enabled = true;
        }

        //reset main content size fitter
        suppliesHolder.parent.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        yield return null;
        suppliesHolder.parent.GetComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.MinSize;
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
