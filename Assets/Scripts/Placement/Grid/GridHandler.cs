using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

public class GridHandler : MonoBehaviour
{
    public static GridHandler instance;

    private Dictionary<Vector2, GridTile> gridCells = new Dictionary<Vector2, GridTile>();
    public float cellSize = 1;

    [Header("Generation")]
    public Vector2 size = new Vector2(10, 10);
    public float noiseFrequency = 2f;
    public float waterThreshold = 0.1f;
    public float natureThreshold = 0.4f;
    [Header("Generation References")]
    public Transform gridCellHolder;
    public GameObject gridCellPrefab;
    public List<Buildable> natureBuildables = new List<Buildable>();

    void Awake() {
        instance = this;
    }

    void Start()
    {
        Invoke(nameof(CreateGrid), 1);
    }

    void CreateGrid() {

        //CREATING TILES

        for(int i = 0; i < size.x; i++) {

            for(int j = 0; j < size.y; j++) {
                
                //GET CELL POS
                Vector2 gridPos = new Vector2(i, j);
                Vector3 pos = GetHexPos((int)gridPos.x, (int)gridPos.y);

                float seed = Random.Range(0, 1000000);
                float waterValue = Mathf.PerlinNoise((pos.x + seed) / noiseFrequency, (pos.z + seed) / noiseFrequency);
                float falloffValue = GetFalloff(i, j);
                float combinedValue = waterValue * falloffValue;

                if(combinedValue < waterThreshold) {
                    //STORE CELL AS EMPTY TILE / WATER TILE IN DICTIONARY
                    gridCells.Add(gridPos, null);

                    continue;
                }

                //CREATE CELL
                GridTile gridTile = CreateGridCell(pos, gridPos);
                //STORE CELL IN DICTIONARY
                gridCells.Add(gridPos, gridTile);

                //CHECK IF CELL HAS NATURE ELEMENTS ON IT
                float natureValue = Mathf.PerlinNoise((pos.x + seed * 2) / noiseFrequency, (pos.z + seed * 2) / noiseFrequency);

                //ADD NATURE ELEMENTS IF IT DOES
                if(natureValue < natureThreshold) {

                    Buildable randomNatureBuildable = natureBuildables[Random.Range(0, natureBuildables.Count)];

                    gridTile.Initialize(randomNatureBuildable);
                    
                    //Update stats
                    //StatsHandler.instance.UpdateStats(randomNatureBuildable);

                    continue;
                }
                //INITIALIZE GRID TILE IF NOT INITIALIZED ALREADY
                gridTile.Initialize();

            }

        }

        //ASSIGNING NEIGHBORS - USED IN GetConnections()
        foreach(var cell in gridCells) {
            Vector2 cellPos = cell.Key;
            GridTile cellTile = cell.Value;
            
            if(cellTile != null) {
                cellTile.neighbors = GetHexNeighbors(cellPos);
            }
        }

        //-----------------------EXTRA-----------------------

        //CREATE THE STARTING TWO HOUSES
        List<GridTile> tilesWithoutBuildables = GetUnoccupiedTiles();
        //scramble the tiles without buildables
        tilesWithoutBuildables = tilesWithoutBuildables.OrderBy(x => Random.value).ToList();
        //keep track of available spots for the houses
        (GridTile, GridTile)? availableSpots = null;
        //attempt to find available spots
        foreach(GridTile tile in tilesWithoutBuildables) {
            List<GridTile> unoccupiedNeighbors = tile.neighbors.Where(x => tilesWithoutBuildables.Contains(x)).ToList();
            if(unoccupiedNeighbors.Count != 0) {
                GridTile randomHouse1 = tile;
                GridTile randomHouse2 = unoccupiedNeighbors[Random.Range(0, unoccupiedNeighbors.Count)];

                availableSpots = new (randomHouse1, randomHouse2);

                break;
            }
        } 
        //spawn two houses if there are available spots - there should always be atleast 2 available spots
        if(availableSpots != null) {
            Buildable houseBuildable = Resources.Load<Buildable>("Buildables/Tent");
            PlacementHandler.instance.AddBuildable(houseBuildable, PlacementHandler.instance.rotations[1], availableSpots.Value.Item1);
            PlacementHandler.instance.AddBuildable(houseBuildable, PlacementHandler.instance.rotations[0], availableSpots.Value.Item2);
        }

        //ASSIGN CONNECTIONS IF ANY (primarily for the two tents)
        //loop through all the tiles that started with a buildable
        List<GridTile> tilesWithBuildables = gridCells.Values.Where(x => x != null && x.currentBuildable != null).ToList();
        foreach(GridTile tile in tilesWithBuildables) {
            ConnectionsHandler.instance.TryAddConnections(tile);
        }

        //ASSIGN COMMUNITIES IF ANY (primarily for the two tents)
        //try to add or merge communities
        foreach(GridTile tile in tilesWithBuildables) {
            CommunitiesHandler.instance.TryAddCommunities(tile);
        }

        //-----------------------START GAME-----------------------
        //makes sure that map is created before game 'actually' starts
        StartCoroutine(GameHandler.instance.StartGame());
    }

    private GridTile CreateGridCell(Vector3 pos, Vector2 gridPos) {
        float animationTime = 0.5f;
        float animationTimeOffset = Random.Range(0, 0.2f);

        GameObject cell = Instantiate(gridCellPrefab, gridCellHolder);
        cell.transform.localScale = Vector3.zero;
        cell.transform.position = pos;
        cell.name = $"GridCell ({gridPos.x}, {gridPos.y})";

        cell.transform.DOScale(Vector3.one, animationTime + animationTimeOffset).SetEase(Ease.OutBounce);

        return cell.GetComponent<GridTile>();
    }

    private List<GridTile> GetHexNeighbors(Vector2 pos) {
        // Dictionary<Direction, GridTile> neighbors = new Dictionary<Direction, GridTile>() {
        //     {Direction.TopLeft, null},
        //     {Direction.TopRight, null},
        //     {Direction.Left, null},
        //     {Direction.Right, null},
        //     {Direction.BottomLeft, null},
        //     {Direction.BottomRight, null},
        // };
        List<GridTile> neighbors = new List<GridTile>();

        bool even = pos.y % 2 == 0;

        List<Vector2> evenDirections = new List<Vector2>() {
            new Vector2(-1, 1), // Top left
            new Vector2(0, 1),  // Top right
            new Vector2(-1, 0), // Left
            new Vector2(1, 0),  // Right
            new Vector2(-1, -1),// Bottom left
            new Vector2(0, -1)  // Bottom right
        };
        List<Vector2> oddDirections = new List<Vector2>() {
            new Vector2(0, 1),  // Top left
            new Vector2(1, 1),  // Top right
            new Vector2(-1, 0), // Left
            new Vector2(1, 0),  // Right
            new Vector2(0, -1), // Bottom left
            new Vector2(1, -1)  // Bottom right
        };
        List<Vector2> directions = even ? evenDirections : oddDirections;
        Dictionary<Direction, Vector2> directionsDictionary = new Dictionary<Direction, Vector2>() {
            {Direction.TopLeft, directions[0]},
            {Direction.TopRight, directions[1]},
            {Direction.Left, directions[2]},
            {Direction.Right, directions[3]},
            {Direction.BottomLeft, directions[4]},
            {Direction.BottomRight, directions[5]}
        };

        foreach(Direction direction in directionsDictionary.Keys)
        {
            //COORDINATE OF NEIGHBOR CELL
            Vector2 neighborCoord = pos + directionsDictionary[direction];
            //CHECK IF NEIGHBOR TILE EXISTS
            if(gridCells.TryGetValue(neighborCoord, out GridTile neighborTile)) {
                //CHECK IF NEIGHBOR TILE IS LAND (NOT WATER)

                neighbors.Add(neighborTile);
                //---STORING DIRECTIONS---
                // if(neighborTile != null) {
                //     neighbors[direction] = neighborTile;
                // }

            }
        }

        return neighbors;
    }
    
    private Vector3 GetHexPos(int x, int y) {
        float xPos = x * cellSize + ((y % 2 == 1) ? (cellSize / 2) : 0);
        float yPos = y * cellSize * Mathf.Cos(Mathf.Deg2Rad * 30);
        return new Vector3(xPos, 1.2f, yPos);
    }

    private float GetFalloff(float x, float y) {
        float halfWidth = size.x / 2f;
        float halfHeight = size.y / 2f;

        float nx = (x - halfWidth) / halfWidth;
        float ny = (y - halfHeight) / halfHeight;

        float distance = Mathf.Sqrt(nx * nx + ny * ny); 
        float falloff = Mathf.Clamp01(1f - distance); 

        return falloff;
    }

    public List<GridTile> GetUnoccupiedTiles() {
        return gridCells.Values.Where(x => x != null && x.currentBuildable == null).ToList();
    }
    public List<GridTile> GetOccupiedTiles() {
        return gridCells.Values.Where(x => x != null && x.currentBuildable != null).ToList();
    }

    public Vector3 GetAverageTileListPosition(List<GridTile> tiles) {
        List<Vector3> tilePositions = tiles.ConvertAll(tile => tile.transform.position);
        Vector3 averagePosition = new Vector3(
            tilePositions.Average(pos => pos.x),
            tilePositions.Average(pos => pos.y),
            tilePositions.Average(pos => pos.z)
        );
        return averagePosition;
    }
}

public enum Direction {
    TopLeft,
    TopRight,
    Left,
    Right,
    BottomLeft,
    BottomRight
}
