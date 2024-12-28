using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class GridTile : MonoBehaviour, Interactable
{
    [Header("Tile")]
    public Buildable currentBuildable = null;
    private GameObject currentBuildableObject = null;

    [Header("References")]
    public Transform holder;
    GameObject tileModel;
    public List<GameObject> tileModels = new List<GameObject>();

    public void Initialize(Buildable buildable = null) {
        //CREATING BASE TILE
        GameObject model = tileModels[0];

        if (model != null)
        {
            tileModel = Instantiate(model, holder);
            tileModel.transform.localPosition = Vector3.zero;
        }

        //ADDING OBJECT
        if(buildable != null) {
            AddBuildable(buildable);
        }
    }

    public void AddBuildable(Buildable buildable) {
        currentBuildable = buildable;

        currentBuildableObject = Instantiate(currentBuildable.tile, holder);
        currentBuildableObject.transform.localScale = Vector3.zero;
        currentBuildableObject.transform.localPosition = Vector3.zero;

        currentBuildableObject.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce);
    }
    public void RemoveBuildable() {
        currentBuildable = null;

        Destroy(currentBuildableObject);
    }

    public void Interact()
    {
        Debug.Log(currentBuildable != null ? currentBuildable.name : "Empty");
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
