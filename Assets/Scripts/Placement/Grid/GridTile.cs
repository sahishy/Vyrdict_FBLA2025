using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class GridTile : MonoBehaviour, Interactable
{
    [Header("Tile")]
    public Buildable currentBuildable = null;
    [HideInInspector] public GameObject currentBuildableObject = null;
    public List<GridTile> neighbors = new List<GridTile>();
    //root
    [Header("Connections - Root")]
    public Color32 rootColor;
    public Connection rootConnection = null;
    public List<GridTile> branchNeighbors = new List<GridTile>();
    [Header("Connections - Branch")]
    public GridTile rootNeighbor = null; 
    //public Dictionary<Direction, GridTile> neighbors = new Dictionary<Direction, GridTile>();

    [Header("References")]
    public Transform holder;
    public GameObject focusDisplay;

    public List<GameObject> tileModels = new List<GameObject>();
    private GameObject tileModel;

    void Start() {
        rootColor = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255); 
    }

    //-----------------------------BUILDABLE-----------------------------

    public void Initialize(Buildable buildable = null) {
        //CREATING BASE TILE
        GameObject model = tileModels[0];

        if (model != null)
        {
            tileModel = Instantiate(model, holder);
            tileModel.transform.localPosition = Vector3.zero;
        }

        //ADDING STARTER OBJECT - natural buildable that is generated with tile, ex. forest buildable
        if(buildable != null) {
            AddBuildable(buildable, PlacementHandler.instance.rotations[Random.Range(0, 5)]);
        }
    }    

    public void AddBuildable(Buildable buildable, Vector3 rotation) {
        //DELETE ANY EXISTING BUILDABLE IF ANY
        if(currentBuildable != null) {
            PlacementHandler.instance.RemoveBuildable(this);
            RemoveBuildable();            
        }

        //CREATE NEW BUILDABLE
        currentBuildable = buildable;

        currentBuildableObject = Instantiate(currentBuildable.tile, holder);
        currentBuildableObject.transform.localScale = Vector3.zero;
        currentBuildableObject.transform.localPosition = Vector3.zero;
        currentBuildableObject.transform.localRotation = Quaternion.Euler(rotation);

        currentBuildableObject.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce);
    }
    public void RemoveBuildable() {
        currentBuildable = null;

        Destroy(currentBuildableObject);
    }

    //-----------------------------INTERACTABLE-----------------------------

    public void Interact()
    {
        // if(!PlacementHandler.instance.inPlacementMode) { 
        //     int count = 0;
        //     List<ConnectionGroup> connectionGroups = ConnectionsHandler.instance.GetConnectionGroups(this);
        //     foreach(ConnectionGroup connectionGroup in connectionGroups) {
            
        //         Debug.Log($"{count}: {connectionGroup.connection.name}");
        //         foreach(GridTile tile in connectionGroup.tiles) {
        //             Debug.Log($"------{(tile == this ? "(THIS) " : "")}{tile.currentBuildable} @ {tile.gameObject.name}");
        //         }
                
        //         count++;

        //     }
        // }

        Display();
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    //-----------------------------DISPLAY-----------------------------

    private void Display() {
        if(currentBuildable != null) {
            TileDisplayHandler.instance.ShowDisplay(this);
        } else {
            TileDisplayHandler.instance.HideDisplay();
        }
    }

    public void ToggleFocusDisplay(bool toggle) {
        focusDisplay.SetActive(toggle);
    }
}
