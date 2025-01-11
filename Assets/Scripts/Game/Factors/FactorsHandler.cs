using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FactorsHandler : MonoBehaviour
{
    public static FactorsHandler instance;

    public Dictionary<Factor, FactorUI> factors = new Dictionary<Factor, FactorUI>();

    [Header("References")]
    [SerializeField] private GameObject factorPrefab;
    [SerializeField] private Transform factorHolder;

    void Awake() {
        instance = this;
    }

    public void AddFactor(Factor factor) {
        FactorUI newFactor = Instantiate(factorPrefab, factorHolder).GetComponent<FactorUI>();
        newFactor.Initialize(factor);

        factors.Add(factor, newFactor);
    }

    public void RemoveFactor(FactorUI factorUI) {
        factors.Remove(factors.FirstOrDefault(x => x.Value == factorUI).Key);
    }

}
