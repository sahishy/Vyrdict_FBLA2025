using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using Newtonsoft.Json;

public class MLDataHandler : MonoBehaviour
{
    public static MLDataHandler instance;
    public ResponseData currentData = null;

    void Awake() {
        instance = this;
    }

    private string uri = "http://127.0.0.1:1234/generate";

    public IEnumerator FetchMLResponse() {

        MLData data = GetMLData();

        byte[] encodedJson = Encoding.UTF8.GetBytes(JsonUtility.ToJson(data));

        using UnityWebRequest webRequest = new UnityWebRequest(uri, "POST");
        webRequest.uploadHandler = new UploadHandlerRaw(encodedJson);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError) {
            Debug.LogError("Error connecting to server: " + webRequest.error);
        } else {
            //success
            ResponseData response = JsonConvert.DeserializeObject<ResponseData>(webRequest.downloadHandler.text);

            currentData = response;

            // Debug.Log("Scenario: " + response.scenario);
            // Debug.Log($"Choice 1");
            // Debug.Log($"--------{response.choices.choice1.description}");
            // Debug.Log($"--------{response.choices.choice1.stat1}");
            // Debug.Log($"--------{response.choices.choice1.effect1}");
            // Debug.Log($"--------{response.choices.choice1.stat2}");
            // Debug.Log($"--------{response.choices.choice1.effect2}");
            // Debug.Log($"--------{response.choices.choice1.buildable}");
            // Debug.Log($"Choice 2: {response.choices.choice2.description}");
            // Debug.Log($"--------{response.choices.choice2.description}");
            // Debug.Log($"--------{response.choices.choice2.stat1}");
            // Debug.Log($"--------{response.choices.choice2.effect1}");
            // Debug.Log($"--------{response.choices.choice2.stat2}");
            // Debug.Log($"--------{response.choices.choice2.effect2}");
            // Debug.Log($"--------{response.choices.choice2.buildable}");
            // Debug.Log($"Choice 3: {response.choices.choice3.description}");
            // Debug.Log($"--------{response.choices.choice3.description}");
            // Debug.Log($"--------{response.choices.choice3.stat1}");
            // Debug.Log($"--------{response.choices.choice3.effect1}");
            // Debug.Log($"--------{response.choices.choice3.stat2}");
            // Debug.Log($"--------{response.choices.choice3.effect2}");
            // Debug.Log($"--------{response.choices.choice3.buildable}");
        }
    }

    private MLData GetMLData() {

        string[] tempPlacedBuildables = PlacementHandler.instance.GetPlacedBuildables().ConvertAll(x => x.buildable.name).Where(x => x != "Forest").OrderBy(x => x).ToArray();

        MLData data = new MLData {
            environment = StatsHandler.instance.GetStat(Stat.Materials),
            environmentChange = StatsHandler.instance.GetTotalStatChange(Stat.Materials),
            happiness = StatsHandler.instance.GetStat(Stat.Food),
            happinessChange = StatsHandler.instance.GetTotalStatChange(Stat.Food),
            economy = StatsHandler.instance.GetStat(Stat.Gold),
            economyChange = StatsHandler.instance.GetTotalStatChange(Stat.Gold),
            placedBuildables = tempPlacedBuildables
        };

        return data;
    }
}

public class MLData {
    public int environment;
    public int environmentChange;
    public int happiness;
    public int happinessChange;
    public int economy;
    public int economyChange;
    public string[] placedBuildables;
}

public class ResponseData {
    public string scenario;
    public ChoicesData choices;
}
public class ChoicesData {
    public ChoiceData choice1;
    public ChoiceData choice2;
    public ChoiceData choice3;
}
public class ChoiceData {
    public string description;
    public string stat1;
    public string effect1;
    public string stat2;
    public string effect2;
    public string buildable;
}