using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GridTile : MonoBehaviour, Interactable
{
    [Header("Tile")]
    public Buildable currentBuildable = null;
    private GameObject currentBuildableObject = null;

    [Header("References")]
    public Transform holder;
    public Transform display;
    private bool displayActive = false;
    private GameObject tileModel;
    public List<GameObject> tileModels = new List<GameObject>();

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
 
    public void Interact()
    {
        //Debug.Log(currentBuildable != null ? currentBuildable.name : "Empty");
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
