using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class GameHandler : MonoBehaviour, Animatable
{
    public static GameHandler instance;

    [Header("Time")]
    public bool timeFrozen = false;
    public int currentDay = 0;
    public int currentWeek = 0;

    [Header("Settings")]
    [SerializeField] private float dayDuration = 10f;
    private float dayTimer;

    [Header("Animation")]
    public Animatable currentFocusedAnimatable = null;

    [Header("References")]
    [SerializeField] private CanvasGroup gameplayScreen;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private Image dayBar;
    [SerializeField] private TMP_Text weekText;

    private bool timeInformationActive = false;
    private bool statsInformationActive = false;
    [SerializeField] private Transform timeInformationPanel;
    [SerializeField] private Transform statsInformationPanel;
    [SerializeField] private List<Transform> statsInformationPanels = new List<Transform>();

    void Awake() {
        instance = this;
    }

    void Start() {
        gameplayScreen.alpha = 0;
        Invoke(nameof(IntroAnimation), 1f);
        DOTween.SetTweensCapacity(500, 50);

        UpdateTimeUI();
    }

    void Update() {
        if(!timeFrozen) {
            TimeProgression();
        }
    }

    //Handles the progression of time
    private void TimeProgression() {
        dayTimer += Time.deltaTime;
        if (dayTimer >= dayDuration) {
            EndDay();
            dayTimer = 0;
        }

        //updates the day bar, if time is frozen then the bar is not updated for visual purposes
        if(!timeFrozen) {
            dayBar.fillAmount = dayTimer / dayDuration;
        }
        
    }

    //Called at the end of each day
    private void EndDay() {
        currentDay++;
        if(currentDay % 7 == 0) {
            currentWeek++;
            
            timeFrozen = true;
            DecisionHandler.instance.ProposeDecision();
            //timeFrozen is set to false in DecisionHandler.SelectChoice()
        }

        StatsHandler.instance.UpdateStats();
        CheckGameOver();

        UpdateTimeUI();
    }

    private void UpdateTimeUI() {
        dayText.text = $"Day {currentDay}";
        dayText.transform.DOPunchScale(Vector3.one * 0.2f, 0.1f, 0, 0f);
        dayBar.transform.parent.DOPunchScale(Vector3.one * 0.2f, 0.1f, 0, 0f);
        weekText.text = $"Week {Mathf.FloorToInt(currentWeek / 7)}";
    }

    //Checks if any of the stats reached zero at the end of a day, if so then the player lost
    private void CheckGameOver() {
        if(StatsHandler.instance.AnyStatZero()) {
            EndGame();
        }
    }

    private void EndGame() {
        Debug.Log($"Game Over! Survived {currentDay} days and {currentWeek} weeks.");
    }


    //--------------------------------------GAME INTRO--------------------------------------
    private void IntroAnimation() {
        gameplayScreen.DOFade(1f, 3f);
    }

    //--------------------------------------UI ANIMATIONS--------------------------------------
    public void ShowTimeInformation() {
        timeInformationActive = true;
        currentFocusedAnimatable = this;

        timeInformationPanel.DOScaleY(1, 0.2f).SetEase(Ease.OutBack);
    }
    public void HideTimeInformation() {
        timeInformationActive = false;

        timeInformationPanel.DOScaleY(0, 0.2f).SetEase(Ease.InBack);
    }

    public void ShowStatsInformation(int index) {
        statsInformationActive = true;
        currentFocusedAnimatable = this;

        statsInformationPanel.DOScaleY(1, 0.2f).SetEase(Ease.OutBack);

        StatsHandler.instance.ShowStatsInformation(index);
    }
    public void HideStatsInformation(int index = -1) {
        statsInformationActive = false;

        statsInformationPanel.DOScaleY(0, 0.2f).SetEase(Ease.InBack).OnComplete(() => {
            if(!statsInformationActive) { //prevents panel from disappearing when player switches mid-animation
                if(index != -1) { //make sure index was defined
                    //statsInformationPanels[index].gameObject.SetActive(false);
                }
            }
        });

    }

    public void AnimatableExit()
    {
        if(timeInformationActive) {
            HideTimeInformation();
        }
        if(statsInformationActive) {
            HideStatsInformation();
        }
    }
}