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
    public int economy = 100;
    public int happiness = 100;
    public int environment = 100;
    [Header("Stats - Misc")]
    public int population = 0;
    public int requiredPopulation = 0;

    [Header("References")]
    [SerializeField] private Image environmentBar;
    [SerializeField] private Image happinessBar;
    [SerializeField] private Image economyBar;

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
    private List<GameObject> statEffects = new List<GameObject>();
    

    private Color32 positiveColor = new Color32(195, 250, 216, 255);
    private Color32 negativeColor = new Color32(245, 201, 196, 255);
    private Color32 neutralColor = new Color32(255, 255, 255, 100);

    void Awake() {
        instance = this;
    }

    void Start() {
        SetStartingStats();
        UpdateUI();
    }

    //Sets the main stats to random values at the start of the game
    //---Environment starts off high, as there is barely any pollution when the island is just made
    //---Happiness starts off low, as the community has been sad for a while
    //---Economy starts off low, as there is barely any economic activity
    private void SetStartingStats() {
        Vector2Int randomEnvironmentRange = new Vector2Int(90, 98);
        Vector2Int randomHappinessRange = new Vector2Int(35, 45);
        Vector2Int randomEconomyRange = new Vector2Int(20, 30);

        environment = Random.Range(randomEnvironmentRange.x, randomEnvironmentRange.y);
        happiness = Random.Range(randomHappinessRange.x, randomHappinessRange.y);
        economy = Random.Range(randomEconomyRange.x, randomEconomyRange.y);
    }

    //-------------------------------------------------STATS-------------------------------------------------

    //Handles updating all stats, is called at the end of every day and when a buildable is removed
    public void UpdateStats() {
        //MAIN STATS

        int unclampedEnvironment = environment;
        int unclampedHappiness = happiness;
        int unclampedEconomy = economy;

        foreach(PlacedBuildable placedBuildable in PlacementHandler.instance.GetPlacedBuildables()) {

            (int, int, int) buildableStatEffects = GetBuildableStats(placedBuildable.buildable, placedBuildable.tile);

            unclampedEnvironment += buildableStatEffects.Item1;
            unclampedHappiness += buildableStatEffects.Item2;
            unclampedEconomy += buildableStatEffects.Item3;
        }

        foreach(ConnectionGroup connectionGroup in ConnectionsHandler.instance.connectionGroups) {
            unclampedEnvironment += connectionGroup.connection.environmentEffect;
            unclampedHappiness += connectionGroup.connection.happinessEffect;
            unclampedEconomy += connectionGroup.connection.economyEffect;
        }
        
        environment = Mathf.Clamp(unclampedEnvironment, 0, 100);
        happiness = Mathf.Clamp(unclampedHappiness, 0, 100);
        economy = Mathf.Clamp(unclampedEconomy, 0, 100);

        //MISC STATS

        foreach(Community community in CommunitiesHandler.instance.allCommunities) {

            int communityPopulation = 0;
            int communityRequiredPopulation = 0;

            foreach(Buildable buildable in community.tiles.ConvertAll(x => x.currentBuildable)) {
                
                if(buildable.buildableFocus == Stat.Happiness) {
                    communityPopulation += buildable.residents;
                } else if(buildable.buildableFocus == Stat.Economy) {
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
    }

    //Returns the stat effects of a buildable based on various factors
    public (int, int, int) GetBuildableStats(Buildable buildable, GridTile tile) {
        int environmentEffect = 0;
        int happinessEffect = 0;
        int economyEffect = 0;

        //Check if the buildable has a required connection
        if(buildable.requiredConnection != null) {
            
            bool requiredConnectionMet = ConnectionsHandler.instance.RequiredConnectionMet(tile);
            int effectModifier = requiredConnectionMet ? 1 : -1;

            //Change stat based on whether required connection is met or not
            switch(buildable.buildableFocus) {
            
                case Stat.Environment:
                    environmentEffect += buildable.environmentEffect * effectModifier;
                    happinessEffect = buildable.happinessEffect;
                    economyEffect = buildable.economyEffect;
                    break;
                
                case Stat.Happiness:
                    environmentEffect = buildable.environmentEffect;
                    happinessEffect += buildable.happinessEffect * effectModifier;
                    economyEffect = buildable.economyEffect;
                    break;
                
                case Stat.Economy:
                    environmentEffect = buildable.environmentEffect;
                    happinessEffect = buildable.happinessEffect;
                    economyEffect += buildable.economyEffect * effectModifier;
                    break;
                
            }

        }

        //If the buildable is economic, change its economic output based on a formula which considers population
        //  ( (community's population) / (community's required population) ) * (multiplier)
        if(buildable.buildableFocus == Stat.Economy) {
            Community community = CommunitiesHandler.instance.GetCommunity(tile);

            int population;
            int requiredPopulation;

            if(community != null) {
                population = community.population;
                requiredPopulation = community.requiredPopulation;
            } else {
                population = 0;
                requiredPopulation = buildable.requiredCustomers;
            }

            float multiplier = 1f;
            int economicOutput = Mathf.FloorToInt(population / requiredPopulation * multiplier);
            
            economyEffect += economicOutput;
        }

        return (environmentEffect, happinessEffect, economyEffect);
    }

    //Method for changing a stat directly
    public void ChangeStat(Stat stat, int amount) {
        if(stat == Stat.Environment) {
            environment += amount;
        } else if(stat == Stat.Happiness) {
            happiness += amount;
        } else if(stat == Stat.Economy) {
            economy += amount;
        }
        UpdateUI();
    }

    //-------------------------------------------------UI-------------------------------------------------

    private void UpdateUI() {
        float maxStatAmount = 100f;
        environmentBar.DOFillAmount(environment / maxStatAmount, 0.5f);
        happinessBar.DOFillAmount(happiness / maxStatAmount, 0.5f);
        economyBar.DOFillAmount(economy / maxStatAmount, 0.5f);
    }

    public void ShowStatsInformation(int index) {

        ResetStatsInformation();

        string header = "";
        int focusIndicatorPos = 0;
        Stat stat = Stat.None;

        if(index == 0) {
            header = $"Environmental Health: {environment}";
            focusIndicatorPos = -55;
            stat = Stat.Environment;
        } else if(index == 1) {
            header = $"Community Happiness: {happiness}";
            focusIndicatorPos = 0;
            stat = Stat.Happiness;
        } else if(index == 2) {
            header = $"Economic Health: {economy}";
            focusIndicatorPos = 55;
            stat = Stat.Economy;
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

    //-------------------------------------------------UTILITY-------------------------------------------------

    public int GetStat(Stat stat) {
        if(stat == Stat.Environment) {
            return environment;
        } else if(stat == Stat.Happiness) {
            return happiness;
        } else if(stat == Stat.Economy) {
            return economy;
        }
        return 0;
    }
    public bool AnyStatZero() {
        return economy <= 0 || happiness <= 0 || environment <= 0;
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
            if(stat == Stat.Environment) {
                focusedStatValue = buildableStatEffects.Item1;                
            } else if(stat == Stat.Happiness) {
                focusedStatValue = buildableStatEffects.Item2;
            } else if(stat == Stat.Economy) {
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

        if(stat == Stat.Environment) {
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
        } else if(stat == Stat.Happiness) {
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
        } else if(stat == Stat.Economy) {
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
    private int GetTotalStatChange(Stat stat) {
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
}

public enum Stat {
    None,
    Environment,
    Happiness,
    Economy
}