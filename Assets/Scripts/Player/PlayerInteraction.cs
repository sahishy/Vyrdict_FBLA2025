using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction instance;

    Interactable focusedInteractable = null;
    public Material highlightMaterial;

    void Awake() {
        instance = this;
    }

    void Update()
    {
        Interaction();
    }

    private void Interaction() {

        //INTERACTABLE DETECTION

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity) && !EventSystem.current.IsPointerOverGameObject()) {

            Interactable hitInteractable = hit.collider.gameObject.GetComponent<Interactable>();
            if(hitInteractable != null) {
                
                UpdateFocusedInteractable(hitInteractable);


            } else {
                UpdateFocusedInteractable(null);
            }

        } else {
            UpdateFocusedInteractable(null);
        }

        //INTERACTABLE INTERACTION

        if(focusedInteractable != null) {
            if(Input.GetMouseButtonDown(0)) {
                focusedInteractable.Interact();
            }
        }

    }

    private void UpdateFocusedInteractable(Interactable interactable) {
        if(interactable != focusedInteractable) {

            //HIGHLIGHT THE INTERACTABLE
            HighlightFocusedInteractable(focusedInteractable?.GetGameObject(), interactable?.GetGameObject());
            
            focusedInteractable = interactable;

            //PLACEMENT GHOST
            if(PlacementHandler.instance.currentGhost != null) {
                PlacementHandler.instance.UpdateGhost(GetFocusedGridTile());
            }

        } else {
            return;
        }
    }

    public Interactable GetFocusedInteractable() {
        return focusedInteractable;
    }
    public GridTile GetFocusedGridTile() {
        return focusedInteractable?.GetGameObject().GetComponent<GridTile>();
    }

    private Dictionary<GameObject, Material[]> originalMaterials = new Dictionary<GameObject, Material[]>();
    public void HighlightFocusedInteractable(GameObject previousInteractable, GameObject newInteractable) {

        //----------SCALE----------

        if(previousInteractable != null) {
            previousInteractable.transform.DOMoveY(1.2f, 0.2f);
        }
        if(newInteractable != null) {
            newInteractable.transform.DOMoveY(1.5f, 0.2f);
        }

        //----------MATERIAL----------

        //gets previous focused interactable and restores original materials
        if (previousInteractable != null) {
            Transform previousHolder = previousInteractable.transform.Find("holder");
            if (previousHolder != null) {
                foreach (Transform child in previousHolder) {
                    MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
                    if (meshRenderer && originalMaterials.ContainsKey(child.gameObject)) {
                        //restore original materials
                        meshRenderer.materials = originalMaterials[child.gameObject];
                        originalMaterials.Remove(child.gameObject);
                    }
                }
            }
        }
        
        //gets new focused interactable and adds highlight material, 
        //stores original materials if not already stored
        if (newInteractable != null) {
            Transform newHolder = newInteractable.transform.Find("holder");
            if (newHolder != null) {
                foreach (Transform child in newHolder) {
                    MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
                    if (meshRenderer) {
                        //store original materials if not already stored
                        if (!originalMaterials.ContainsKey(child.gameObject)) {
                            originalMaterials[child.gameObject] = meshRenderer.materials;
                        }

                        //add highlight material
                        List<Material> materialsList = new List<Material>(meshRenderer.materials);
                        materialsList.Add(highlightMaterial);
                        meshRenderer.materials = materialsList.ToArray();
                    }
                }
            }
        }
    }
}
