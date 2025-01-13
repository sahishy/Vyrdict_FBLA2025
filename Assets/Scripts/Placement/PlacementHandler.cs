using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PlacementHandler : MonoBehaviour, Animatable
{
    public static PlacementHandler instance;

    [Header("Placement")]
    public List<Buildable> allPlacedBuildables = new List<Buildable>();
    [HideInInspector] public bool inPlacementMode = false;
    private Buildable currentBuildable;
    [HideInInspector] public GameObject currentGhost;
    private GridTile currentTile;
    
    public Vector3 currentRotation = new Vector3(0, 150, 0);
    private int rotationStep = 0;
    public Vector3[] rotations = new Vector3[] {
        new Vector3(0, 150, 0),
        new Vector3(0, 210, 0),
        new Vector3(0, 270, 0),
        new Vector3(0, 330, 0),
        new Vector3(0, 30, 0),
        new Vector3(0, 90, 0),
    };

    [Header("References")]
    [SerializeField] private GameObject placementPanel;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform cancelButton;
    [SerializeField] private Transform placeButtonFocusHolder;
    [SerializeField] private CanvasGroup placeButton;
    [SerializeField] private Transform rotateButton;
    [SerializeField] private Material positiveHighlightMaterial;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material negativeHighlightMaterial;
    [SerializeField] private Material overlayHighlightMaterial;
    [SerializeField] private Material overlayNegativeHighlightMaterial;

    //stores all of the highlighted meshRenderers when displaying connections
    private Dictionary<MeshRenderer, Material[]> connectionMeshRenderers = new Dictionary<MeshRenderer, Material[]>();

    void Awake() {
        instance = this;

        //place button focus animation
        StartCoroutine(StartPlaceButtonFocusAnimation());
    }

    void Update() {
        PlacementMode();

        // if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !inPlacementMode) {
        //     if(inEditMode) {
        //         ExitEditMode();
        //     } else {
        //         EnterEditMode();
        //     }
        // }
    }

    public void EnterPlacementMode(Buildable buildable) {
        inPlacementMode = true;
        currentBuildable = buildable;

        Camera.main.DOFieldOfView(Camera.main.fieldOfView - 10, 0.5f);

        //Toggle all active UI
        GameHandler.instance.currentFocusedAnimatable?.AnimatableExit();
        TileDisplayHandler.instance.HideDisplay();
        DialogueHandler.instance.CloseDialogue();

        //Placement Visuals
        TogglePlacementUI(true);
        CreateGhost(buildable);
    }
    public void ExitPlacementMode() {
        inPlacementMode = false;
        currentBuildable = null;

        Camera.main.DOFieldOfView(Camera.main.fieldOfView + 10, 0.5f);

        rotationStep = 0;
        currentRotation = rotations[rotationStep];

        DestroyGhost();
        TogglePlacementUI(false);
    }

    private void TogglePlacementUI(bool value) {
        placementPanel.SetActive(value);
        inventoryPanel.SetActive(!value);
    }

    private void Place() {

        GridTile gridTile = PlayerInteraction.instance.GetFocusedGridTile();

        if(gridTile != null) {
            //makes sure buildable can be placed
            if(CanPlaceBuildable(gridTile)) {
                
                bool isUpgrade = GetPreviousUpgrade(currentBuildable) == gridTile.currentBuildable;

                //-----------BUILDABLE GAMEPLAY LOGIC-----------
                AddBuildable(currentBuildable, currentRotation, gridTile);

                //-----------CONNECTION LOGIC-----------

                //reset all of the highlighted connection meshRenderers changed during the placement process
                foreach(MeshRenderer meshRendererer in connectionMeshRenderers.Keys) {
                    meshRendererer.materials = connectionMeshRenderers[meshRendererer];
                }
                connectionMeshRenderers.Clear();

                //Check for any connections - NEEDS buildable to be placed before checking for connection

                //Dictionary<Connection, List<GridTile>> connections = ConnectionsHandler.instance.GetConnections(gridTile);
                
                //CHECK IF PLACEMENT WAS AN UPGRADE, IF SO THEN 'REFRESH' THE CONNECTION GROUPS, IF NOT THEN TREAT NORMALLY
                if(isUpgrade) {
                    ConnectionsHandler.instance.UpdateConnectionGroupsWithTile(gridTile);
                } else {
                    ConnectionsHandler.instance.TryAddConnections(gridTile);
                }

                //-----------INVENTORY LOGIC-----------
                //Remove item from inventory
                InventoryHandler.instance.RemoveItem(currentBuildable);

                //-----------END-----------
                //Exit placement mode here - currentBuildable set to null here
                Invoke(nameof(ExitPlacementMode), 0.01f);     

            } else {

                //PLAYER CAN'T PLACE BUILDABLE, small animation to communicate
                currentGhost.transform.localScale = Vector3.one;
                DOTween.Kill(currentGhost);
                currentGhost.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f).SetId(currentGhost);

            }        
        }

  
    }
    public void AddBuildable(Buildable buildable, Vector3 rotation, GridTile tile) {
        //Store buildable in list
        allPlacedBuildables.Add(buildable);

        //Update stats
        //StatsHandler.instance.UpdateStats(currentBuildable);

        //Add buildable to tile
        tile.AddBuildable(buildable, rotation);
    }
    public void RemoveBuildable(GridTile tile) {
        //Remove buildable from list
        allPlacedBuildables.Remove(tile.currentBuildable);
        //Update stats
        StatsHandler.instance.UpdateStats();
        //Remove buildable from tile
        tile.RemoveBuildable();
    }

    private void PlacementMode() {
        if(inPlacementMode && currentGhost != null) {

            if(Input.GetKeyDown(KeyCode.R)) {
                Rotate();
            }

            if(Input.GetMouseButtonDown(0)) {
                Place();
            }

        }
    }
    private void Rotate() {
        if(rotationStep < 5) {
            rotationStep++;
        } else {
            rotationStep = 0;
        }
        currentRotation = rotations[rotationStep];
        currentGhost.transform.DORotate(currentRotation, 0.2f);

        //rotate button animation
        rotateButton.transform.DOPunchScale(Vector3.one * 0.1f, 0.1f, 0, 0f);
    }

    private void CreateGhost(Buildable buildable) {
        currentGhost = Instantiate(buildable.tile, new Vector3(0, -5, 0), Quaternion.Euler(rotations[0]));
        foreach(MeshRenderer meshRenderer in currentGhost.GetComponentsInChildren<MeshRenderer>()) {
            meshRenderer.sharedMaterial = highlightMaterial;
        }
        foreach(MonoBehaviour script in currentGhost.GetComponentsInChildren<MonoBehaviour>()) {
            script.enabled = false;
        }
        currentGhost.name = $"PLACEMENT GHOST - {buildable.name}";
    }
    private void DestroyGhost() {
        DOTween.Kill(currentGhost.transform);
        Destroy(currentGhost);
        currentGhost = null;
    }
    public void UpdateGhost(GridTile gridTile) {
        if(gridTile != null) {

            //-----------------------------GHOST-----------------------------

            currentGhost.transform.SetParent(gridTile.holder, false);
            currentGhost.transform.position = gridTile.holder.position;

            //---show tile buildable if it was hidden while UPGRADING---
            if(currentTile != null) {
                if(currentTile.currentBuildableObject != null) {
                    currentTile.currentBuildableObject.SetActive(true);
                }
            }

            //CHECK IF PLAYER CAN PLACE BUILDABLE
            if(CanPlaceBuildable(gridTile)) {

                bool isUpgrade = gridTile.currentBuildable != null && GetPreviousUpgrade(currentBuildable) == gridTile.currentBuildable;

                //change color of highlight based on whether placement is an upgrade or normal placement
                Material targetMaterial = isUpgrade ? positiveHighlightMaterial : highlightMaterial;

                //hide the gridTile's current tile if we are upgrading (prevents overlapping)
                if(isUpgrade) {
                    if(gridTile.currentBuildableObject != null) {
                        gridTile.currentBuildableObject.SetActive(false);
                    }
                }

                //highlight ghost
                foreach(MeshRenderer meshRenderer in currentGhost.GetComponentsInChildren<MeshRenderer>()) {
                    meshRenderer.sharedMaterial = targetMaterial;
                }

                //also communicate in place button
                placeButton.DOFade(1f, 0.2f);
                placeButtonFocusHolder.gameObject.SetActive(true);
                
            } else {
                foreach(MeshRenderer meshRenderer in currentGhost.GetComponentsInChildren<MeshRenderer>()) {
                    meshRenderer.sharedMaterial = negativeHighlightMaterial;
                }

                //also communicate in place button
                placeButton.DOFade(0.2f, 0.2f);
                placeButtonFocusHolder.gameObject.SetActive(false);
            }

            currentTile = gridTile;

            //-----------------------------CONNECTIONS-----------------------------

            //reset all of the highlighted connection meshRenderers
            foreach(MeshRenderer meshRendererer in connectionMeshRenderers.Keys) {
                meshRendererer.materials = connectionMeshRenderers[meshRendererer];
            }
            //connectionMeshRenderers.Clear();

            //highlight all of the tiles that this tile can connect to, make sure tile isn't occupied before showing the effect
            bool showConnections = gridTile.currentBuildable == null || GetPreviousUpgrade(currentBuildable) == gridTile.currentBuildable;
            if(showConnections) {
                //get all the tiles of the possible connections the tile can have
                Dictionary<Connection, List<GridTile>> connectionTiles = ConnectionsHandler.instance.GetConnectionTilesFromTileBuildable(gridTile, currentBuildable);
                //loop through all connection tiles
                foreach(var connectionTilePair in connectionTiles) {
                    
                    Connection connection = connectionTilePair.Key;
                    List<GridTile> tiles = connectionTilePair.Value;

                    //highlight the meshRenderers of each tile's buildable
                    foreach(GridTile tile in tiles) {
                        foreach(MeshRenderer meshRenderer in tile.currentBuildableObject.transform.GetComponentsInChildren<MeshRenderer>()) {
                            //store the original materials in list, so they can be reset later
                            if(!connectionMeshRenderers.ContainsKey(meshRenderer)) {
                                connectionMeshRenderers.Add(meshRenderer, meshRenderer.materials);
                            }

                            //change material of meshRenderer
                            List<Material> materialsList = new List<Material>(meshRenderer.materials) {
                                ConnectionsHandler.instance.GetConnectionPositive(connection) ? overlayHighlightMaterial : overlayNegativeHighlightMaterial
                            };
                            meshRenderer.materials = materialsList.ToArray();
                        }
                    }

                }
            }


        } else {
            //HIDE GHOST
            currentGhost.transform.position = new Vector3(0, -5, 0);

            //---show tile buildable if it was hidden while UPGRADING---
            if(currentTile != null) {
                if(currentTile.currentBuildableObject != null) {
                    currentTile.currentBuildableObject?.SetActive(true);
                }
            }

            //reset all of the highlighted connection meshRenderers
            foreach(MeshRenderer meshRendererer in connectionMeshRenderers.Keys) {
                meshRendererer.materials = connectionMeshRenderers[meshRendererer];
            }
        }

    }
    //returns the count of a certain buildable
    public int GetPlacedBuildableCount(string name) {
        return allPlacedBuildables.Where(x => x.name == name).Count();
    }
    //returns whether a buildable can be placed on a tile
    private bool CanPlaceBuildable(GridTile tile) {
        Buildable tileBuildable = tile.currentBuildable;
        Buildable previousCurrentBuildableUpgrade = GetPreviousUpgrade(currentBuildable);

        bool unoccupiedTileAndNoUpgrade = tile.currentBuildable == null && previousCurrentBuildableUpgrade == null;
        bool occupiedTileAndUpgrade = tile.currentBuildable != null && previousCurrentBuildableUpgrade == tileBuildable;
        
        return unoccupiedTileAndNoUpgrade || occupiedTileAndUpgrade;
    }
    //returns a buildables previous upgrade (ex. big house -> small house, rock -> null)
    private Buildable GetPreviousUpgrade(Buildable buildable) {
        return Resources.LoadAll<Buildable>("Buildables").FirstOrDefault(x => x.upgrade != null && x.upgrade.name == buildable.name);
    }

    //----------------------------UI----------------------------

    public void CancelButtonClick() {
        ExitPlacementMode();
        cancelButton.localScale = Vector3.one;
    }
    public void CancelButtonEnter() {
        GameHandler.instance.currentFocusedAnimatable = this;
        cancelButton.DOScale(1.2f, 0.2f);
    }
    public void CancelButtonExit() {
        cancelButton.DOScale(1f, 0.2f);
    }

    public void RotateButtonClick() {
        Rotate();
        rotateButton.localScale = Vector3.one;
    }
    public void RotateButtonEnter() {
        GameHandler.instance.currentFocusedAnimatable = this;
        rotateButton.DOScale(1.2f, 0.2f);
    }
    public void RotateButtonExit() {
        rotateButton.DOScale(1f, 0.2f);
    }

    public void AnimatableExit() {
        CancelButtonExit();
        RotateButtonExit();
    }

    private IEnumerator StartPlaceButtonFocusAnimation() {
        placeButtonFocusHolder.GetChild(0).GetComponent<RectTransform>().DOSizeDelta(Vector2.one * 80, 2f).SetLoops(-1, LoopType.Restart);
        placeButtonFocusHolder.GetChild(0).GetComponent<Image>().DOFade(0, 2f).SetLoops(-1, LoopType.Restart);
        yield return new WaitForSeconds(1f);
        placeButtonFocusHolder.GetChild(1).GetComponent<RectTransform>().DOSizeDelta(Vector2.one * 80, 2f).SetLoops(-1, LoopType.Restart);
        placeButtonFocusHolder.GetChild(1).GetComponent<Image>().DOFade(0, 2f).SetLoops(-1, LoopType.Restart);
        yield return new WaitForSeconds(1f);
        placeButtonFocusHolder.GetChild(2).GetComponent<RectTransform>().DOSizeDelta(Vector2.one * 80, 2f).SetLoops(-1, LoopType.Restart);
        placeButtonFocusHolder.GetChild(2).GetComponent<Image>().DOFade(0, 2f).SetLoops(-1, LoopType.Restart);
    }
}
