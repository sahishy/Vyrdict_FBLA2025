using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "NewBuildable", menuName = "Game/Buildable")]
public class Buildable : ScriptableObject
{
    [SerializeField] private Sprite _icon;
    [SerializeField] private string _description;
    //static effects
    [SerializeField] private int _suppliesEffect;
    [SerializeField] private int _foodEffect;
    [SerializeField] private int _goldEffect;
    //buildable type
    [SerializeField] private bool _pollutable;
    [SerializeField] private Stat _buildableFocus;
        //buildable type > all
        [SerializeField] private Connection _requiredConnection;
        [SerializeField] private string _positiveRequiredConnectionStatusText;
        [SerializeField] private string _negativeRequiredConnectionStatusText;
        //buildable type > natural
        [SerializeField] private int _pollutionInfluence;
        [SerializeField] private string _positivePollutionInfluenceStatusText;
        [SerializeField] private string _negativePollutionInfluenceStatusText;
        //buildable type > residential
        [SerializeField] private int _residents;
        //buildable type > economic
        [SerializeField] private int _requiredCustomers;
        [SerializeField] private string _positiveRequiredCustomersStatusText;
        [SerializeField] private string _negativeRequiredCustomersStatusText;
    //references
    [SerializeField] private Buildable _upgrade;
    [SerializeField] private GameObject _tile;

    public Sprite icon { get => _icon; set => _icon = value; }
    public string description { get => _description; set => _description = value; }
    public int suppliesEffect { get => _suppliesEffect; set => _suppliesEffect = value; }
    public int foodEffect { get => _foodEffect; set => _foodEffect = value; }
    public int goldEffect { get => _goldEffect; set => _goldEffect = value; }
    public Connection requiredConnection { get => _requiredConnection; set => _requiredConnection = value; }
    public bool pollutable { get => _pollutable; set => _pollutable = value; }
    public Stat buildableFocus { get => _buildableFocus; set => _buildableFocus = value; }
    public int pollutionInfluence { get => _pollutionInfluence; set => _pollutionInfluence = value; }
    public int residents { get => _residents; set => _residents = value; }
    public int requiredCustomers { get => _requiredCustomers; set => _requiredCustomers = value; }
    public Buildable upgrade { get => _upgrade; set => _upgrade = value; }
    public GameObject tile { get => _tile; set => _tile = value; }
    public string positiveRequiredConnectionStatusText { get => _positiveRequiredConnectionStatusText; set => _positiveRequiredConnectionStatusText = value; }
    public string negativeRequiredConnectionStatusText { get => _negativeRequiredConnectionStatusText; set => _negativeRequiredConnectionStatusText = value; }
    public string positivePollutionInfluenceStatusText { get => _positivePollutionInfluenceStatusText; set => _positivePollutionInfluenceStatusText = value; }
    public string negativePollutionInfluenceStatusText { get => _negativePollutionInfluenceStatusText; set => _negativePollutionInfluenceStatusText = value; }
    public string positiveRequiredCustomersStatusText { get => _positiveRequiredCustomersStatusText; set => _positiveRequiredCustomersStatusText = value; }
    public string negativeRequiredCustomersStatusText { get => _negativeRequiredCustomersStatusText; set => _negativeRequiredCustomersStatusText = value; }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Buildable))]
public class ItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var script = (Buildable)target;
        var textAreaStyle = new GUIStyle(EditorStyles.textArea) { wordWrap = true };

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

        script.description = EditorGUILayout.TextArea(script.description, textAreaStyle, GUILayout.Height(64));

        // EFFECTS SECTION
        DrawHorizontalRule();
        DrawHeader("All Buildables");

        script.suppliesEffect = (int)EditorGUILayout.Slider("Materials", script.suppliesEffect, -3, 3);
        script.foodEffect = (int)EditorGUILayout.Slider("Food", script.foodEffect, -3, 3);
        script.goldEffect = (int)EditorGUILayout.Slider("Gold", script.goldEffect, -3, 3);

        script.pollutable = EditorGUILayout.Toggle("Pollutable", script.pollutable);

        // BUILDABLE FOCUS SECTION
        DrawHorizontalRule();
        DrawHeader("Buildable Focus");

        script.buildableFocus = (Stat)EditorGUILayout.EnumPopup("Focused Stat", script.buildableFocus);
        script.requiredConnection = EditorGUILayout.ObjectField("Required Connection", script.requiredConnection, typeof(Connection), false) as Connection;
        if(script.requiredConnection != null) {
            EditorGUILayout.Space(16);
            EditorGUILayout.LabelField("Positive Status (Connection)");
            script.positiveRequiredConnectionStatusText = EditorGUILayout.TextArea(script.positiveRequiredConnectionStatusText, textAreaStyle);
            EditorGUILayout.LabelField("Negative Status (Connection)");
            script.negativeRequiredConnectionStatusText = EditorGUILayout.TextArea(script.negativeRequiredConnectionStatusText, textAreaStyle);
            EditorGUILayout.Space(16);
        }

        if(script.buildableFocus == Stat.Supplies || script.buildableFocus == Stat.Food) {
            script.pollutionInfluence = (int)EditorGUILayout.Slider("Pollution Influence", script.pollutionInfluence, 0, 3);
            if(script.pollutionInfluence == 0) {
                EditorGUILayout.Space(16);
                EditorGUILayout.LabelField("Status (Pollution)");
                script.positivePollutionInfluenceStatusText = EditorGUILayout.TextArea(script.positivePollutionInfluenceStatusText, textAreaStyle);
                EditorGUILayout.Space(16);
            } else {
                EditorGUILayout.Space(16);
                EditorGUILayout.LabelField("Positive Status (Pollution)");
                script.positivePollutionInfluenceStatusText = EditorGUILayout.TextArea(script.positivePollutionInfluenceStatusText, textAreaStyle);
                EditorGUILayout.LabelField("Negative Status (Pollution)");
                script.negativePollutionInfluenceStatusText = EditorGUILayout.TextArea(script.negativePollutionInfluenceStatusText, textAreaStyle);
                EditorGUILayout.Space(16);
            }
        } else if(script.buildableFocus == Stat.Gold) {
            script.requiredCustomers = (int)EditorGUILayout.Slider("Required Customers", script.requiredCustomers, 0, 3);
            if(script.requiredCustomers == 0) {
                EditorGUILayout.Space(16);
                EditorGUILayout.LabelField("Status (Customer)");
                script.positiveRequiredCustomersStatusText = EditorGUILayout.TextArea(script.positiveRequiredCustomersStatusText, textAreaStyle);
                EditorGUILayout.Space(16);
            } else {
                EditorGUILayout.Space(16);
                EditorGUILayout.LabelField("Positive Status (Customer)");
                script.positiveRequiredCustomersStatusText = EditorGUILayout.TextArea(script.positiveRequiredCustomersStatusText, textAreaStyle);
                EditorGUILayout.LabelField("Negative Status (Customer)");
                script.negativeRequiredCustomersStatusText = EditorGUILayout.TextArea(script.negativeRequiredCustomersStatusText, textAreaStyle);
                EditorGUILayout.Space(16);
            }
        } else if(script.buildableFocus == Stat.Population) {
            script.residents = (int)EditorGUILayout.Slider("Residents", script.residents, 0, 3);
        }

        // REFERENCES SECTION
        DrawHorizontalRule();
        DrawHeader("References");

        script.upgrade = EditorGUILayout.ObjectField("Upgrade", script.upgrade, typeof(Buildable), false) as Buildable;
        script.tile = EditorGUILayout.ObjectField("Tile", script.tile, typeof(GameObject), false) as GameObject;

        if(GUILayout.Button("Auto-Assign Prefab")) {
            FindAndAssignPrefab(script);
        }

        // UI END
        if(GUI.changed) {
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }

    }

    private void FindAndAssignPrefab(Buildable script)
    {
        string[] guids = AssetDatabase.FindAssets("t:GameObject");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (obj != null && obj.name == script.name)
            {
                script.tile = obj;
                EditorUtility.SetDirty(script);
                return;
            }
        }

        Debug.LogError($"No prefab found with the name '{script.name}'.");
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