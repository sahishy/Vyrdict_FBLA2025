using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class MenuButtonAnimations : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(1.1f, 0.2f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(1f, 0.2f);
    }

    public void OnPointerClick(PointerEventData eventData) {
        transform.DOScale(1f, 0.2f);
    }
}
