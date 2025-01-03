using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionsHandler : MonoBehaviour
{
    public static ConnectionsHandler instance;
    //List of all connections
    [HideInInspector] public List<Connection> allConnections;
    //List of the current connections during gameplay
    public List<ConnectionGroup> connectionGroups = new List<ConnectionGroup>();

    [Header("References")]
    [SerializeField] private GameObject connectionLinePrefab;

    void Awake() {
        instance = this;
        
        //Load all connections
        allConnections = Resources.LoadAll<Connection>("Connections").ToList();
    }

    public void TryAddConnections(GridTile tile) {

        //key: connection - value: connecting tiles
        Dictionary<Connection, List<GridTile>> connections = GetConnections(tile);

        //loop through each connection
        foreach(Connection connection in connections.Keys) {
            List<GridTile> connectingTiles = connections[connection];

            //find all groups that intersect with the connecting tiles
            List<ConnectionGroup> intersectingGroups = connectionGroups.Where(group => group.connection == connection && group.tiles.Any(t => connectingTiles.Contains(t))).ToList();

            if (intersectingGroups.Count == 0) {

                //no intersecting groups, create a new group
                ConnectionGroup newGroup = new ConnectionGroup {
                    connection = connection,
                    tiles = new List<GridTile>(connectingTiles)
                };
                newGroup.tiles.Add(tile); //include the initiating tile
                connectionGroups.Add(newGroup);

            } else {

                //merge intersecting groups and add the tile
                ConnectionGroup mergedGroup = intersectingGroups[0]; //first group will be the primary

                //loop through each group to merge with the primary group, skip the primary when looping
                foreach(ConnectionGroup group in intersectingGroups.Skip(1)) {

                    //merge tiles from the other group into the primary group
                    foreach(GridTile t in group.tiles) {
                        if(!mergedGroup.tiles.Contains(t)) {
                            mergedGroup.tiles.Add(t);
                        }
                    }
                    //remove the group that was just merged
                    connectionGroups.Remove(group);

                }

                //add the current tile to the merged group if it's not already in it
                if(!mergedGroup.tiles.Contains(tile)) {
                    mergedGroup.tiles.Add(tile);
                }

                //add any newly connecting tiles to the merged group
                foreach(GridTile t in connectingTiles) {
                    if(!mergedGroup.tiles.Contains(t)) {
                        mergedGroup.tiles.Add(t);
                    }
                }

            }
        }

        UpdateVisuals();

        foreach(ConnectionGroup connectionGroup in connectionGroups) {
            Connection connection = connectionGroup.connection;
            List<GridTile> gridTiles = connectionGroup.tiles;
            Debug.Log($"{connection.name} ({gridTiles.Count - 1}x)");
        }
    }

    private List<ConnectionLine> allLines = new List<ConnectionLine>();
    private void UpdateVisuals() {

        foreach(ConnectionLine line in allLines) {
            if(line == null) {
                continue;
            }
            Destroy(line.gameObject);
        }

        foreach(ConnectionGroup connectionGroup in connectionGroups) {

            List<GridTile> gridTiles = connectionGroup.tiles;
            for(int i = 0; i < gridTiles.Count; i++) {

                Transform target0 = gridTiles[i].transform;
                Transform target1 = gridTiles.Count > (i + 1) ? gridTiles[i + 1].transform : gridTiles[0].transform;

                ConnectionLine line = Instantiate(connectionLinePrefab, Vector3.zero, Quaternion.identity).GetComponent<ConnectionLine>();
                line.Initialize(target0, target1);
                allLines.Add(line);

            }

        }

    }

    //Returns all of the connection groups a tile is in (OFFICIAL CONNECTIONS)
    public List<ConnectionGroup> GetConnectionGroups(GridTile tile) {

        //list of the connection groups the tile is in that will be returned
        List<ConnectionGroup> tileConnectionGroups = new List<ConnectionGroup>();

        //loop through all the connection groups
        foreach(ConnectionGroup connectionGroup in connectionGroups) {
            
            //store the group if the group contains the tile
            if(connectionGroup.tiles.Contains(tile)) {
                tileConnectionGroups.Add(connectionGroup);
            }

        }

        return tileConnectionGroups;

    }

    //Returns the possible connections a tile can have (NON OFFICIAL CONNECTIONS)
    public Dictionary<Connection, List<GridTile>> GetConnections(GridTile tile) {

        //List of connections the tile can have that will be returned
        Dictionary<Connection, List<GridTile>> connections = new Dictionary<Connection, List<GridTile>>();

        //Buildable on tile
        Buildable tileBuildable = tile.currentBuildable;
        //Filter connections so it only leaves those that include the buildable, as the connection wouldn't be possible
        List<Connection> possibleConnections = allConnections.Where(x => x.rootBuildables.Contains(tileBuildable) || x.branchBuildables.Contains(tileBuildable)).ToList();
        //Filter neighbors so it only leaves existing neighbors (non-water)
        List<GridTile> possibleNeighbors = tile.neighbors.Where(x => x != null).ToList();

        //Loop through all possible connections
        foreach(Connection connection in possibleConnections) {

            //store if tile is root and/or branch
            bool tileIsRoot = connection.rootBuildables.Contains(tileBuildable);
            bool tileIsBranch = connection.branchBuildables.Contains(tileBuildable);

            //loop through all neighbors of tile
            foreach(GridTile neighbor in possibleNeighbors) {

                //buildable on neighbor
                Buildable neighborBuildable = neighbor.currentBuildable;

                //store if neighbor is root and/or branch
                bool neighborIsRoot = connection.rootBuildables.Contains(neighborBuildable);
                bool neighborIsBranch = connection.branchBuildables.Contains(neighborBuildable);

                //check if tile is a root and the neighbor is a branch
                if((tileIsRoot && neighborIsBranch) || (tileIsBranch && neighborIsRoot)) {

                    //store connection and the connecting neighbor
                    if(connections.ContainsKey(connection)) {
                        if(!connections[connection].Contains(neighbor)) {
                            connections[connection].Add(neighbor);
                        }
                    } else {
                        connections.Add(connection, new List<GridTile>());
                        connections[connection].Add(neighbor);
                    }
                    
                } 

            }

        }   

        return connections;

    }

    public int GetConnectionCount(string name) {
        int count = 0;
        foreach(ConnectionGroup connectionGroup in connectionGroups.Where(x => x.connection.name == name)) {
            count += connectionGroup.tiles.Count - 1;
        }
        return count;
    }
}

public class ConnectionGroup {
    public Connection connection;
    public List<GridTile> tiles;
    public Color32 color;
}