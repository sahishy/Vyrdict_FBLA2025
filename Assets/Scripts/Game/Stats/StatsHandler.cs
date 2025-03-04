using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsHandler : MonoBehaviour
{
    public static StatsHandler instance;

    [Header("Stats - Main")]
    public int supplies = 100;
    public int food = 100;
    public int gold = 100;
    [Header("Stats - Misc")]
    public int population = 0;
    public int requiredPopulation = 0;
    
    //the list of constant stat changes, from non-instant boost cards
    private List<StatChange> currentStatChanges = new List<StatChange>();

    [Header("References")]
    [SerializeField] private TMP_Text suppliesText;
    [SerializeField] private TMP_Text foodText;
    [SerializeField] private TMP_Text goldText;

    [SerializeField] private RectTransform focusIndicator;
    [SerializeField] private ContentSizeFitter contentSizeFitter;
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private Image headerIcon;
    [SerializeField] private TMP_Text totalText;
    [SerializeField] private Transform buildablesHolder;
    [SerializeField] private Transform connectionsHolder;
    [SerializeField] private Transform factorsHolder;
    [SerializeField] private List<Sprite> statSprites = new List<Sprite>();
    [SerializeField] private GameObject statEffectPrefab;
    [SerializeField] private GameObject statChangePrefab;
    private List<GameObject> statEffects = new List<GameObject>();
    
    [Header("Stat Colors")]
    [SerializeField] private Color32 positiveColor = new Color32(195, 250, 216, 255);
    [SerializeField] private Color32 negativeColor = new Color32(245, 201, 196, 255);
    [SerializeField] private Color32 neutralColor = new Color32(255, 255, 255, 100);

    void Awake() {
        instance = this;
    }

    void Start() {
        SetStartingStats();
        UpdateUI();
    }

    //Sets the main stats to random values at the start of the game
    //---supplies starts off high, as the island is full of resources when it is just made
    //---Food starts off low, as the community only has 2 tents at the start
    //---Gold starts off low, as again, the community only has 2 tents at the start
    private void SetStartingStats() {
        Vector2Int randomsuppliesRange = new Vector2Int(90, 98);
        Vector2Int randomFoodRange = new Vector2Int(35, 45);
        Vector2Int randomGoldRange = new Vector2Int(20, 30);

        supplies = Random.Range(randomsuppliesRange.x, randomsuppliesRange.y);
        food = Random.Range(randomFoodRange.x, randomFoodRange.y);
        gold = Random.Range(randomGoldRange.x, randomGoldRange.y);
    }

    //-------------------------------------------------STATS-------------------------------------------------

    //Handles updating all stats, is called at the end of every day and when a buildable is removed
    public void UpdateStats() {
        //MAIN STATS

        foreach(PlacedBuildable placedBuildable in PlacementHandler.instance.GetPlacedBuildables()) {

            (int, int, int) buildableStatEffects = GetBuildableStats(placedBuildable.buildable, placedBuildable.tile);

            supplies += buildableStatEffects.Item1;
            food += buildableStatEffects.Item2;
            gold += buildableStatEffects.Item3;
        }

        foreach(ConnectionGroup connectionGroup in ConnectionsHandler.instance.connectionGroups) {
            supplies += connectionGroup.connection.environmentEffect;
            food += connectionGroup.connection.happinessEffect;
            gold += connectionGroup.connection.economyEffect;
        }

        //MISC STATS

        foreach(Community community in CommunitiesHandler.instance.allCommunities) {

            int communityPopulation = 0;
            int communityRequiredPopulation = 0;

            foreach(Buildable buildable in community.tiles.ConvertAll(x => x.currentBuildable)) {
                
                if(buildable.buildableFocus == Stat.Food) {
                    communityPopulation += buildable.residents;
                } else if(buildable.buildableFocus == Stat.Gold) {
                    communityRequiredPopulation += buildable.requiredCustomers;
                }

            }

            community.population = communityPopulation;
            community.requiredPopulation = communityRequiredPopulation;
        }

        population = CommunitiesHandler.instance.allCommunities.ConvertAll(x => x.population).Sum();
        requiredPopulation = CommunitiesHandler.instance.allCommunities.ConvertAll(x => x.requiredPopulation).Sum();

        //update factors in case new stats completed any
        FactorsHandler.instance.UpdateFactors();

        //update UI to accurately display stats
        UpdateUI();

        //show stat change animation for each stat
        StatChangeAnimation(Stat.Supplies, GetTotalStatChange(Stat.Supplies));
        StatChangeAnimation(Stat.Food, GetTotalStatChange(Stat.Food));
        StatChangeAnimation(Stat.Gold, GetTotalStatChange(Stat.Gold));
    }

    //Returns the stat effects of a buildable based on various factors
    public (int, int, int) GetBuildableStats(Buildable buildable, GridTile tile) {
        int suppliesEffect = 0;
        int foodEffect = 0;
        int goldEffect = 0;

        bool hasRequiredConnection = buildable.requiredConnection != null;
        bool meetsRequiredConnection = hasRequiredConnection ? ConnectionsHandler.instance.RequiredConnectionMet(tile) : false;

        //Change output based on whether the buildable has a required connection and meets it
        int effectModifier = hasRequiredConnection ? (meetsRequiredConnection ? 1 : -1) : 1;

        //Change stat based on whether required connection is met or not
        if(buildable.buildableFocus == Stat.Supplies || buildable.buildableFocus == Stat.Food) {
            suppliesEffect += buildable.suppliesEffect * effectModifier;
            foodEffect = buildable.foodEffect * effectModifier;
            goldEffect = buildable.goldEffect;
        } else if(buildable.buildableFocus == Stat.Gold) {
            suppliesEffect = buildable.suppliesEffect;
            foodEffect = buildable.foodEffect;
            goldEffect += buildable.goldEffect * effectModifier;
        } else if(buildable.buildableFocus == Stat.Population) {
            suppliesEffect = buildable.suppliesEffect;
            foodEffect = buildable.foodEffect;
            goldEffect = buildable.goldEffect;
        }

        //If there is a required connection and it isn't met, then don't apply the focused stat changes
        if(!hasRequiredConnection || (hasRequiredConnection && !meetsRequiredConnection)) {

            Community community = CommunitiesHandler.instance.GetCommunity(tile);

            //If the buildable focuses on supplies or food, change its output based on a formula
            //  effect  *  ( (community's # of polluting buildables)  *  (buildable's pollution influence)  *  (multiplier) )
            if(buildable.buildableFocus == Stat.Supplies || buildable.buildableFocus == Stat.Food) {
                
                int output = GetPollutionOutput(buildable);

                //only have effects from pollution if it's at a significant level
                if(output > 1) {
                    if(buildable.buildableFocus == Stat.Supplies) {
                        suppliesEffect *= -output;
                    } else if(buildable.buildableFocus == Stat.Food) {
                        foodEffect *= -output;
                    }
                }


            }
            //Otherwise if the buildable focuses on gold, change its output based on a formula
            //  effect  *  ( (community's population) / (community's required population) ) * (multiplier)
            else if(buildable.buildableFocus == Stat.Gold) {

                int population = community.population;
                int requiredPopulation = community.requiredPopulation;

                float multiplier = 0.5f;
                int output = requiredPopulation != 0 ? Mathf.FloorToInt(population / requiredPopulation * multiplier) : 0;
                
                goldEffect *= output;

            }
              
        }

        return (suppliesEffect, foodEffect, goldEffect);
    }

    //Method for changing a stat directly
    public void ChangeStat(Stat stat, int amount) {
        if(stat == Stat.Supplies) {
            supplies += amount;
        } else if(stat == Stat.Food) {
            food += amount;
        } else if(stat == Stat.Gold) {
            gold += amount;
        }
        UpdateUI();
        StatChangeAnimation(stat, amount);
    }

    //Creates a constant stat change, normally created by cards
    public void CreateConstantStatChange(Stat stat, int change, int timeLeft) {
        StatChange newStatChange = new StatChange(stat, change, timeLeft);

        Debug.Log($"Created {newStatChange.stat} by {newStatChange.change}. {newStatChange.timeLeft} days left.");
        ChangeStat(newStatChange.stat, newStatChange.change);

        currentStatChanges.Add(newStatChange);
    }

    //Constant stat changes, called at the end of each day
    public void ConstantStatChange() {
        List<StatChange> statChangesToRemove = new List<StatChange>();

        foreach(StatChange statChange in currentStatChanges) {

            statChange.timeLeft--;

            if(statChange.timeLeft > 0) {
                ChangeStat(statChange.stat, statChange.change);
                Debug.Log($"Changed {statChange.stat} by {statChange.change}. {statChange.timeLeft} days left.");
            } else {
                Debug.Log("Removed stat change");
                statChangesToRemove.Add(statChange);
            }

        }

        foreach(StatChange statChangeToRemove in statChangesToRemove) {
            currentStatChanges.Remove(statChangeToRemove);
        }
    }

    //-------------------------------------------------UI-------------------------------------------------

    private void UpdateUI() {
        suppliesText.text = supplies.ToString();
        foodText.text = food.ToString();
        goldText.text = gold.ToString();

        suppliesText.color = supplies > 0 ? Color.white : negativeColor;
        foodText.color = food > 0 ? Color.white : negativeColor;
        goldText.color = gold > 0 ? Color.white : negativeColor;
    }

    public void ShowStatsInformation(int index) {

        ResetStatsInformation();

        string header = "";
        int focusIndicatorPos = 0;
        Stat stat = Stat.None;

        if(index == 0) {
            header = $"Supplies: {supplies}";
            focusIndicatorPos = -55;
            stat = Stat.Supplies;
        } else if(index == 1) {
            header = $"Food: {food}";
            focusIndicatorPos = 0;
            stat = Stat.Food;
        } else if(index == 2) {
            header = $"Gold: {gold}";
            focusIndicatorPos = 55;
            stat = Stat.Gold;
        }

        headerText.text = header;
        headerIcon.sprite = GetStatSprite(stat);

        int totalStatChange = GetTotalStatChange(stat);
        totalText.text = totalStatChange != 0 ? $"({ConvertToEffectValue(totalStatChange)} / day)" : "(No Change)";
        totalText.color = GetSimpleStatusColor(totalStatChange);

        focusIndicator.anchoredPosition = new Vector2(focusIndicatorPos, 7.5f);
        
        Dictionary<string, int> statBuildableEffects = GetStatBuildableEffects(stat);
        foreach(string buildable in statBuildableEffects.Keys) {
            GameObject statEffect = Instantiate(statEffectPrefab, buildablesHolder);
            statEffect.transform.Find("name").GetComponent<TMP_Text>().text = ConvertToBuildableEffectName(buildable);
            statEffect.transform.Find("effect").GetComponent<TMP_Text>().text = $"{ConvertToEffectValue(statBuildableEffects[buildable])} / day";
            statEffect.transform.Find("effect").GetComponent<TMP_Text>().color = GetSimpleStatusColor(statBuildableEffects[buildable]);
            statEffects.Add(statEffect);
        }

        Dictionary<string, int> statConnectionEffects = GetStatConnectionEffects(stat);
        foreach(string connection in statConnectionEffects.Keys) {
            GameObject statEffect = Instantiate(statEffectPrefab, connectionsHolder);
            statEffect.transform.Find("name").GetComponent<TMP_Text>().text = ConvertToConnectionEffectName(connection);
            statEffect.transform.Find("effect").GetComponent<TMP_Text>().text = $"{ConvertToEffectValue(statConnectionEffects[connection])} / day";
            statEffect.transform.Find("effect").GetComponent<TMP_Text>().color = GetSimpleStatusColor(statConnectionEffects[connection]);
            statEffects.Add(statEffect);
        }
        
        Dictionary<string, int> statFactorEffects = GetStatFactorEffects(stat);
        foreach(string factor in statFactorEffects.Keys) {
            GameObject statEffect = Instantiate(statEffectPrefab, factorsHolder);
            statEffect.transform.Find("name").GetComponent<TMP_Text>().text = ConvertToFactorEffectName(factor);
            statEffect.transform.Find("effect").GetComponent<TMP_Text>().text = $"{ConvertToEffectValue(statFactorEffects[factor])} / day";
            statEffect.transform.Find("effect").GetComponent<TMP_Text>().color = GetSimpleStatusColor(statFactorEffects[factor]);
            statEffects.Add(statEffect);
        }

        buildablesHolder.Find("Nothing").gameObject.SetActive(statBuildableEffects.Count == 0);
        connectionsHolder.Find("Nothing").gameObject.SetActive(statConnectionEffects.Count == 0);
        factorsHolder.Find("Nothing").gameObject.SetActive(statFactorEffects.Count == 0);

        StartCoroutine(RefreshContentSizeFitter());

    }
    private IEnumerator RefreshContentSizeFitter() {
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        yield return null;
        contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }
    private void ResetStatsInformation() {
        foreach(GameObject effect in statEffects) {
            Destroy(effect);
        }
    }

    private void StatChangeAnimation(Stat stat, int change) {
        if(change == 0) {
            return;
        }

        Vector3 targetPos = Vector3.zero;
        if(stat == Stat.Supplies) {
            targetPos = suppliesText.transform.position;
        } else if(stat == Stat.Food) {
            targetPos = foodText.transform.position;
        } else if(stat == Stat.Gold) {
            targetPos = goldText.transform.position;
        }
        Vector3 startPos = targetPos - new Vector3(0, 60, 0);
        Vector3 endPos = targetPos - new Vector3(0, 20, 0);

        TMP_Text statChange = Instantiate(statChangePrefab, suppliesText.transform.parent.parent.parent).GetComponent<TMP_Text>();
        statChange.text = change > 0 ? $"+{change}" : $"{change}";
        statChange.color = change > 0 ? positiveColor : negativeColor;
        statChange.transform.position = startPos;
        statChange.transform.DOMove(endPos, 3f);
        statChange.DOFade(0f, 3f);
    }

    //-------------------------------------------------UTILITY-------------------------------------------------

    public int GetStat(Stat stat) {
        if(stat == Stat.Supplies) {
            return supplies;
        } else if(stat == Stat.Food) {
            return food;
        } else if(stat == Stat.Gold) {
            return gold;
        }
        return 0;
    }
    public Stat GetStatByName(string name) {
        if(name == "supplies") {
            return Stat.Supplies;
        } else if(name == "Food") {
            return Stat.Food;
        } else if(name == "Gold") {
            return Stat.Gold;
        }
        return Stat.None;
    }
    private string ConvertToBuildableEffectName(string name) {
        int placedBuildableCount = PlacementHandler.instance.GetPlacedBuildableCount(name);
        return placedBuildableCount != 1 ? $"{name} ({placedBuildableCount})" : name;
    }
    private string ConvertToConnectionEffectName(string name) {
        int connectionCount = ConnectionsHandler.instance.GetConnectionCount(name);
        return connectionCount != 1 ? $"{name} ({connectionCount})" : name;
    }
    private string ConvertToFactorEffectName(string name) {
        return name;
    }
    public string ConvertToEffectValue(int value) {
        string number;
        if(value < 0) {
            number = $"-{Mathf.Abs(value)}";
        } else {
            number = $"+{value}";
        }
        return number;
    }
    public Color32 GetSimpleStatusColor(int value) {
        if(value > 0) {
            return positiveColor;
        } else if(value == 0) {
            return neutralColor;
        } else {
            return negativeColor;
        }
    }
    public Color GetStatusColor(int value) {
        Gradient gradient = new Gradient();

        GradientColorKey[] colorKeys = new GradientColorKey[3];
        colorKeys[0].color = negativeColor;
        colorKeys[0].time = 0f;
        colorKeys[1].color = Color.white; 
        colorKeys[1].time = 0.5f;
        colorKeys[2].color = positiveColor;
        colorKeys[2].time = 1f;

        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[3];
        alphaKeys[0].alpha = 1f; 
        alphaKeys[0].time = 0f;
        alphaKeys[1].alpha = 1f; 
        alphaKeys[1].time = 0.5f;
        alphaKeys[2].alpha = 1f; 
        alphaKeys[2].time = 1f;

        gradient.SetKeys(colorKeys, alphaKeys);

        float normalizedValue = Mathf.Clamp01(value / 100f);
        return gradient.Evaluate(normalizedValue);
    }
    private Dictionary<string, int> GetStatBuildableEffects(Stat stat) {
        //key: buildable name, value: effect on stat
        Dictionary<string, int> effects = new Dictionary<string, int>();

        foreach(PlacedBuildable placedBuildable in PlacementHandler.instance.GetPlacedBuildables()) {

            (int, int, int) buildableStatEffects = GetBuildableStats(placedBuildable.buildable, placedBuildable.tile);

            int focusedStatValue = 0;
            if(stat == Stat.Supplies) {
                focusedStatValue = buildableStatEffects.Item1;                
            } else if(stat == Stat.Food) {
                focusedStatValue = buildableStatEffects.Item2;
            } else if(stat == Stat.Gold) {
                focusedStatValue = buildableStatEffects.Item3;
            }

            if(focusedStatValue != 0) {
                if(effects.ContainsKey(placedBuildable.buildable.name)) {
                    effects[placedBuildable.buildable.name] += focusedStatValue;
                } else {
                    effects.Add(placedBuildable.buildable.name, focusedStatValue);                    
                }
            }
        }

        //sort from highest to lowest effects
        effects = effects.OrderByDescending(e => e.Value).ToDictionary(e => e.Key, e => e.Value);

        return effects;
    }

    private Dictionary<string, int> GetStatConnectionEffects(Stat stat) {
        //key: connection name, value: effect on stat
        Dictionary<string, int> effects = new Dictionary<string, int>();

        if(stat == Stat.Supplies) {
            foreach(ConnectionGroup connectionGroup in ConnectionsHandler.instance.connectionGroups) {
                Connection connection = connectionGroup.connection;

                if(effects.ContainsKey(connection.name)) {
                    continue;
                }

                if(connection.environmentEffect != 0) {
                    int sameConnectionCount = ConnectionsHandler.instance.GetConnectionCount(connection.name);
                    effects.Add(connection.name, connection.environmentEffect * sameConnectionCount);
                }
            }
        } else if(stat == Stat.Food) {
            foreach(ConnectionGroup connectionGroup in ConnectionsHandler.instance.connectionGroups) {
                Connection connection = connectionGroup.connection;

                if(effects.ContainsKey(connection.name)) {
                    continue;
                }

                if(connection.happinessEffect != 0) {
                    int sameConnectionCount = ConnectionsHandler.instance.GetConnectionCount(connection.name);
                    effects.Add(connection.name, connection.happinessEffect * sameConnectionCount);
                }
            }  
        } else if(stat == Stat.Gold) {
            foreach(ConnectionGroup connectionGroup in ConnectionsHandler.instance.connectionGroups) {
                Connection connection = connectionGroup.connection;

                if(effects.ContainsKey(connection.name)) {
                    continue;
                }

                if(connection.economyEffect != 0) {
                    int sameConnectionCount = ConnectionsHandler.instance.GetConnectionCount(connection.name);
                    effects.Add(connection.name, connection.economyEffect * sameConnectionCount);
                }
            }
        }

        //sort from highest to lowest effects
        effects = effects.OrderByDescending(e => e.Value).ToDictionary(e => e.Key, e => e.Value);

        return effects;
    }
    private Dictionary<string, int> GetStatFactorEffects(Stat stat) {
        //key: connection name, value: effect on stat
        Dictionary<string, int> effects = new Dictionary<string, int>();

        foreach(Factor factor in FactorsHandler.instance.factors.Keys) {
            if(effects.ContainsKey(factor.name)) {
                continue;
            }

            if(factor.statEffect == stat) {
                effects.Add(factor.name, factor.statEffectValue);
            }
        }

        //sort from highest to lowest effects
        effects = effects.OrderByDescending(e => e.Value).ToDictionary(e => e.Key, e => e.Value);

        return effects;
    }
    public int GetTotalStatChange(Stat stat) {
        int change = 0;

        Dictionary<string, int> statBuildableEffects = GetStatBuildableEffects(stat);
        Dictionary<string, int> statConnectionEffects = GetStatConnectionEffects(stat);
        Dictionary<string, int> statFactorEffects = GetStatFactorEffects(stat);

        foreach(int effect in statBuildableEffects.Values) {
            change += effect;
        }
        foreach(int effect in statConnectionEffects.Values) {
            change += effect;
        }
        foreach(int effect in statFactorEffects.Values) {
            change += effect;
        }

        return change;
    }
    public Sprite GetStatSprite(Stat stat) {
        List<Stat> stats = new List<Stat>((Stat[])System.Enum.GetValues(typeof(Stat)));
        return statSprites[stats.IndexOf(stat)];
    }
    public int GetPollutionOutput(Buildable buildable) {
        int economicBuildableCount = PlacementHandler.instance.GetPlacedBuildables().Where(x => x.buildable.pollutable).Count();
        int pollutionInfluence = buildable.pollutionInfluence;
        
        float multiplier = 0.3f;
        int output = Mathf.FloorToInt(economicBuildableCount * pollutionInfluence * multiplier);

        return output;
    }

}

public enum Stat {
    None,
    Supplies,
    Food,
    Gold,
    Population
}

public class StatChange {
    public Stat stat;
    public int change;
    public int timeLeft;

    public StatChange(Stat stat, int change, int timeLeft) {
        this.stat = stat;
        this.change = change;
        this.timeLeft = timeLeft;
    }
}