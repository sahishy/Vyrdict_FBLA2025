using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FactorsHandler : MonoBehaviour
{
    public static FactorsHandler instance;

    private List<Factor> allFactors = new List<Factor>();
    public Dictionary<Factor, FactorUI> factors = new Dictionary<Factor, FactorUI>();

    [SerializeField] private List<Factor> startingEvents = new List<Factor>();

    [Header("References")]
    [SerializeField] private GameObject factorPrefab;
    [SerializeField] private Transform factorHolder;

    void Awake() {
        instance = this;

        allFactors = Resources.LoadAll<Factor>("Factors").ToList();
    }

    public void TryAddRandomFactor() {
        
        if(factors.Count < 3) {
            bool activeEvent = factors.Any(x => x.Key.factorType == FactorType.Event);
            bool activeDemand = factors.Any(x => x.Key.factorType == FactorType.Demand);
            bool activeQuest = factors.Any(x => x.Key.factorType == FactorType.Quest);

            Debug.Log($"#{factors.Keys.Count} | Event: {activeEvent} | Demand: {activeDemand} | Quest: {activeQuest}");

            Factor randomFactor = null;

            if(!activeEvent) {
                randomFactor = GetRandomFactor(FactorType.Event);
            } else if(!activeDemand) {
                randomFactor = GetRandomFactor(FactorType.Demand);
            } else if(!activeQuest) {
                randomFactor = GetRandomFactor(FactorType.Quest);
            }

            if(randomFactor != null) {
                Debug.Log(randomFactor.name);
                AddFactor(randomFactor);  
            } else {
                Debug.Log(null);
            }            
        }

    }

    //Starts off the game with the starting events
    public void AddStartingEvents() {
        AddFactor(startingEvents[Random.Range(0, startingEvents.Count)]);
    }

    //Returns a random factor based on the player's current situation
    private Factor GetRandomFactor(FactorType type) {

        //all possible factors to choose from
        List<Factor> possibleFactors = new List<Factor>();
        //ensure that the starting factors aren't considered (they are only supposed to be for the start of the game)
        possibleFactors = allFactors.Where(x => !startingEvents.Contains(x)).ToList();
        //get a random factor that is the given type
        possibleFactors = allFactors.Where(x => x.factorType == type).ToList();

        int environment = StatsHandler.instance.GetStat(Stat.Environment);
        int happiness = StatsHandler.instance.GetStat(Stat.Happiness);
        int economy = StatsHandler.instance.GetStat(Stat.Economy);

        //list of best factors based on player's situation
        List<Factor> bestFactors = new List<Factor>();

        foreach(Factor factor in possibleFactors) {
            int happinessChange = factor.statEffect == Stat.Happiness ? factor.statEffectValue : 0;
            int economyChange = factor.statEffect == Stat.Economy ? factor.statEffectValue : 0;
            int environmentChange = factor.statEffect == Stat.Environment ? factor.statEffectValue : 0;

            if(happiness < 30 && happinessChange > 0) {
                bestFactors.Add(factor);
            
            } else if(economy > 80 && economyChange < 0) {
                bestFactors.Add(factor);

            } else if(environment > 80 && environmentChange < 0) {
                bestFactors.Add(factor);
            
            }
        }

        //Return a random 'best' factor
        if(bestFactors.Count > 0) {
            Debug.Log("Hi");
            Factor bestFactor = bestFactors[Random.Range(0, bestFactors.Count)];
            int allBestFactorsCount = 0;

            while(factors.ContainsKey(bestFactor) && allBestFactorsCount < bestFactors.Count) {
                bestFactor = bestFactors[Random.Range(0, bestFactors.Count)];
                allBestFactorsCount++;
            }
            
            return bestFactor;
        }

        //Return a random factor if there was an issue getting the best factor
        return possibleFactors[Random.Range(0, possibleFactors.Count)];
    }

    public void UpdateFactors() {
        
        Dictionary<Factor, FactorUI> temp = new Dictionary<Factor, FactorUI>(factors);

        foreach(var factorPair in temp) {
            Factor factor = factorPair.Key;
            FactorUI factorUI = factorPair.Value;

            //End factor if requirement is met
            if(factor.factorType == FactorType.Demand || factor.factorType == FactorType.Quest) {
                
                if(factor.requirement == FactorRequirement.Buildable) {
                    
                    int currentBuildableCount = PlacementHandler.instance.GetPlacedBuildableCount(factor.requiredBuildable.name);
                    int placedBuildables = currentBuildableCount - factorUI.startingBuildableCount;
                    if(placedBuildables >= factor.requiredBuildableCount) {

                        //Factor is completed, buildable requirement was met

                        if(factor.factorType == FactorType.Quest) {
                            //give reward
                            Debug.Log("Buildable requirement met");
                        }

                        factorUI.DestroyFactor();
                    }

                } else if(factor.requirement == FactorRequirement.Stat) {

                    if(StatsHandler.instance.GetStat(factor.requiredStat) > factor.requiredStatValue) {

                        //Factor is completed, stat requirement was met

                        if(factor.factorType == FactorType.Quest) {
                            //give reward
                            Debug.Log("Stat requirement met");
                        }

                        factorUI.DestroyFactor();
                    }

                }
            }
        }

    }

    public void AddFactor(Factor factor) {
        FactorUI newFactor = Instantiate(factorPrefab, factorHolder).GetComponent<FactorUI>();
        newFactor.Initialize(factor);

        factors.Add(factor, newFactor);
    }

    public void RemoveFactor(FactorUI factorUI) {
        factors.Remove(factors.FirstOrDefault(x => x.Value == factorUI).Key);
    }

}
