using DG.Tweening;
using UnityEngine;

public class GameplayUI : MonoBehaviour
{
    [Header("Stats References")]
    public Transform statsInformationPanel;

    public void ShowStatsInformation() {
        statsInformationPanel.DOScaleY(1, 0.2f).SetEase(Ease.OutBack);
    }
    public void HideStatsInformation() {
        statsInformationPanel.DOScaleY(0, 0.2f).SetEase(Ease.InBack);
    }
}
