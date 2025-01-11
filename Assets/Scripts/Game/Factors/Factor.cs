using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "NewFactor", menuName = "Game/Factor")]
public class Factor : ScriptableObject
{
    [SerializeField] private Sprite _icon;
    [SerializeField] private FactorType _factorType;
    [SerializeField] private string _description;
    [SerializeField] private int _length;
    //event & demand
    [SerializeField] private Stat _statEffect;
    [SerializeField] private int _statEffectValue;
    //demand & quest
    [SerializeField] private FactorRequirement _requirement;
    [SerializeField] private Buildable _requiredBuildable;
    [SerializeField] private Stat _requiredStat;
    [SerializeField] private int _requiredStatValue;

    //quest
    [SerializeField] private FactorReward _reward;
    [SerializeField] private Buildable _buildableReward;
    [SerializeField] private Stat _statReward;
    [SerializeField] private int _statRewardValue;

    public Sprite icon { get => _icon; set => _icon = value; }
    public FactorType factorType { get => _factorType; set => _factorType = value; }
    public string description { get => _description; set => _description = value; }
    public int length { get => _length; set => _length = value; }
    public Stat statEffect { get => _statEffect; set => _statEffect = value; }
    public int statEffectValue { get => _statEffectValue; set => _statEffectValue = value; }
    public FactorRequirement requirement { get => _requirement; set => _requirement = value; }
    public FactorReward reward { get => _reward; set => _reward = value; }
    public Buildable requiredBuildable { get => _requiredBuildable; set => _requiredBuildable = value; }
    public Stat requiredStat { get => _requiredStat; set => _requiredStat = value; }
    public int requiredStatValue { get => _requiredStatValue; set => _requiredStatValue = value; }
    public Buildable buildableReward { get => _buildableReward; set => _buildableReward = value; }
    public Stat statReward { get => _statReward; set => _statReward = value; }
    public int statRewardValue { get => _statRewardValue; set => _statRewardValue = value; }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Factor))]
public class FactorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var script = (Factor)target;

        // UI START
        GUILayout.Space(16);

        // HEADER SECTION
        GUILayout.BeginHorizontal();

        script.icon = EditorGUILayout.ObjectField(script.icon, typeof(Sprite), false, GUILayout.Width(64), GUILayout.Height(64)) as Sprite;

        GUILayout.Space(16);
        GUILayout.Label(script.name, new GUIStyle(EditorStyles.label) { fontSize = 24, fontStyle = FontStyle.Bold } );
        GUILayout.EndHorizontal();

        // DESCRIPTION SECTION
        DrawHorizontalRule();
        DrawHeader("Description");

        script.description = EditorGUILayout.TextArea(script.description, GUILayout.Height(64));

        // FACTOR MAIN SECTION
        DrawHorizontalRule();
        DrawHeader("Factor");

        script.length = EditorGUILayout.IntField("Factor Length (Days)", script.length);
        script.factorType = (FactorType)EditorGUILayout.EnumPopup("Factor Type", script.factorType);

        if(script.factorType == FactorType.Event || script.factorType == FactorType.Demand) {
            GUILayout.Space(16);

            script.statEffect = (Stat)EditorGUILayout.EnumPopup($"Stat Effect", script.statEffect);
            script.statEffectValue = EditorGUILayout.IntField("Effect Value", script.statEffectValue);
        }
            
        if(script.factorType == FactorType.Demand || script.factorType == FactorType.Quest) {
            GUILayout.Space(16);

            script.requirement = (FactorRequirement)EditorGUILayout.EnumPopup($"{script.factorType} Requirement", script.requirement);
            if(script.requirement == FactorRequirement.Buildable) {
                script.requiredBuildable = (Buildable)EditorGUILayout.ObjectField("Buildable", script.requiredBuildable, typeof(Buildable), false);
            } else if(script.requirement == FactorRequirement.Stat) {
                script.requiredStat = (Stat)EditorGUILayout.EnumPopup("Stat", script.requiredStat);
                script.requiredStatValue = EditorGUILayout.IntField("Value", script.requiredStatValue);
            }
        }

        if(script.factorType == FactorType.Quest) {
            GUILayout.Space(16);

            script.reward = (FactorReward)EditorGUILayout.EnumPopup("Quest Reward", script.reward);
            if(script.reward == FactorReward.Buildable) {
                script.buildableReward = (Buildable)EditorGUILayout.ObjectField("Buildable", script.buildableReward, typeof(Buildable), false);
            } else if(script.reward == FactorReward.Stat) {
                script.statReward = (Stat)EditorGUILayout.EnumPopup("Stat", script.statReward);
                script.statRewardValue = EditorGUILayout.IntField("Value", script.statRewardValue);
            }
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


public enum FactorType {
    None,
    Event,
    Demand,
    Quest
}
public enum FactorReward {
    Buildable,
    Stat
}
public enum FactorRequirement {
    Buildable,
    Stat
}
