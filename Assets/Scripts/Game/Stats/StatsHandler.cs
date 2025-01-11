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

    [Header("Stats")]
    public int economy = 100;
    public int happiness = 100;
    public int environment = 100;
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

    private void SetStartingStats() {
        Vector2Int randomEnvironmentRange = new Vector2Int(90, 98);
        Vector2Int randomHappinessRange = new Vector2Int(35, 45);
        Vector2Int randomEconomyRange = new Vector2Int(20, 30);

        environment = Random.Range(randomEnvironmentRange.x, randomEnvironmentRange.y);
        happiness = Random.Range(randomHappinessRange.x, randomHappinessRange.y);
        economy = Random.Range(randomEconomyRange.x, randomEconomyRange.y);
    }

    public void UpdateStats() {
        int unclampedEnvironment = environment;
        int unclampedHappiness = happiness;
        int unclampedEconomy = economy;

        foreach(Buildable buildable in PlacementHandler.instance.allPlacedBuildables) {
            unclampedEnvironment += buildable.environmentEffect;
            unclampedHappiness += buildable.happinessEffect;
            unclampedEconomy += buildable.economyEffect;
        }
        foreach(ConnectionGroup connectionGroup in ConnectionsHandler.instance.connectionGroups) {
            unclampedEnvironment += connectionGroup.connection.environmentEffect;
            unclampedHappiness += connectionGroup.connection.happinessEffect;
            unclampedEconomy += connectionGroup.connection.economyEffect;
        }
        
        environment = Mathf.Clamp(unclampedEnvironment, 0, 100);
        happiness = Mathf.Clamp(unclampedHappiness, 0, 100);
        economy = Mathf.Clamp(unclampedEconomy, 0, 100);

        UpdateUI();
    }
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
        //key: buildable name, value: (number of buildable, effect on stat)
        Dictionary<string, int> effects = new Dictionary<string, int>();

        if(stat == Stat.Environment) {
            foreach(Buildable buildable in PlacementHandler.instance.allPlacedBuildables) {
                if(effects.ContainsKey(buildable.name)) {
                    continue;
                }

                if(buildable.environmentEffect != 0) {
                    int sameBuildableCount = PlacementHandler.instance.GetPlacedBuildableCount(buildable.name);
                    effects.Add(buildable.name, buildable.environmentEffect * sameBuildableCount);
                }
            }
        } else if(stat == Stat.Happiness) {
            foreach(Buildable buildable in PlacementHandler.instance.allPlacedBuildables) {
                if(effects.ContainsKey(buildable.name)) {
                    continue;
                }

                if(buildable.happinessEffect != 0) {
                    int sameBuildableCount = PlacementHandler.instance.GetPlacedBuildableCount(buildable.name);
                    effects.Add(buildable.name, buildable.happinessEffect * sameBuildableCount);
                }
            }
        } else if(stat == Stat.Economy) {
            foreach(Buildable buildable in PlacementHandler.instance.allPlacedBuildables) {
                if(effects.ContainsKey(buildable.name)) {
                    continue;
                }

                if(buildable.economyEffect != 0) {
                    int sameBuildableCount = PlacementHandler.instance.GetPlacedBuildableCount(buildable.name);
                    effects.Add(buildable.name, buildable.economyEffect * sameBuildableCount);
                }
            }
        }

        //sort from highest to lowest effects
        effects = effects.OrderByDescending(e => e.Value).ToDictionary(e => e.Key, e => e.Value);

        return effects;
    }
    private Dictionary<string, int> GetStatConnectionEffects(Stat stat) {
        //key: connection name, value: (number of connection, effect on stat)
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
        //key: connection name, value: (number of connection, effect on stat)
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