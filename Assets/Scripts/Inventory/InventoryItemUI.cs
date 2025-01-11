using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour, Animatable
{
    public Buildable buildable;
    
    [Header("References")]
    public TMP_Text itemName;
    public TMP_Text itemAmount;
    public Image itemIcon;

    public void Initialize(Buildable item) {
        buildable = item;

        itemName.text = buildable.name;
        itemAmount.text = "1";
        itemIcon.sprite = buildable.icon;
    }

    public void UpdateAmount(int amount) {
        itemAmount.text = amount.ToString();
        itemAmount.transform.parent.gameObject.SetActive(amount > 0);
    }

    public void ButtonClick() {
        PlacementHandler.instance.EnterPlacementMode(buildable);

        transform.DOScale(Vector3.one, 0.2f);
    }

    public void ButtonEnter() {
        GameHandler.instance.currentFocusedAnimatable = this;

        transform.DOScale(1.2f, 0.2f);
        itemName.transform.parent.DOScale(0.8f, 0.2f);
    }
    public void ButtonExit() {
        transform.DOScale(1f, 0.2f);
        itemName.transform.parent.DOScale(0f, 0.2f);
    }
    public void AnimatableExit() {
        ButtonExit();
    }

}
