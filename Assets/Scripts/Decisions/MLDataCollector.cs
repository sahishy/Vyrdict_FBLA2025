using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class MLDataCollector : MonoBehaviour
{
    public static MLDataCollector instance;

    void Awake() {
        instance = this;
    }

    public void ImportMLData() {
        StartCoroutine(GetRequest("http://127.0.0.1:5000/"));
    }
    private IEnumerator GetRequest(string uri) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri)) {

            yield return webRequest.SendWebRequest();

            if(webRequest.result == UnityWebRequest.Result.ConnectionError) {
                Debug.Log("Error connecting to server: " + webRequest.error);
            } else {
                Debug.Log(webRequest.downloadHandler.text);
            }

        }
    }

    //exports the necessary data for the machine learning model to a json file
    //this file containing the data will be then read by a python script
    public void ExportMLData() {

        Dictionary<string, int> tempPlacedBuildables = new Dictionary<string, int>();
        foreach(PlacedBuildable placedBuildable in PlacementHandler.instance.GetPlacedBuildables()) {
            if(tempPlacedBuildables.ContainsKey(placedBuildable.buildable.name)) {
                continue;
            }

            tempPlacedBuildables.Add(placedBuildable.buildable.name, PlacementHandler.instance.GetPlacedBuildableCount(placedBuildable.buildable.name));
        }

        MLData data = new MLData {
            environment = StatsHandler.instance.GetStat(Stat.Environment),
            happiness = StatsHandler.instance.GetStat(Stat.Happiness),
            economy = StatsHandler.instance.GetStat(Stat.Economy),
            //activeFactors = FactorsHandler.instance.factors.Keys.ToList().ConvertAll(x => x.name),
            placedBuildables = tempPlacedBuildables,
            //population = StatsHandler.instance.population,
            //requiredPopulation = StatsHandler.instance.requiredPopulation,
            //weeksPassed = GameHandler.instance.currentWeek
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Application.persistentDataPath + "/MLData.json", json);
        //return json;
    }
}

public class MLData {
    public int environment;
    public int happiness;
    public int economy;
    //public List<string> activeFactors;
    public Dictionary<string, int> placedBuildables;
}