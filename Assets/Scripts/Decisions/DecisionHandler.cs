using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecisionHandler : MonoBehaviour
{
    public static DecisionHandler instance;

    public bool inDecisionMode = false;
    private bool cooldown = false;

    private List<Decision> allDecisions = new List<Decision>();

    [Header("References")]
    [SerializeField] private GameObject gameplayScreen;
    [SerializeField] private GameObject decisionScreen;

    [SerializeField] private TMP_Text weekCounter;
    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_Text scenario;

    [SerializeField] private TMP_Text eventLabel;
    [SerializeField] private TMP_Text eventEffect;

    [SerializeField] private RectTransform weekStatsHolder;
    [SerializeField] private Image environmentBar;
    [SerializeField] private Image happinessBar;
    [SerializeField] private Image economyBar;

    [SerializeField] private RectTransform weekStatsTextHolder;
    [SerializeField] private TMP_Text environmentText;
    [SerializeField] private TMP_Text happinessText;
    [SerializeField] private TMP_Text economyText;

    [SerializeField] private GameObject decisionChoiceHolder;
    [SerializeField] private GameObject decisionChoicePrefab;


    void Awake() {
        instance = this;

        allDecisions = Resources.LoadAll<Decision>("Decisions").ToList();
    }

    //--------------------------------------DECISIONS--------------------------------------

    //Called in the game loop at the start of each week, or 7 day period
    public void ProposeDecision() {
        inDecisionMode = true;

        cooldown = true;
        Invoke(nameof(DecisionCooldown), 1f);

        decisionScreen.SetActive(true);
        decisionScreen.GetComponent<CanvasGroup>().DOFade(1, 1f);
        gameplayScreen.GetComponent<CanvasGroup>().DOFade(0, 1f).OnComplete(() => {
            gameplayScreen.SetActive(false);
        });

        //CLOSE ACTIVE PROCESSES
        if(PlacementHandler.instance.inPlacementMode) {
            PlacementHandler.instance.ExitPlacementMode();
        }
        if(DialogueHandler.instance.activeDialogue) {
            DialogueHandler.instance.CloseDialogue();
        }

        //STARTING ANIMATION
        StartCoroutine(DecisionMakingAnimation());
    }
    //Creates choices from ML generated scenario
    private void ChoiceCreation(ResponseData response) {
        

        for(int i = 0; i < 3; i++) {
            ChoiceData choiceData;
            if (i == 0) {
                choiceData = response.choices.choice1;
            } else if(i == 1) {
                choiceData = response.choices.choice2;
            } else {
                choiceData = response.choices.choice3;
            }

            DecisionChoice choice = Instantiate(decisionChoicePrefab, decisionChoiceHolder.transform).GetComponent<DecisionChoice>();

            choice.Initialize(choiceData, i);
        }
    }
    //Resets cooldown to prevent player from accidentally selecting a choice the second it appears
    private void DecisionCooldown() {
        cooldown = false;
    }
    //Called when player selects a choice in the decision screen
    public void SelectChoice(ChoiceData choiceData) {
        if(cooldown) {
            return;
        }

        //unfocus any animatable
        GameHandler.instance.currentFocusedAnimatable?.AnimatableExit();

        //closing animation
        decisionScreen.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() => {
            decisionScreen.SetActive(false); 
        });
        gameplayScreen.SetActive(true);
        gameplayScreen.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

        Invoke(nameof(ResetDecisionMakingAnimation), 1f);

        //CHANGE STATS AND ADD ITEM
        StatsHandler.instance.ChangeStat(StatsHandler.instance.GetStatByName(choiceData.stat1), int.Parse(choiceData.effect1));
        StatsHandler.instance.ChangeStat(StatsHandler.instance.GetStatByName(choiceData.stat2), int.Parse(choiceData.effect2));
        InventoryHandler.instance.TryAddItem(PlacementHandler.instance.GetBuildable(choiceData.buildable), 1);

        inDecisionMode = false;
        cooldown = false;

        //RESUME THE WEEK
        GameHandler.instance.StartWeek();
    }

    //--------------------------------------UI--------------------------------------

    //Animation that handles showing all the important factors one at a time
    private IEnumerator DecisionMakingAnimation() {
        
        //---------------------------------FETCH GENERATED ML DATA----------------------------------------------

        ResponseData response = null;
        StartCoroutine(MLDataHandler.instance.FetchMLResponse());
        
        //------------------------------------------------------------------------------------------------------

        weekCounter.text = $"Week {GameHandler.instance.currentWeek - 1}";
        weekCounter.transform.DOScale(0.8f, 1f);

        //wait for screen to fade in, give player time to process the current week
        yield return new WaitForSeconds(1f);

        weekCounter.text = $"Week {GameHandler.instance.currentWeek}";
        //weekCounter.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 0, 0f);
        weekCounter.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);

        yield return new WaitForSeconds(1f);

        //fade out week counter
        weekCounter.DOFade(0, 1f);
        yield return new WaitForSeconds(1f);

        //show stats of the week
        label.DOFade(1f, 3f);
        weekStatsHolder.GetComponent<CanvasGroup>().DOFade(1f, 1f);
        weekStatsTextHolder.GetComponent<CanvasGroup>().DOFade(1f, 1f);

        yield return new WaitForSeconds(1f);

        float animationTime = 3f;
        float randomEnvironmentAnimationTime = animationTime + Random.Range(0f, 1f);
        float randomHappinessAnimationTime = animationTime + Random.Range(0f, 1f);
        float randomEconomyAnimationTime = animationTime + Random.Range(0f, 1f);

        environmentBar.DOFillAmount(StatsHandler.instance.GetStat(Stat.Materials) / 100f, randomEnvironmentAnimationTime).SetEase(Ease.OutExpo);
        happinessBar.DOFillAmount(StatsHandler.instance.GetStat(Stat.Food) / 100f, randomHappinessAnimationTime).SetEase(Ease.OutExpo);
        economyBar.DOFillAmount(StatsHandler.instance.GetStat(Stat.Gold) / 100f, randomEconomyAnimationTime).SetEase(Ease.OutExpo);

        //show raw stat values, do counting animation
        float _environmentValue = 0f;
        float _happinessValue = 0f;
        float _economyValue = 0f;

        DOTween.To(x => _environmentValue = x, 0f, StatsHandler.instance.GetStat(Stat.Materials), randomEnvironmentAnimationTime)
        .SetEase(Ease.OutExpo).OnUpdate(() => environmentText.text = Mathf.RoundToInt(_environmentValue).ToString());
        DOTween.To(x => _happinessValue = x, 0f, StatsHandler.instance.GetStat(Stat.Food), randomHappinessAnimationTime)
        .SetEase(Ease.OutExpo).OnUpdate(() => happinessText.text = Mathf.RoundToInt(_happinessValue).ToString());
        DOTween.To(x => _economyValue = x, 0f, StatsHandler.instance.GetStat(Stat.Gold), randomEconomyAnimationTime)
        .SetEase(Ease.OutExpo).OnUpdate(() => economyText.text = Mathf.RoundToInt(_economyValue).ToString());

        yield return new WaitForSeconds(3f);

        //show status color of raw stats (red bad, white neutral, green good)
        environmentText.DOColor(StatsHandler.instance.GetStatusColor(StatsHandler.instance.GetStat(Stat.Materials)), 1f);
        happinessText.DOColor(StatsHandler.instance.GetStatusColor(StatsHandler.instance.GetStat(Stat.Food)), 1f);
        economyText.DOColor(StatsHandler.instance.GetStatusColor(StatsHandler.instance.GetStat(Stat.Gold)), 1f);

        //give extra time to look at stats
        yield return new WaitForSeconds(2f);

        //---------------------------------WAIT UNTIL GENERATED ML DATA LOADS------------------------------------

        yield return new WaitUntil(() => MLDataHandler.instance.currentData != null);

        response = MLDataHandler.instance.currentData;

        MLDataHandler.instance.currentData = null;
        
        //-------------------------------------------------------------------------------------------------------

        //hide stats of the week
        label.DOFade(0, 1f);
        yield return new WaitForSeconds(1f);

        weekStatsHolder.DOAnchorPos(new Vector2(0, 315), 2f).SetEase(Ease.InOutCubic);
        weekStatsHolder.transform.DOScale(0.575f, 2f).SetEase(Ease.InOutCubic);
        weekStatsTextHolder.DOAnchorPos(new Vector2(0, 280), 2f).SetEase(Ease.InOutCubic);
        weekStatsTextHolder.transform.DOScale(0.575f, 2f).SetEase(Ease.InOutCubic);
        yield return new WaitForSeconds(2f);

        //propose scenario
        yield return StartCoroutine(ScenarioTextAnimation(response.scenario));

        //yield return new WaitForSeconds(1);

        //propose choices
        label.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);
        label.text = "What will you do?";
        label.DOFade(1f, 3f);

        //yield return new WaitForSeconds(1f);

        //CREATING CHOICES
        ChoiceCreation(response);
    }
    //Reset the changes made by animation in DecisionMakingAnimation()
    private void ResetDecisionMakingAnimation() {
        weekCounter.text = "";
        weekCounter.alpha = 1f;

        label.alpha = 0;
        label.text = "Here are your stats for the week:";
        label.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 75);
        scenario.text = "";

        weekStatsHolder.anchoredPosition = new Vector2(0, 0);
        weekStatsHolder.GetComponent<CanvasGroup>().alpha = 0;
        weekStatsHolder.transform.localScale = Vector3.one;
        environmentBar.fillAmount = 0;
        happinessBar.fillAmount = 0;
        economyBar.fillAmount = 0;

        weekStatsTextHolder.anchoredPosition = new Vector2(0, -75);
        weekStatsTextHolder.GetComponent<CanvasGroup>().alpha = 0;
        weekStatsTextHolder.transform.localScale = Vector3.one;
        environmentText.text = "";
        environmentText.color = Color.white;
        happinessText.text = "";
        happinessText.color = Color.white;
        economyText.text = "";
        economyText.color = Color.white;

        foreach(Transform child in decisionChoiceHolder.transform) {
            Destroy(child.gameObject);
        }
    }
    //Scenario text animation, typewriter effect
    private IEnumerator ScenarioTextAnimation(string scenarioText) {
        float normalDelay = 0.04f;
        float periodDelay = 0.5f;
        float commaDelay = 0.2f;

        for (int i = 0; i < scenarioText.Length; i++) {
            scenario.text = scenarioText.Substring(0, i + 1);

            if(scenarioText[i] == '.') {
                yield return new WaitForSeconds(periodDelay);
            } else if(scenarioText[i] == ',') {
                yield return new WaitForSeconds(commaDelay);
            } else {
                yield return new WaitForSeconds(normalDelay);
            }
        }
    }

}
