using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "NewCard", menuName = "Game/Card")]
public class Card : ScriptableObject
{
    [SerializeField] private Sprite _icon;
    [SerializeField] private string _description;
    [SerializeField] private CardRarity _cardRarity;
    [SerializeField] private CardType _cardType;
    //buildable card
    [SerializeField] private Buildable _buildable;
    [SerializeField] private int _buildableSuppliesCost;
    [SerializeField] private int _buildableGoldCost;
    //upgrade card
    [SerializeField] private Buildable _upgrade;
    [SerializeField] private int _upgradeSuppliesCost;
    [SerializeField] private int _upgradeGoldCost;
    //convert card
    [SerializeField] private Stat _statFrom;
    [SerializeField] private int _statFromValue;
    [SerializeField] private Stat _statTo;
    [SerializeField] private int _statToValue;
    //boost card
    [SerializeField] private bool _instantBoost;
    [SerializeField] private Stat _boostedStat;
    [SerializeField] private int _boostValue;
    [SerializeField] private int _boostLength;
    //ability card

    //factor card
    [SerializeField] private Factor _factor;
    //strike card
    [SerializeField] private Stat _strikedStat;
    [SerializeField] private int _strikeValue;
    //crisis card


    public Sprite icon { get => _icon; set => _icon = value; }
    public string description { get => _description; set => _description = value; }
    public CardRarity cardRarity { get => _cardRarity; set => _cardRarity = value; }
    public CardType cardType { get => _cardType; set => _cardType = value; }
    //buildable card
    public Buildable buildable { get => _buildable; set => _buildable = value; }
    public int buildableSuppliesCost { get => _buildableSuppliesCost; set => _buildableSuppliesCost = value; }
    public int buildableGoldCost { get => _buildableGoldCost; set => _buildableGoldCost = value; }
    //upgrade card
    public Buildable upgrade { get => _upgrade; set => _upgrade = value; }
    public int upgradeSuppliesCost { get => _upgradeSuppliesCost; set => _upgradeSuppliesCost = value; }
    public int upgradeGoldCost { get => _upgradeGoldCost; set => _upgradeGoldCost = value; }
    //convert card
    public Stat statFrom { get => _statFrom; set => _statFrom = value; }
    public int statFromValue { get => _statFromValue; set => _statFromValue = value; }
    public Stat statTo { get => _statTo; set => _statTo = value; }
    public int statToValue { get => _statToValue; set => _statToValue = value; }
    //boost card
    public bool instantBoost { get => _instantBoost; set => _instantBoost = value; }
    public Stat boostedStat { get => _boostedStat; set => _boostedStat = value; }
    public int boostValue { get => _boostValue; set => _boostValue = value; }
    public int boostLength { get => _boostLength; set => _boostLength = value; }
    //ability card

    //factor card
    public Factor factor { get => _factor; set => _factor = value; }
    //strike card
    public Stat strikedStat { get => _strikedStat; set => _strikedStat = value; }
    public int strikeValue { get => _strikeValue; set => _strikeValue = value; }
    //crisis card

}

#if UNITY_EDITOR
[CustomEditor(typeof(Card))]
public class CardEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var script = (Card)target;

        // UI START
        GUILayout.Space(16);

        // HEADER SECTION
        GUILayout.Label(script.name, new GUIStyle(EditorStyles.label) { fontSize = 24, fontStyle = FontStyle.Bold } );

        // DESCRIPTION SECTION
        DrawHorizontalRule();
        DrawHeader("Description");

        script.description = EditorGUILayout.TextArea(script.description, GUILayout.Height(64));

        // CARD SECTION
        DrawHorizontalRule();
        DrawHeader("Card");

        script.cardRarity = (CardRarity)EditorGUILayout.EnumPopup("Card Rarity", script.cardRarity);
        script.cardType = (CardType)EditorGUILayout.EnumPopup("Card Type", script.cardType);
        
        // CARD -> BUILDABLE

        if(script.cardType == CardType.Buildable) {
            script.buildable = (Buildable)EditorGUILayout.ObjectField("Buildable", script.buildable, typeof(Buildable), false);
            script.buildableSuppliesCost = EditorGUILayout.IntField("Supplies Cost", script.buildableSuppliesCost);
            script.buildableGoldCost = EditorGUILayout.IntField("Gold Cost", script.buildableGoldCost);
        }
        
        // CARD -> UPGRADE

        if(script.cardType == CardType.Upgrade) {
            script.upgrade = (Buildable)EditorGUILayout.ObjectField("Upgrade", script.upgrade, typeof(Buildable), false);
            script.upgradeSuppliesCost = EditorGUILayout.IntField("Supplies Cost", script.upgradeSuppliesCost);
            script.upgradeGoldCost = EditorGUILayout.IntField("Gold Cost", script.upgradeGoldCost);
        }

        // CARD -> CONVERT

        if(script.cardType == CardType.Convert) {
            script.statFrom = (Stat)EditorGUILayout.EnumPopup("Stat From", script.statFrom);
            script.statFromValue = EditorGUILayout.IntField("Stat From Value", script.statFromValue);
            script.statTo = (Stat)EditorGUILayout.EnumPopup("Stat To", script.statTo);
            script.statToValue = EditorGUILayout.IntField("Stat To Value", script.statToValue);
        }

        // CARD -> BOOST

        if(script.cardType == CardType.Boost) {
            script.instantBoost = EditorGUILayout.Toggle("Instant Boost", script.instantBoost);
            script.boostedStat = script.statTo = (Stat)EditorGUILayout.EnumPopup("Boosted Stat", script.boostedStat);
            script.boostValue = EditorGUILayout.IntField("Boost Value", script.boostValue);
            if(!script.instantBoost) {
                script.boostLength = EditorGUILayout.IntField("Boost Length", script.boostLength);
            }
        }

        // CARD -> ABILITY

        if(script.cardType == CardType.Ability) {
            
        }

        // CARD -> FACTOR

        if(script.cardType == CardType.Factor) {
            script.factor = (Factor)EditorGUILayout.ObjectField("Factor", script.factor, typeof(Factor), false);
        }

        // CARD -> STRIKE

        if(script.cardType == CardType.Strike) {
            script.strikedStat = (Stat)EditorGUILayout.EnumPopup("Striked Stat", script.strikedStat);
            script.strikeValue = EditorGUILayout.IntField("Strike Value", script.strikeValue);
        }

        // CARD -> CRISIS

        if(script.cardType == CardType.Crisis) {
            
        }
        
        // UI END
        if(GUI.changed) {
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }

    }
    private void DrawHorizontalRule()
    {
        GUILayout.Space(16);
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), new Color(0.5f, 0.5f, 0.5f, 1));
        GUILayout.Space(16);
    }
    private void DrawHeader(string text) {
        GUIStyle headerStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
        };

        GUILayout.Label(text, headerStyle);
        GUILayout.Space(8);
    }
}
#endif

public enum CardType {
    None,
    Buildable,
    Upgrade,
    Convert,
    Boost,
    Ability,
    Factor,
    Strike,
    Crisis
}

public enum CardRarity {
    None,
    Risk,
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}