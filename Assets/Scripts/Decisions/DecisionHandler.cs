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
    //Creates choices slowly, makes sure player isn't overwhelmed by all at once
    private void ChoiceCreation() {
        List<Decision> topDecisions = GetTopDecisions();

        for(int i = 0; i < 3; i++) {
            DecisionChoice choice = Instantiate(decisionChoicePrefab, decisionChoiceHolder.transform).GetComponent<DecisionChoice>();
            choice.Initialize(topDecisions[i], i);
        }
    }
    //Resets cooldown to prevent player from accidentally selecting a choice the second it appears
    private void DecisionCooldown() {
        cooldown = false;
    }
    //Called when player selects a choice in the decision screen
    public void SelectChoice(Decision decision) {
        if(cooldown) {
            return;
        }

        //unfocus any animatable
        GameHandler.instance.currentFocusedAnimatable.AnimatableExit();

        //closing animation
        decisionScreen.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() => {
            decisionScreen.SetActive(false); 
        });
        gameplayScreen.SetActive(true);
        gameplayScreen.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

        Invoke(nameof(ResetDecisionMakingAnimation), 1f);

        //add choice to inventory
        InventoryHandler.instance.TryAddItem(decision.buildable, decision.amount);

        inDecisionMode = false;
        cooldown = false;

        //RESUME THE WEEK
        GameHandler.instance.StartWeek();
    }

    //Returns the best 3 decisions to give the player based on their current stats
    public List<Decision> GetTopDecisions() {
        // Create a dictionary to store each decision and its score
        Dictionary<Decision, float> decisionScores = new Dictionary<Decision, float>();

        // Evaluate each decision based on relevance
        foreach (Decision decision in allDecisions) {
            Buildable buildable = decision.buildable;
            int amount = decision.amount;

            // Calculate the potential stat changes from the buildable
            int environmentEffect = buildable.environmentEffect * amount;
            int happinessEffect = buildable.happinessEffect * amount;
            int economyEffect = buildable.economyEffect * amount;

            // Score the decision based on the player's current stat needs
            float score = 0;

            // Prioritize decisions that address deficits
            if (StatsHandler.instance.environment < 50) score += environmentEffect * 1.5f;
            else score += environmentEffect;

            if (StatsHandler.instance.happiness < 50) score += happinessEffect * 1.5f;
            else score += happinessEffect;

            if (StatsHandler.instance.economy < 50) score += economyEffect * 1.5f;
            else score += economyEffect;

            // Avoid decisions that worsen stats already in deficit
            if (StatsHandler.instance.environment < 50 && environmentEffect < 0) score -= Mathf.Abs(environmentEffect) * 2;
            if (StatsHandler.instance.happiness < 50 && happinessEffect < 0) score -= Mathf.Abs(happinessEffect) * 2;
            if (StatsHandler.instance.economy < 50 && economyEffect < 0) score -= Mathf.Abs(economyEffect) * 2;

            // Add the score to the dictionary
            decisionScores.Add(decision, score);
        }

        // Sort decisions by score and return the top 3
        return decisionScores.OrderByDescending(pair => pair.Value).Select(pair => pair.Key).Take(3).ToList();
    }

    //--------------------------------------UI--------------------------------------

    //Animation that handles showing all the important factors one at a time
    private IEnumerator DecisionMakingAnimation() {
        
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

        environmentBar.DOFillAmount(StatsHandler.instance.environment / 100f, randomEnvironmentAnimationTime).SetEase(Ease.OutExpo);
        happinessBar.DOFillAmount(StatsHandler.instance.happiness / 100f, randomHappinessAnimationTime).SetEase(Ease.OutExpo);
        economyBar.DOFillAmount(StatsHandler.instance.economy / 100f, randomEconomyAnimationTime).SetEase(Ease.OutExpo);

        //show raw stat values, do counting animation
        float _environmentValue = 0f;
        float _happinessValue = 0f;
        float _economyValue = 0f;

        DOTween.To(x => _environmentValue = x, 0f, StatsHandler.instance.environment, randomEnvironmentAnimationTime)
        .SetEase(Ease.OutExpo).OnUpdate(() => environmentText.text = Mathf.RoundToInt(_environmentValue).ToString());
        DOTween.To(x => _happinessValue = x, 0f, StatsHandler.instance.happiness, randomHappinessAnimationTime)
        .SetEase(Ease.OutExpo).OnUpdate(() => happinessText.text = Mathf.RoundToInt(_happinessValue).ToString());
        DOTween.To(x => _economyValue = x, 0f, StatsHandler.instance.economy, randomEconomyAnimationTime)
        .SetEase(Ease.OutExpo).OnUpdate(() => economyText.text = Mathf.RoundToInt(_economyValue).ToString());

        yield return new WaitForSeconds(3f);

        //show status color of raw stats (red bad, white neutral, green good)
        environmentText.DOColor(StatsHandler.instance.GetStatusColor(StatsHandler.instance.environment), 1f);
        happinessText.DOColor(StatsHandler.instance.GetStatusColor(StatsHandler.instance.happiness), 1f);
        economyText.DOColor(StatsHandler.instance.GetStatusColor(StatsHandler.instance.economy), 1f);

        //give extra time to look at stats
        yield return new WaitForSeconds(2f);

        //hide stats of the week, propose question
        label.DOFade(0, 1f);
        yield return new WaitForSeconds(1f);

        weekStatsHolder.DOAnchorPos(new Vector2(0, 315), 2f).SetEase(Ease.InOutCubic);
        weekStatsHolder.transform.DOScale(0.575f, 2f).SetEase(Ease.InOutCubic);
        weekStatsTextHolder.DOAnchorPos(new Vector2(0, 280), 2f).SetEase(Ease.InOutCubic);
        weekStatsTextHolder.transform.DOScale(0.575f, 2f).SetEase(Ease.InOutCubic);
        yield return new WaitForSeconds(2f);

        label.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 200);
        label.text = "What will you do?";
        label.DOFade(1f, 3f);

        yield return new WaitForSeconds(2f);

        //CREATING CHOICES
        ChoiceCreation();
    }
    //Reset the changes made by animation in DecisionMakingAnimation()
    private void ResetDecisionMakingAnimation() {
        weekCounter.text = "";
        weekCounter.alpha = 1f;

        label.alpha = 0;
        label.text = "Here are your stats for the week:";
        label.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 75);

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

}
