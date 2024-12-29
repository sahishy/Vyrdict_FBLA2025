using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DecisionHandler : MonoBehaviour
{
    public static DecisionHandler instance;
    public bool inDecisionMode;

    public Decision testDecision;

    [Header("References")]
    [SerializeField] private GameObject gameplayScreen;
    [SerializeField] private GameObject decisionScreen;
    [SerializeField] private GameObject decisionChoiceHolder;
    [SerializeField] private GameObject decisionChoicePrefab;


    void Awake() {
        instance = this;
    }

    void Start() {
        Invoke(nameof(MakeDecision), 5f);
    }

    private void MakeDecision() {
        inDecisionMode = true;

        decisionScreen.SetActive(true);
        decisionScreen.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
        gameplayScreen.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() => {
            gameplayScreen.SetActive(false);
        });

        if(PlacementHandler.instance.inEditMode) {
            PlacementHandler.instance.ExitEditMode();
        }

        DecisionChoice choice = Instantiate(decisionChoicePrefab, decisionChoiceHolder.transform).GetComponent<DecisionChoice>();
        choice.Initialize(testDecision);
    }
    public void SelectChoice(Decision decision) {
        decisionScreen.GetComponent<CanvasGroup>().DOFade(0, 0.5f).OnComplete(() => {
           decisionScreen.SetActive(false); 
        });
        gameplayScreen.SetActive(true);
        gameplayScreen.GetComponent<CanvasGroup>().DOFade(1, 0.5f);

        Debug.Log($"Selected: {decision.amount} x {decision.buildable.name}");

        InventoryHandler.instance.AddItem(decision.buildable, decision.amount);

        inDecisionMode = false;
    }
}
