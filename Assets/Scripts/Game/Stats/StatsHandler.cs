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
    public int materials = 100;
    public int food = 100;
    public int gold = 100;
    [Header("Stats - Misc")]
    public int population = 0;
    public int requiredPopulation = 0;

    [Header("References")]
    [SerializeField] private TMP_Text materialsText;
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
    //---Materials starts off high, as the island is full of resources when it is just made
    //---Food starts off low, as the community only has 2 tents at the start
    //---Gold starts off low, as again, the community only has 2 tents at the start
    private void SetStartingStats() {
        Vector2Int randomMaterialsRange = new Vector2Int(90, 98);
        Vector2Int randomFoodRange = new Vector2Int(35, 45);
        Vector2Int randomGoldRange = new Vector2Int(20, 30);

        materials = Random.Range(randomMaterialsRange.x, randomMaterialsRange.y);
        food = Random.Range(randomFoodRange.x, randomFoodRange.y);
        gold = Random.Range(randomGoldRange.x, randomGoldRange.y);
    }

    //-------------------------------------------------STATS-------------------------------------------------

    //Handles updating all stats, is called at the end of every day and when a buildable is removed
    public void UpdateStats() {
        //MAIN STATS

        foreach(PlacedBuildable placedBuildable in PlacementHandler.instance.GetPlacedBuildables()) {

            (int, int, int) buildableStatEffects = GetBuildableStats(placedBuildable.buildable, placedBuildable.tile);

            materials += buildableStatEffects.Item1;
            food += buildableStatEffects.Item2;
            gold += buildableStatEffects.Item3;
        }

        foreach(ConnectionGroup connectionGroup in ConnectionsHandler.instance.connectionGroups) {
            materials += connectionGroup.connection.environmentEffect;
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
        StatChangeAnimation(Stat.Materials, GetTotalStatChange(Stat.Materials));
        StatChangeAnimation(Stat.Food, GetTotalStatChange(Stat.Food));
        StatChangeAnimation(Stat.Gold, GetTotalStatChange(Stat.Gold));
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
            
                case Stat.Materials:
                    environmentEffect += buildable.environmentEffect * effectModifier;
                    happinessEffect = buildable.happinessEffect;
                    economyEffect = buildable.economyEffect;
                    break;
                
                case Stat.Food:
                    environmentEffect = buildable.environmentEffect;
                    happinessEffect += buildable.happinessEffect * effectModifier;
                    economyEffect = buildable.economyEffect;
                    break;
                
                case Stat.Gold:
                    environmentEffect = buildable.environmentEffect;
                    happinessEffect = buildable.happinessEffect;
                    economyEffect += buildable.economyEffect * effectModifier;
                    break;
                
            }

        }

        //If the buildable is economic, change its economic output based on a formula which considers population
        //  ( (community's population) / (community's required population) ) * (multiplier)
        if(buildable.buildableFocus == Stat.Gold) {
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
        if(stat == Stat.Materials) {
            materials += amount;
        } else if(stat == Stat.Food) {
            food += amount;
        } else if(stat == Stat.Gold) {
            gold += amount;
        }
        UpdateUI();
        StatChangeAnimation(stat, amount);
    }

    //-------------------------------------------------UI-------------------------------------------------

    private void UpdateUI() {
        materialsText.text = materials.ToString();
        foodText.text = food.ToString();
        goldText.text = gold.ToString();
    }

    public void ShowStatsInformation(int index) {

        ResetStatsInformation();

        string header = "";
        int focusIndicatorPos = 0;
        Stat stat = Stat.None;

        if(index == 0) {
            header = $"Materials: {materials}";
            focusIndicatorPos = -55;
            stat = Stat.Materials;
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
        if(stat == Stat.Materials) {
            targetPos = materialsText.transform.position;
        } else if(stat == Stat.Food) {
            targetPos = foodText.transform.position;
        } else if(stat == Stat.Gold) {
            targetPos = goldText.transform.position;
        }
        Vector3 startPos = targetPos - new Vector3(0, 60, 0);
        Vector3 endPos = targetPos - new Vector3(0, 20, 0);

        TMP_Text statChange = Instantiate(statChangePrefab, materialsText.transform.parent.parent.parent).GetComponent<TMP_Text>();
        statChange.text = change > 0 ? $"+{change}" : $"{change}";
        statChange.color = change > 0 ? positiveColor : negativeColor;
        statChange.transform.position = startPos;
        statChange.transform.DOMove(endPos, 3f);
        statChange.DOFade(0f, 3f);
    }

    //-------------------------------------------------UTILITY-------------------------------------------------

    public int GetStat(Stat stat) {
        if(stat == Stat.Materials) {
            return materials;
        } else if(stat == Stat.Food) {
            return food;
        } else if(stat == Stat.Gold) {
            return gold;
        }
        return 0;
    }
    public Stat GetStatByName(string name) {
        if(name == "Materials") {
            return Stat.Materials;
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
            if(stat == Stat.Materials) {
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

        if(stat == Stat.Materials) {
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
}

public enum Stat {
    None,
    Materials,
    Food,
    Gold
}