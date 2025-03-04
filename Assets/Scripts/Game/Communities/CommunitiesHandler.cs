using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CommunitiesHandler : MonoBehaviour
{
    public static CommunitiesHandler instance;

    //List of the current communities during gameplay
    public List<Community> allCommunities = new List<Community>();

    [Header("References")]
    [SerializeField] private GameObject communityNamePrefab;

    private void Awake() {
        instance = this;
    }

    public void TryAddCommunities(GridTile tile) {

        if(tile == null) {
            return;
        }

        if(tile.currentBuildable == null) {
            return;
        }

        if(tile.currentBuildable.name == "Forest") {
            return;
        }

        //find all communities that have tiles neighboring the current tile
        List<Community> intersectingCommunities = allCommunities.Where(c => c.tiles.Any(t => tile.neighbors.Contains(t) && t.currentBuildable != null)).ToList();

        if(intersectingCommunities.Count == 0) {
            //no intersecting communities, create a new one
            CreateNewCommunity(tile);
        } else {
            //merge intersecting communities and add the new tile
            MergeCommunities(intersectingCommunities, tile);
        }

        UpdateCommunityVisuals();
    }
    private void CreateNewCommunity(GridTile tile)
    {
        Community newCommunity = new Community {
            name = GenerateRandomCommunityName(),
            tiles = new List<GridTile> { tile },
            population = 0,
            requiredPopulation = 0
        };

        //add valid (non-natural) neighbors to the new community
        foreach(GridTile neighbor in tile.neighbors) {

            if(IsValidForCommunity(neighbor) && !newCommunity.tiles.Contains(neighbor)) {
                newCommunity.tiles.Add(neighbor);
            }

        }

        allCommunities.Add(newCommunity);
    }
    private void MergeCommunities(List<Community> communitiesToMerge, GridTile tile)
    {
        //select the first community as the primary one
        Community primaryCommunity = communitiesToMerge[0];

        //merge other communities into the primary community
        for(int i = 1; i < communitiesToMerge.Count; i++) {

            Community communityToMerge = communitiesToMerge[i];

            foreach(GridTile t in communityToMerge.tiles) {
                if(!primaryCommunity.tiles.Contains(t)) {
                    primaryCommunity.tiles.Add(t);
                }
            }

            allCommunities.Remove(communityToMerge);
        
        }

        //add the new tile if not already present
        if(!primaryCommunity.tiles.Contains(tile)) {
            primaryCommunity.tiles.Add(tile);
        }

        //add valid (non-natural) neighbors
        foreach(GridTile neighbor in tile.neighbors) {
            if(IsValidForCommunity(neighbor) && !primaryCommunity.tiles.Contains(neighbor)) {
                primaryCommunity.tiles.Add(neighbor);
            }
        }
    }
    private bool IsValidForCommunity(GridTile tile)
    {
        return tile != null && tile.currentBuildable != null;
    }
    public Community GetCommunity(GridTile tile) {
        return allCommunities.FirstOrDefault(x => x.tiles.Contains(tile));
    }

    private string GenerateRandomCommunityName() {
        string[] adjectives = new string[] {"Green", "River", "Sky", "Sunny", "Stone"};
        string[] nouns = new string[] {"ridge", "henge", "town", "ville", "spire", "dale", "view"};
        
        string name = adjectives[Random.Range(0, adjectives.Length)] + nouns[Random.Range(0, nouns.Length)];

        int crashPreventionCount = 0; //if the attempted names exceeds the possible combinations then end the while loop
        while(allCommunities.Any(x => x.name == name) && crashPreventionCount < adjectives.Length * nouns.Length) {

            name = adjectives[Random.Range(0, adjectives.Length)] + nouns[Random.Range(0, nouns.Length)];

            crashPreventionCount++;
        }

        return name;
    }

    private List<GameObject> allCommunityNames = new List<GameObject>();
    private void UpdateCommunityVisuals() {
        foreach(GameObject communityName in allCommunityNames) {
            Destroy(communityName);
        }

        foreach(Community community in allCommunities) {

            Vector3 pos = GridHandler.instance.GetAverageTileListPosition(community.tiles) + new Vector3(0, 4, 0);
            GameObject communityName = Instantiate(communityNamePrefab, pos, Quaternion.identity);
            communityName.transform.Find("Display").Find("name").GetComponent<TMP_Text>().text = community.name;
            allCommunityNames.Add(communityName);

        }
    }

}

public class Community {
    public string name;
    public List<GridTile> tiles;
    public int population;
    public int requiredPopulation;
}