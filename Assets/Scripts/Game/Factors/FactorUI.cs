using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FactorUI : MonoBehaviour, Animatable
{
    private Factor factor;

    private float factorLength;
    private float startingTime;
    private bool destroying;

    //Requirements
    [HideInInspector] public int startingBuildableCount;
    
    [Header("References")]
    [SerializeField] private Image factorBar;
    [SerializeField] private Transform factorInfo;

    [SerializeField] private Image factorIcon;
    [SerializeField] private TMP_Text factorName; 
    [SerializeField] private TMP_Text factorDescription;
    [SerializeField] private TMP_Text factorType;

    [SerializeField] private GameObject factorStatEffectHolder;
    [SerializeField] private Image factorStatEffectIcon;
    [SerializeField] private TMP_Text factorStatEffectValue;

    [SerializeField] private GameObject factorRewardHolder;
    [SerializeField] private TMP_Text factorReward;

    public void Initialize(Factor _factor) {
        factor = _factor;

        factorLength = factor.length != -1f ? factor.length * 20f : Mathf.Infinity;
        startingTime = GameHandler.instance.timer;

        factorIcon.sprite = factor.icon;
        factorName.text = factor.name;
        if(factor.factorType == FactorType.Event) {
            factorDescription.text = factor.description;
        } else {
            factorDescription.text = GetDescription();
        }
        factorType.text = factor.factorType.ToString();

        if(factor.factorType == FactorType.Event || factor.factorType == FactorType.Demand) {
            factorStatEffectHolder.SetActive(true);
            factorStatEffectIcon.sprite = StatsHandler.instance.GetStatSprite(factor.statEffect);
            factorStatEffectIcon.color = StatsHandler.instance.GetSimpleStatusColor(factor.statEffectValue);
            factorStatEffectValue.text = $"{StatsHandler.instance.ConvertToEffectValue(factor.statEffectValue)} / day";
            factorStatEffectValue.color = StatsHandler.instance.GetSimpleStatusColor(factor.statEffectValue);
        } else if(factor.factorType == FactorType.Quest) {
            factorRewardHolder.SetActive(true);
            factorReward.text = factor.reward == FactorReward.Buildable ? $"1x {factor.buildableReward.name}" : $"{StatsHandler.instance.ConvertToEffectValue(factor.statRewardValue)} {factor.statReward}";
        }

        //Requirements
        if(factor.factorType == FactorType.Demand || factor.factorType == FactorType.Quest) {
            if(factor.requirement == FactorRequirement.Buildable) {
                startingBuildableCount = PlacementHandler.instance.GetPlacedBuildableCount(factor.requiredBuildable.name);
            }
        }

        transform.DOScale(1f, 0.5f).SetEase(Ease.InBack);
    }
    
    private void Update() {

        if(factor != null) {

            //End factor after max time
            factorBar.fillAmount = (factorLength - (GameHandler.instance.timer - startingTime)) / factorLength;

            if(factorBar.fillAmount == 0 && !destroying) {
                DestroyFactor();
            }
            
        }

        //factorBar.color = new Color32(255, 255, 255, (byte)(GameHandler.instance.timeFrozen || GameHandler.instance.timeScale == 0 ? 128 : 255));
    }

    private string GetDescription() {
        string description = factor.description;

        if(factor.factorType != FactorType.Event) {

            int finalDay = GameHandler.instance.currentDay + factor.length;
            string finalDate = finalDay % 7 == 0 ? $"Week {finalDay / 7}" : $"Day {finalDay}";

            if(factor.requirement == FactorRequirement.Stat) {

                description += $" Reach {factor.requiredStatValue} {factor.requiredStat} by {finalDate}.";

            } else if(factor.requirement == FactorRequirement.Buildable) {

                string buildableCount = factor.requiredBuildableCount == 1 ? $"a {factor.requiredBuildable.name}"
                : $"{factor.requiredBuildableCount} {factor.requiredBuildable.name}s";

                description += $" Build {buildableCount} by {finalDate}.";

            }

        }

        return description;
    }

    public void DestroyFactor() {
        destroying = true;

        FactorsHandler.instance.RemoveFactor(this);
        
        if((object)GameHandler.instance.currentFocusedAnimatable == this) {
            GameHandler.instance.currentFocusedAnimatable = null;
        }
        ButtonExit();
        transform.DOScale(0f, 0.5f).SetEase(Ease.InBack).OnComplete(() => {
            Destroy(gameObject, 0.1f);
        });
    }

    public void ButtonEnter() {
        if(!destroying) {
            GameHandler.instance.currentFocusedAnimatable = this;

            transform.DOScale(1.2f, 0.2f);
            factorInfo.DOScale(0.8f, 0.2f);            
        }

    }
    public void ButtonExit() {
        if(!destroying) {
            transform.DOScale(1f, 0.2f);
            factorInfo.DOScale(0f, 0.2f);            
        }

    }
    public void AnimatableExit() {
        ButtonExit();
    }

}
