using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileDisplayHandler : MonoBehaviour, Animatable
{
    public static TileDisplayHandler instance;
    private GridTile displayedTile = null;

    [SerializeField] private Vector2 targetPos;

    [Header("References")]
    [SerializeField] private GameObject display;
    [SerializeField] private RectTransform displayPanel;
    [SerializeField] private Image buildableIcon;
    [SerializeField] private TMP_Text buildableName;
    [SerializeField] private TMP_Text buildableDescription;
    [SerializeField] private TMP_Text buildableStatus;
    private bool panelToggling = false; //used to prevent hover animation from happening while the UI is already opening/closing

    void Awake() {
        instance = this;
    }

    public void ShowDisplay(GridTile tile) {
        //prevents unwanted display right after placing something down 
        if(PlacementHandler.instance.inPlacementMode) {
            return;
        }

        //toggles display off if the same tile is clicked again
        if(displayedTile == tile) {
            HideDisplay();
            return;
        }

        //end all current UI animations
        DOTween.Kill(displayPanel);
        if(panelToggling) {
            displayPanel.anchoredPosition = targetPos;
            panelToggling = false;            
        }
        displayPanel.transform.DOPunchScale(Vector3.one * 0.02f, 0.1f).SetId(displayPanel);

        //hide the old tile outline
        if(displayedTile != null) {
            displayedTile.ToggleFocusDisplay(false);
        }

        //store tile
        displayedTile = tile;

        //--GET STATUS--
        List<string> statusTexts = new List<string>();
        //connection
        if(tile.currentBuildable.requiredConnection != null) {
            if(ConnectionsHandler.instance.RequiredConnectionMet(tile)) {
                statusTexts.Add(tile.currentBuildable.positiveRequiredConnectionStatusText);
            } else {
                statusTexts.Add(tile.currentBuildable.negativeRequiredConnectionStatusText);
            }
        }
        //customers -- error here
        if(CommunitiesHandler.instance.GetCommunity(tile) != null) {
            if(CommunitiesHandler.instance.GetCommunity(tile).population >= CommunitiesHandler.instance.GetCommunity(tile).requiredPopulation) {
                statusTexts.Add(tile.currentBuildable.positiveRequiredCustomersStatusText);
            } else {
                statusTexts.Add(tile.currentBuildable.negativeRequiredCustomersStatusText);
            }            
        }

        //pollution
        if(StatsHandler.instance.GetPollutionOutput(tile.currentBuildable) <= 1) {
            statusTexts.Add(tile.currentBuildable.positivePollutionInfluenceStatusText);
        } else {
            statusTexts.Add(tile.currentBuildable.negativePollutionInfluenceStatusText);
        }
        //default status if no status texts
        if(statusTexts.Count == 0) {
            statusTexts.Add("Materials are being used as the building waits for an upgrade.");
        }
        //combine
        string status = string.Join(" ", statusTexts.ToArray());

        //---DISPLAY UI---
        buildableIcon.sprite = tile.currentBuildable.icon;
        buildableName.text = tile.currentBuildable.name;
        buildableDescription.text = tile.currentBuildable.description;
        buildableStatus.text = status;
        // Debug.Log($"Display Shown: {tile.currentBuildable.name}");

        displayedTile.ToggleFocusDisplay(true);

        //show connections
        foreach(ConnectionGroup group in ConnectionsHandler.instance.GetConnectionGroups(tile)) {
            Transform holder = ConnectionsHandler.instance.connectionLineHolder.Find(group.id);
            holder.gameObject.SetActive(true);
        }

        //opening animation
        panelToggling = true;
        if(!display.activeSelf) {
            display.SetActive(true);
            displayPanel.DOAnchorPos(targetPos, 0.2f).SetEase(Ease.OutSine).OnComplete(() => {
                panelToggling = false;
            }).SetId(displayPanel);
        }

        StartCoroutine(RefreshContentSizeFitter());
    }
    public void HideDisplay() {
        if(displayedTile == null) {
            return;
        }
        
        // Debug.Log("Display Hidden");

        //hide the old tile outline
        displayedTile.ToggleFocusDisplay(false);

        //hide connections
        foreach(ConnectionGroup group in ConnectionsHandler.instance.GetConnectionGroups(displayedTile)) {
            Transform holder = ConnectionsHandler.instance.connectionLineHolder.Find(group.id);
            holder.gameObject.SetActive(false);
        }

        //reset tile
        displayedTile = null;

        //closing animation
        panelToggling = true;
        displayPanel.DOAnchorPos(new Vector2(0, displayPanel.sizeDelta.y * -1f), 0.2f).SetEase(Ease.InSine).OnComplete(() => {
            display.SetActive(false);
            panelToggling = false;
        });
    }

    //----------------------------UI----------------------------

    private IEnumerator RefreshContentSizeFitter() {
        displayPanel.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        yield return null;
        displayPanel.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.MinSize;
    }

    public void PanelEnter() {
        if(panelToggling) {
            return;
        }

        panelToggling = true;
        displayPanel.DOAnchorPos(targetPos - new Vector2(0, 10), 0.2f).OnComplete(() => {
            panelToggling = false;
        });
        GameHandler.instance.currentFocusedAnimatable = this;
    }

    public void PanelExit() {
        if(panelToggling) {
            return;
        }

        panelToggling = true;
        displayPanel.DOAnchorPos(targetPos, 0.2f).OnComplete(() => {
            panelToggling = false;
        });
    }

    public void PanelClick() {
        HideDisplay();
    }

    public void AnimatableExit()
    {
        PanelExit();
    }
}
