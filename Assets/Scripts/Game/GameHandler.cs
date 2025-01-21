using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class GameHandler : MonoBehaviour, Animatable
{
    public static GameHandler instance;

    [Header("Time")]
    public bool timeFrozen = true;
    public int currentDay = 0;
    public int currentWeek = 0;

    [Header("Settings")]
    [SerializeField] private float dayDuration = 10f;
    [HideInInspector] public float timer;
    private float dayTimer;
    [HideInInspector] public float timeScale = 1;
    private float housingSpawnChance = 1f;

    [Header("Animation")]
    public Animatable currentFocusedAnimatable = null;

    [Header("References")]
    [SerializeField] private CanvasGroup gameplayScreen;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private Image dayBar;
    [SerializeField] private TMP_Text weekText;

    private bool timeInformationActive = false;
    private int statsInformationIndex = -1;
    [SerializeField] private Transform timeInformationPanel;
    [SerializeField] private Image timeInformationSpeedImage;
    [SerializeField] private List<Sprite> timeInformationSpeedIcons;
    [SerializeField] private Transform statsInformationPanel;
    [SerializeField] private List<Transform> statsInformationPanels = new List<Transform>();

    void Awake() {
        instance = this;

        DOTween.SetTweensCapacity(500, 50);
        gameplayScreen.alpha = 0;
    }

    //Starting method called at the start of the game (CALLED BY GridHandler, MAKES SURE MAP IS CREATED)
    public IEnumerator StartGame() {

        //INTRO ANIMATION
        Invoke(nameof(IntroAnimation), 1f);

        //START WEEK
        //make sure map is generated before starting week, events depend on the map
        StartWeek();

        //UPDATE UI
        UpdateTimeUI();
        StartCoroutine(RefreshContentSizeFitter(dayText.transform.parent.GetComponent<ContentSizeFitter>(), 1f));

        //STARTING PROCESS - create factors, two starter houses, cutscene, dialogue

        //STARTING FACTORS
        FactorsHandler.instance.AddStartingEvents();
        
        //show the starting emote for the two houses
        ConnectionGroup targetNeighborsGroup = ConnectionsHandler.instance.connectionGroups.FirstOrDefault(x => x.connection.name == "Neighbors");
        EmoteHandler.instance.CreateEmote(Emote.Sad, targetNeighborsGroup, 5f);
        //animation for focusing on two houses
        PlayerController.instance.CameraZoom(GridHandler.instance.GetAverageTileListPosition(targetNeighborsGroup.tiles), 4f, 2f, 2f, 3f);
        
        //show starting dialogue
        yield return new WaitForSeconds(5f);
        DialogueHandler.instance.AddDialogue("This is your community. They feel cramped living in such a small town.");
        DialogueHandler.instance.AddDialogue("The population won't fit in these two tents for long. The community is growing at a rapid rate.");
    }

    void Update() {
        if(!timeFrozen) {
            TimeProgression();
        }
    }

    //Handles the progression of time
    private void TimeProgression() {
        float time = Time.deltaTime * timeScale;
        timer += time;
        dayTimer += time;
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
            EndWeek();
        }

        //GAME LOOP - create random housing every 3 days
        if(housingSpawnChance > Random.Range(0, 100)) {
            housingSpawnChance = 1;
            SpawnHousing();
        } else {
            housingSpawnChance *= 3; //1, 3, 9, 27, 81, 100+
        }

        //GAME LOOP - try to add a random factor if not at max
        FactorsHandler.instance.TryAddRandomFactor();

        StatsHandler.instance.UpdateStats();
        CheckGameOver();

        //UPDATE ACTIVE UI - make sure to update UI that is open after stats are updated
        UpdateTimeUI();
        if(statsInformationIndex != -1) {
            StatsHandler.instance.ShowStatsInformation(statsInformationIndex);
        }
    }

    //Called at the end of each week
    private void EndWeek() {
        currentWeek++;
        
        ToggleTimeFreeze(true);
        DecisionHandler.instance.ProposeDecision();
        //timeFrozen is set to false in DecisionHandler.SelectChoice()
        
    }

    //Called at the start of each week by DecisionHandler
    public void StartWeek() {
        ToggleTimeFreeze(false);
    }

    //Checks if any of the stats reached zero at the end of a day, if so then the player lost
    private void CheckGameOver() {
        if(StatsHandler.instance.AnyStatZero()) {
            EndGame();
        }
    }

    //Toggle time freezing
    public void ToggleTimeFreeze(bool toggle) {
        timeFrozen = toggle;
        if(timeFrozen) {
            dayBar.DOFade(0.5f, 0.5f);
        } else {
            if(timeScale != 0) {
                dayBar.DOFade(1f, 0.5f);
            }
        }
    }

    private void EndGame() {
        Debug.Log($"Game Over! Survived {currentDay} days and {currentWeek} weeks.");
    }

    ////--------------------------------------GAME LOOP--------------------------------------
    private void SpawnHousing() {
        List<GridTile> availableSpots = GridHandler.instance.GetUnoccupiedTiles();
        GridTile targetSpot = availableSpots[Random.Range(0, availableSpots.Count)];
        Vector3 randomRotation = PlacementHandler.instance.rotations[Random.Range(0, 6)];

        Buildable houseBuildable = Resources.Load<Buildable>("Buildables/Tent");
        PlacementHandler.instance.AddBuildable(houseBuildable, randomRotation, targetSpot);

        //add connections buildable has with adjacent tiles, if any
        ConnectionsHandler.instance.TryAddConnections(targetSpot);

        //add or merge communities if any
        CommunitiesHandler.instance.TryAddCommunities(targetSpot);

        //create emote to alert player
        EmoteHandler.instance.CreateEmote(Emote.Alert, targetSpot.holder.transform.position, 1f);
    }

    //--------------------------------------GAME INTRO--------------------------------------
    private void IntroAnimation() {
        gameplayScreen.DOFade(1f, 3f);
    }

    //--------------------------------------UI--------------------------------------
    private void UpdateTimeUI() {
        dayText.text = $"Day {currentDay}";
        dayText.transform.DOPunchScale(Vector3.one * 0.2f, 0.1f, 0, 0f);
        dayBar.transform.parent.DOPunchScale(Vector3.one * 0.2f, 0.1f, 0, 0f);
        weekText.text = $"Week {currentWeek}";

        RefreshContentSizeFitter(dayText.transform.parent.GetComponent<ContentSizeFitter>());
    }
    private IEnumerator RefreshContentSizeFitter(ContentSizeFitter contentSizeFitter, float delay = 0f) {
        yield return new WaitForSeconds(delay);
        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        yield return null;
        contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.MinSize;
    }
    public void ChangeTimeScale(float value) {
        List<float> timeScales = new List<float> {
            0, //freeze (slider 0)
            0.5f, //slow (slider 1)
            1, //default (slider 2)
            2, //fast (slider 3)
            4 //super fast (slider 4)
        };
        timeScale = timeScales[(int)value];
        if(!timeFrozen) {
            dayBar.DOFade(timeScale == 0 ? 0.5f : 1f, 0.5f);
        }

        timeInformationSpeedImage.sprite = timeInformationSpeedIcons[(int)value];
    }

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
        statsInformationIndex = index;
        currentFocusedAnimatable = this;

        statsInformationPanel.DOScaleY(1, 0.2f).SetEase(Ease.OutBack);

        StatsHandler.instance.ShowStatsInformation(index);
    }
    public void HideStatsInformation(int index = -1) {
        statsInformationIndex = -1;

        statsInformationPanel.DOScaleY(0, 0.2f).SetEase(Ease.InBack).OnComplete(() => {
            if(statsInformationIndex == -1) { //prevents panel from disappearing when player switches mid-animation
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
        if(statsInformationIndex != -1) {
            HideStatsInformation();
        }
    }
}