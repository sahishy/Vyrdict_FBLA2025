using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacementHandler : MonoBehaviour
{
    public static PlacementHandler instance;

    public bool inEditMode = false;

    [Header("Placement")]
    bool inPlacementMode = false;
    private Buildable currentBuildable;
    public GameObject currentGhost;
    
    public Vector3 currentRotation = new Vector3(0, 210, 0);
    private int rotationStep = 0;
    public Vector3[] rotations = new Vector3[] {
        new Vector3(0, 210, 0),
        new Vector3(0, 270, 0),
        new Vector3(0, 330, 0),
        new Vector3(0, 30, 0),
        new Vector3(0, 90, 0),
        new Vector3(0, 150, 0),
    };

    [Header("References")]
    [SerializeField] private RectTransform editPanel;
    [SerializeField] private GameObject placementPanel;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material negativeHighlightMaterial;

    void Awake() {
        instance = this;
    }

    void Update() {
        PlacementMode();

        if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && !inPlacementMode) {
            if(inEditMode) {
                ExitEditMode();
            } else {
                EnterEditMode();
            }
        }
    }

    public void EnterEditMode() {
        inEditMode = true;
        editPanel.DOAnchorPos(new Vector2(0, 25), 0.5f);
        Camera.main.DOFieldOfView(40, 0.5f);
    }
    public void ExitEditMode() {
        inEditMode = false;
        editPanel.DOAnchorPos(new Vector2(0, -125), 0.5f);
        Camera.main.DOFieldOfView(60, 0.5f);

        //if forced out of edit mode (not getting chance to exit placement mode)
        if(inPlacementMode) {
            ExitPlacementMode();
        }
    }

    public void EnterPlacementMode(Buildable buildable) {
        inPlacementMode = true;
        currentBuildable = buildable;

        TogglePlacementUI(true);
        CreateGhost(buildable);
    }
    public void ExitPlacementMode() {
        inPlacementMode = false;
        currentBuildable = null;

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

                //ADD BUILDABLE TO FOCUSED TILE
                gridTile.AddBuildable(currentBuildable, currentRotation);
                //REMOVE ITEM FROM INVENTORY
                InventoryHandler.instance.RemoveItem(currentBuildable);
                //EXIT PLACEMENT MODE
                Invoke(nameof(ExitPlacementMode), 0.01f);     

            }            
        }

  
    }
    private void PlacementMode() {
        if(inPlacementMode && inEditMode && currentGhost != null) {

            if(Input.GetKeyDown(KeyCode.R)) {

                if(rotationStep < 5) {
                    rotationStep++;
                } else {
                    rotationStep = 0;
                }
                currentRotation = rotations[rotationStep];
                currentGhost.transform.DORotate(currentRotation, 0.2f);

            }

            if(Input.GetMouseButtonDown(0)) {
                Place();
            }

        }
    }

    private void CreateGhost(Buildable buildable) {
        currentGhost = Instantiate(buildable.tile, new Vector3(0, -5, 0), Quaternion.Euler(rotations[0]));
        currentGhost.GetComponent<MeshRenderer>().sharedMaterial = highlightMaterial;
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
                currentGhost.GetComponent<MeshRenderer>().sharedMaterial = highlightMaterial;
            } else {
                currentGhost.GetComponent<MeshRenderer>().sharedMaterial = negativeHighlightMaterial;
            }

        } else {
            currentGhost.transform.position = new Vector3(0, -5, 0);
        }

    }
}
