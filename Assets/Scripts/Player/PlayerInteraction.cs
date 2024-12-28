using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    Interactable focusedInteractable = null;
    public Material highlightMaterial;


    void Update()
    {
        Interaction();
    }

    private void Interaction() {

        //INTERACTABLE DETECTION

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) {

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

            HighlightFocusedInteractable(focusedInteractable?.GetGameObject(), interactable?.GetGameObject());
            focusedInteractable = interactable;

        } else {
            return;
        }
    }
    private Dictionary<GameObject, Material[]> originalMaterials = new Dictionary<GameObject, Material[]>();
    private void HighlightFocusedInteractable(GameObject previousInteractable, GameObject newInteractable) {

        //----------SCALE----------

        if(previousInteractable != null) {
            previousInteractable.transform.DOMoveY(1.2f, 0.2f);
        }
        if(newInteractable != null) {
            newInteractable.transform.DOMoveY(1.5f, 0.2f);
        }

        //----------MATERIAL----------
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
