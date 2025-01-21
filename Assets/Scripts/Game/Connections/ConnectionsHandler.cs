using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConnectionsHandler : MonoBehaviour
{
    public static ConnectionsHandler instance;
    //List of all connections
    [HideInInspector] public List<Connection> allConnections;
    //List of the current connections during gameplay
    public List<ConnectionGroup> connectionGroups = new List<ConnectionGroup>();

    [Header("References")]
    public Transform connectionLineHolder;
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
                    id = Guid.NewGuid().ToString(),
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

        UpdateConnectionVisuals();
    }

    //Refreshes a connection group after a tile in it is changed
    public void UpdateConnectionGroupsWithTile(GridTile tile) {
        List<ConnectionGroup> groupsWithTile = GetConnectionGroups(tile);

        //remove tile from groups
        foreach(ConnectionGroup group in groupsWithTile) {
            group.tiles.Remove(tile);
        }

        //try to add tile back to group (in the case that new tile still has a connection)
        TryAddConnections(tile);

    }

    private Dictionary<ConnectionGroup, List<ConnectionLine>> allLines = new Dictionary<ConnectionGroup, List<ConnectionLine>>();
    private void UpdateConnectionVisuals() {

        foreach(List<ConnectionLine> lines in allLines.Values) {
            foreach(ConnectionLine line in lines) {
                if(line == null) {
                    continue;
                }
                Destroy(line.gameObject);                
            }
        }

        foreach(ConnectionGroup connectionGroup in connectionGroups) {

            List<GridTile> gridTiles = connectionGroup.tiles;
            //gridTiles.Sort((a, b) => GridHandler.instance..name.CompareTo(b.connection.name));
            for(int i = 0; i < gridTiles.Count; i++) {

                //CREATE HOLDER IF IT DOESN'T EXIST
                Transform lineHolder = connectionLineHolder.Find(connectionGroup.id);
                if(lineHolder == null) {
                    lineHolder = new GameObject(connectionGroup.id).transform;
                    lineHolder.gameObject.SetActive(false);
                    lineHolder.parent = connectionLineHolder;                    
                }

                //GET LINE TARGETS
                Transform target0 = gridTiles[i].transform;
                Transform target1 = gridTiles.Count > (i + 1) ? gridTiles[i + 1].transform : gridTiles[0].transform;

                //CREATE LINE
                ConnectionLine line = Instantiate(connectionLinePrefab, Vector3.zero, Quaternion.identity).GetComponent<ConnectionLine>();
                line.Initialize(target0, target1);
                line.gameObject.name = gridTiles[i].gameObject.name;
                line.transform.parent = lineHolder;

                //STORE LINES
                if(allLines.TryGetValue(connectionGroup, out List<ConnectionLine> lines)) {
                    lines.Add(line);
                } else {
                    allLines.Add(connectionGroup, new List<ConnectionLine>() {line});
                }
               

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
    public Dictionary<Connection, List<GridTile>> GetConnections(GridTile tile, Buildable buildable = null) {

        //List of connections the tile can have that will be returned
        Dictionary<Connection, List<GridTile>> connections = new Dictionary<Connection, List<GridTile>>();

        //Buildable on tile - use provided buildable if its there
        Buildable tileBuildable = buildable != null ? buildable : tile.currentBuildable;
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

    //Returns ALL of the tiles that a tile would connect to IF a buildable was placed on it (MAINLY USED FOR PLACEMENT)
    public Dictionary<Connection, List<GridTile>> GetConnectionTilesFromTileBuildable(GridTile tile, Buildable buildable) {
        Dictionary<Connection, List<GridTile>> targetTiles = new Dictionary<Connection, List<GridTile>>();

        //get all possible connections for the tile and buildable
        Dictionary<Connection, List<GridTile>> connections = GetConnections(tile, buildable);
        
        //store direct tiles
        foreach(Connection connection in connections.Keys) {
            List<GridTile> connectingTiles = connections[connection];
            foreach(GridTile connectingTile in connectingTiles) {
                if(targetTiles.ContainsKey(connection)) {
                    if(!targetTiles[connection].Contains(connectingTile)) {
                        targetTiles[connection].Add(connectingTile);
                    }
                } else {
                    targetTiles.Add(connection, new List<GridTile>());
                    targetTiles[connection].Add(connectingTile);
                }
            }
        }

        //get all connection groups of each tile if any
        List<ConnectionGroup> groups = new List<ConnectionGroup>();
        foreach(Connection connection in connections.Keys) {
            List<GridTile> connectingTiles = connections[connection];
            foreach(GridTile connectingTile in connectingTiles) {

                List<ConnectionGroup> _groups = GetConnectionGroups(connectingTile);
                foreach(ConnectionGroup group in _groups) {
                    
                    //make sure the group is not already in the list and the group is a valid connection
                    if(!groups.Contains(group) && group.connection == connection) {
                        groups.Add(group);
                    }
                }

            }
        }

        //store indirect tiles (in connection groups)
        foreach(ConnectionGroup group in groups) {
            foreach(GridTile connectingTile in group.tiles) {
                if(targetTiles.ContainsKey(group.connection)) {
                    if(!targetTiles[group.connection].Contains(connectingTile)) {
                        targetTiles[group.connection].Add(connectingTile);
                    }
                } else {
                    targetTiles.Add(group.connection, new List<GridTile>());
                    targetTiles[group.connection].Add(connectingTile);
                }
            }
        }

        return targetTiles;
    }

    //Get the number of connecting tiles a connection has
    public int GetConnectionCount(string name) {
        int count = 0;
        foreach(ConnectionGroup connectionGroup in connectionGroups.Where(x => x.connection.name == name)) {
            count += connectionGroup.tiles.Count - 1;
        }
        return count;
    }

    //Returns whether a connection's net effect is benefiting the stats or decreasing the stats
    public bool GetConnectionPositive(Connection connection) {
        return connection.environmentEffect + connection.happinessEffect + connection.economyEffect >= 0;
    }
    
    //Return whether a buildable's required connection is met
    public bool RequiredConnectionMet(GridTile tile) {
        return GetConnectionGroups(tile).Any(x => x.connection == tile.currentBuildable.requiredConnection);
    }

}

public class ConnectionGroup {
    public string id;
    public Connection connection;
    public List<GridTile> tiles;
    public Color32 color;
}