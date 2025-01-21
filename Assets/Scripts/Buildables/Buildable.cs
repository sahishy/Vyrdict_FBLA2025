using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "NewBuildable", menuName = "Game/Buildable")]
public class Buildable : ScriptableObject
{
    [SerializeField] private Sprite _icon;
    [SerializeField] private string _description;
    //static effects
    [SerializeField] private int _environmentEffect;
    [SerializeField] private int _happinessEffect;
    [SerializeField] private int _economyEffect;
    //buildable type
    [SerializeField] private Stat _buildableFocus;
        //buildable type > all
        [SerializeField] private Connection _requiredConnection;
        [SerializeField] private string _positiveRequiredConnectionStatusText;
        [SerializeField] private string _negativeRequiredConnectionStatusText;
        //buildable type > natural
        [SerializeField] private int _pollutionInfluence;
        [SerializeField] private string _pollutionInfluenceStatusText;
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
    public int environmentEffect { get => _environmentEffect; set => _environmentEffect = value; }
    public int happinessEffect { get => _happinessEffect; set => _happinessEffect = value; }
    public int economyEffect { get => _economyEffect; set => _economyEffect = value; }
    public Connection requiredConnection { get => _requiredConnection; set => _requiredConnection = value; }
    public Stat buildableFocus { get => _buildableFocus; set => _buildableFocus = value; }
    public int pollutionInfluence { get => _pollutionInfluence; set => _pollutionInfluence = value; }
    public int residents { get => _residents; set => _residents = value; }
    public int requiredCustomers { get => _requiredCustomers; set => _requiredCustomers = value; }
    public Buildable upgrade { get => _upgrade; set => _upgrade = value; }
    public GameObject tile { get => _tile; set => _tile = value; }
    public string positiveRequiredConnectionStatusText { get => _positiveRequiredConnectionStatusText; set => _positiveRequiredConnectionStatusText = value; }
    public string negativeRequiredConnectionStatusText { get => _negativeRequiredConnectionStatusText; set => _negativeRequiredConnectionStatusText = value; }
    public string pollutionInfluenceStatusText { get => _pollutionInfluenceStatusText; set => _pollutionInfluenceStatusText = value; }
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

        // EFFECTS SECTION
        DrawHorizontalRule();
        DrawHeader("All Buildables");

        script.environmentEffect = (int)EditorGUILayout.Slider("Environment", script.environmentEffect, -5, 5);
        script.happinessEffect = (int)EditorGUILayout.Slider("Happiness", script.happinessEffect, -5, 5);
        script.economyEffect = (int)EditorGUILayout.Slider("Economy", script.economyEffect, -5, 5);

        // BUILDABLE TYPE SECTION
        DrawHorizontalRule();
        DrawHeader("Buildable Focus");

        script.buildableFocus = (Stat)EditorGUILayout.EnumPopup("Focused Stat", script.buildableFocus);
        script.requiredConnection = EditorGUILayout.ObjectField("Required Connection", script.requiredConnection, typeof(Connection), false) as Connection;
        if(script.requiredConnection != null) {
            script.positiveRequiredConnectionStatusText = EditorGUILayout.TextField("Positive Status", script.positiveRequiredConnectionStatusText);
            script.negativeRequiredConnectionStatusText = EditorGUILayout.TextField("Negative Status", script.negativeRequiredConnectionStatusText);
        }

        if(script.buildableFocus == Stat.Environment) {
            script.pollutionInfluence = (int)EditorGUILayout.Slider("Pollution Influence", script.pollutionInfluence, 0, 3);
            script.pollutionInfluenceStatusText = EditorGUILayout.TextField("Status", script.pollutionInfluenceStatusText);
        } else if(script.buildableFocus == Stat.Happiness) {
            script.residents = (int)EditorGUILayout.Slider("Residents", script.residents, 0, 3);
        } else if(script.buildableFocus == Stat.Economy) {
            script.requiredCustomers = (int)EditorGUILayout.Slider("Required Customers", script.requiredCustomers, 0, 3);
            script.positiveRequiredCustomersStatusText = EditorGUILayout.TextField("Positive Status", script.positiveRequiredCustomersStatusText);
            script.negativeRequiredCustomersStatusText = EditorGUILayout.TextField("Negative Status", script.negativeRequiredCustomersStatusText);
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