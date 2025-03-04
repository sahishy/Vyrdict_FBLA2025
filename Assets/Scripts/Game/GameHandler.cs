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
    public int fateTimer = 7;

    [Header("Settings")]
    [SerializeField] private float dayDuration = 10f;
    [HideInInspector] public float timer;
    private float dayTimer;
    [HideInInspector] public float timeScale = 1;
    private float housingSpawnChance = 1f;

    [Header("Animation")]
    public Animatable currentFocusedAnimatable = null;
    [SerializeField] private Gradient timeColor;

    [Header("References")]
    [SerializeField] private CanvasGroup sceneChangeScreen;
    [SerializeField] private CanvasGroup gameplayScreen;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private Image dayBar;
    [SerializeField] private TMP_Text weekText;
    [SerializeField] private TMP_Text fateText;
    [SerializeField] private Image fateBar;
    [SerializeField] private Transform mainLight;

    private bool timeInformationActive = false;
    private int statsInformationIndex = -1;
    private bool fateInformationActive = false;
    [SerializeField] private Transform timeInformationPanel;
    [SerializeField] private Image timeInformationSpeedImage;
    [SerializeField] private List<Sprite> timeInformationSpeedIcons;
    [SerializeField] private Transform statsInformationPanel;
    [SerializeField] private Transform fateInformationPanel;

    void Awake() {
        instance = this;

        DOTween.SetTweensCapacity(500, 50);
        gameplayScreen.alpha = 0;

        //UPDATE TIME LIGHTING
        UpdateTimeLighting();

        sceneChangeScreen.gameObject.SetActive(true);
        sceneChangeScreen.DOFade(0f, 1f).OnComplete(() => {
            sceneChangeScreen.gameObject.SetActive(false);
        });
    }

    //Starting method called at the start of the game (CALLED BY GridHandler, MAKES SURE MAP IS CREATED)
    public IEnumerator StartGame() {

        //INTRO ANIMATION
        Invoke(nameof(IntroAnimation), 1f);

        //DISABLE PLAYING CARDS (prevent player from using cards immediately)
        //CardsHandler.instance.canPlay = false;

        //UPDATE UI
        UpdateTimeUI();
        StartCoroutine(RefreshContentSizeFitter(dayText.transform.parent.GetComponent<ContentSizeFitter>(), 1f));

        //STARTING PROCESS - create factors, two starter houses, cutscene, dialogue

        //STARTING FACTORS
        FactorsHandler.instance.AddStartingEvents();

        //show starting dialogue scene
        StartCoroutine(IntroDialogue());
        
        //START WEEK

        yield return new WaitForSeconds(2f);

        //make sure map is generated before starting week, events depend on the map
        StartWeek();
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
        dayBar.fillAmount = dayTimer / dayDuration;

        //time animation
        UpdateTimeLighting();

    }
    private void UpdateTimeLighting() {
        //time animation
        mainLight.rotation = Quaternion.Euler((dayTimer / dayDuration) * 360, -30, 0);
        RenderSettings.ambientLight = timeColor.Evaluate(dayTimer / dayDuration) * 1.7f;
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
        //FactorsHandler.instance.TryAddRandomFactor();

        //GAME LOOP - change stats
        StatsHandler.instance.ConstantStatChange();
        StatsHandler.instance.UpdateStats();

        //GAME LOOP - check if player lost
        Fate();

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
        StartCoroutine(CardsHandler.instance.EnterDrawPhase());
    }

    //Fate functionality, decreases fate timer at the end of each day, ends game if the fate timer is at zero
    private void Fate() {
        ChangeFate(-1);

        if(fateTimer <= 0) {
            EndGame();
        }
    }
    public void ChangeFate(int value) {
        fateTimer += value;
        UpdateFateUI();
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

    //--------------------------------------GAME LOOP--------------------------------------
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

        //StartCoroutine(IntroStormAnimation());
    }
    private IEnumerator IntroStormAnimation() {
        GameObject.Find("Rain").SetActive(true);
        DOTween.To(() => RenderSettings.fogColor, x => RenderSettings.fogColor = x, new Color32(60, 170, 170, 255), 3);
        DOTween.To(() => RenderSettings.fogStartDistance, x => RenderSettings.fogStartDistance = x, 0, 3);
        DOTween.To(() => RenderSettings.fogEndDistance, x => RenderSettings.fogEndDistance = x, 30, 3);

        yield return new WaitUntil(() => currentDay >= 7);

        GameObject.Find("Rain").transform.GetChild(0).GetComponent<ParticleSystem>().Stop();

        DOTween.To(() => RenderSettings.fogColor, x => RenderSettings.fogColor = x, new Color32(0, 153, 178, 255), 3);
        DOTween.To(() => RenderSettings.fogStartDistance, x => RenderSettings.fogStartDistance = x, 20, 3);
        DOTween.To(() => RenderSettings.fogEndDistance, x => RenderSettings.fogEndDistance = x, 50, 3);
    }

    private IEnumerator IntroDialogue() {
        yield return new WaitUntil(() => dayTimer > 5f);

        //show the starting emote for the two houses
        ConnectionGroup targetNeighborsGroup = ConnectionsHandler.instance.connectionGroups.FirstOrDefault(x => x.connection.name == "Neighbors");
        EmoteHandler.instance.CreateEmote(Emote.Sad, targetNeighborsGroup);
        //animation for focusing on two houses
        PlayerController.instance.CameraZoom(GridHandler.instance.GetAverageTileListPosition(targetNeighborsGroup.tiles), 4f, 2f, 2f);
        
        DialogueHandler.instance.AddDialogue("This is your community. They feel cramped living in such a small town.");
        DialogueHandler.instance.AddDialogue("The population won't fit in these two tents for long. The community is growing at a rapid rate.");
    }

    //--------------------------------------UI--------------------------------------
    private void UpdateTimeUI() {
        dayText.text = $"Day {currentDay}";
        dayText.transform.DOPunchScale(Vector3.one * 0.2f, 0.1f, 0, 0f);
        dayBar.transform.parent.DOPunchScale(Vector3.one * 0.2f, 0.1f, 0, 0f);
        weekText.text = $"Week {currentWeek}";
        StartCoroutine(RefreshContentSizeFitter(dayText.transform.parent.GetComponent<ContentSizeFitter>()));
    }
    private void UpdateFateUI() {
        fateText.text = $"{fateTimer} Days";
        fateText.transform.parent.DOPunchScale(Vector3.one * 0.2f, 0.1f, 0, 0f);
        fateBar.DOFillAmount(fateTimer / 7f, 0.5f);
        fateBar.transform.parent.DOPunchScale(Vector3.one * 0.2f, 0.1f, 0, 0f);
        StartCoroutine(RefreshContentSizeFitter(fateText.transform.parent.GetComponent<ContentSizeFitter>()));
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

    public void ShowFateInformation() {
        fateInformationActive = true;
        currentFocusedAnimatable = this;

        fateInformationPanel.DOScaleY(1, 0.2f).SetEase(Ease.OutBack);
    }
    public void HideFateInformation() {
        fateInformationActive = false;

        fateInformationPanel.DOScaleY(0, 0.2f).SetEase(Ease.InBack);
    }

    public void AnimatableExit()
    {
        if(timeInformationActive) {
            HideTimeInformation();
        }
        if(statsInformationIndex != -1) {
            HideStatsInformation();
        }
        if(fateInformationActive) {
            HideFateInformation();
        }
    }
}