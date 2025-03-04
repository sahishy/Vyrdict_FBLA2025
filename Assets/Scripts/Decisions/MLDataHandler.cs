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
    public NormalResponseData currentNormalData = null;
    public CrisisResponseData currentCrisisData = null;

    void Awake() {
        instance = this;
    }

    private string normalURI = "http://127.0.0.1:1234/generate/normal";
    private string crisisURI = "http://127.0.0.1:1234/generate/crisis";

    public IEnumerator FetchMLResponse(bool isCrisis) {

        string uri = isCrisis ? crisisURI : normalURI;

        MLData data = GetMLData();

        byte[] encodedJson = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));

        using UnityWebRequest webRequest = new UnityWebRequest(uri, "POST");
        webRequest.uploadHandler = new UploadHandlerRaw(encodedJson);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError) {
            Debug.LogError("Error connecting to server: " + webRequest.error);
        } else {
            //success

            if(isCrisis) {
                CrisisResponseData response = JsonConvert.DeserializeObject<CrisisResponseData>(webRequest.downloadHandler.text);
                currentCrisisData = response;

                Debug.Log(JsonConvert.SerializeObject(response, Formatting.Indented));
            } else {
                NormalResponseData response = JsonConvert.DeserializeObject<NormalResponseData>(webRequest.downloadHandler.text);
                currentNormalData = response;

                Debug.Log(JsonConvert.SerializeObject(response, Formatting.Indented));
            }

        }
    }

    private MLData GetMLData() {

        string[] tempPlacedBuildables = PlacementHandler.instance.GetPlacedBuildables().ConvertAll(x => x.buildable.name).Where(x => x != "Forest").OrderBy(x => x).ToArray();

        MLData data = new MLData {
            supplies = StatsHandler.instance.GetStat(Stat.Supplies),
            suppliesChange = StatsHandler.instance.GetTotalStatChange(Stat.Supplies),
            food = StatsHandler.instance.GetStat(Stat.Food),
            foodChange = StatsHandler.instance.GetTotalStatChange(Stat.Food),
            gold = StatsHandler.instance.GetStat(Stat.Gold),
            goldChange = StatsHandler.instance.GetTotalStatChange(Stat.Gold),
            fate = GameHandler.instance.fateTimer,
            placedBuildables = tempPlacedBuildables
        };

        return data;
    }
}

public class MLData {
    public int supplies;
    public int suppliesChange;
    public int food;
    public int foodChange;
    public int gold;
    public int goldChange;
    public int fate;
    public string[] placedBuildables;
}

public class NormalResponseData {
    public string scenario;
    public Dictionary<string, NormalChoiceData> choices;
}
public class NormalChoiceData {
    public string description;
    public string stat1;
    public int effect1;
    public string stat2;
    public int effect2;
    public int fateEffect;
}
public class CrisisResponseData {
    public string crisis;
    public Dictionary<string, CrisisChoiceData> choices;
}
public class CrisisChoiceData {
    public string description;
    public string stat;
    public int effect;
}