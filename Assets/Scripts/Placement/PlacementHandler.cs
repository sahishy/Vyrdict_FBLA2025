using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlacementHandler : MonoBehaviour, Animatable
{
    public static PlacementHandler instance;

    [Header("Placement")]
    public List<Buildable> allPlacedBuildables = new List<Buildable>();
    [HideInInspector] public bool inPlacementMode = false;
    private Buildable currentBuildable;
    [HideInInspector] public GameObject currentGhost;
    
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
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material negativeHighlightMaterial;

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

        GameHandler.instance.currentFocusedAnimatable?.AnimatableExit();

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
            if(gridTile.currentBuildable == null) {
                
                //-----------BUILDABLE GAMEPLAY LOGIC-----------
                AddBuildable(currentBuildable, gridTile);

                //-----------CONNECTION LOGIC-----------
                //Check for any connections - NEEDS buildable to be placed before checking for connection

                //Dictionary<Connection, List<GridTile>> connections = ConnectionsHandler.instance.GetConnections(gridTile);
                ConnectionsHandler.instance.TryAddConnections(gridTile);

                //-----------INVENTORY LOGIC-----------
                //Remove item from inventory
                InventoryHandler.instance.RemoveItem(currentBuildable);

                //-----------END-----------
                //Exit placement mode here - currentBuildable set to null here
                Invoke(nameof(ExitPlacementMode), 0.01f);     

            }            
        }

  
    }
    public void AddBuildable(Buildable buildable, GridTile tile) {
        //Store buildable in list
        allPlacedBuildables.Add(currentBuildable);
        //Update stats
        StatsHandler.instance.UpdateStats(currentBuildable);
        //Add buildable to tile
        tile.AddBuildable(currentBuildable, currentRotation);
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

            currentGhost.transform.SetParent(gridTile.holder, false);
            currentGhost.transform.position = gridTile.holder.position;

            //CHECK IF TILE IS OCCUPIED
            if(gridTile.currentBuildable == null) {
                foreach(MeshRenderer meshRenderer in currentGhost.GetComponentsInChildren<MeshRenderer>()) {
                    meshRenderer.sharedMaterial = highlightMaterial;
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

        } else {
            currentGhost.transform.position = new Vector3(0, -5, 0);
        }

    }
    public int GetPlacedBuildableCount(string name) {
        return allPlacedBuildables.Where(x => x.name == name).Count();
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
