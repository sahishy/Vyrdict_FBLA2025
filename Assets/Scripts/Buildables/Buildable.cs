using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "NewBuildable", menuName = "Game/Buildable")]
public class Buildable : ScriptableObject
{
    [SerializeField] private Sprite _icon;
    [SerializeField] private string _description;
    [SerializeField] private int _environmentEffect;
    [SerializeField] private int _happinessEffect;
    [SerializeField] private int _economyEffect;
    [SerializeField] private GameObject _tile;

    public Sprite icon { get => _icon; set => _icon = value; }
    public string description { get => _description; set => _description = value; }
    public int environmentEffect { get => _environmentEffect; set => _environmentEffect = value; }
    public int happinessEffect { get => _happinessEffect; set => _happinessEffect = value; }
    public int economyEffect { get => _economyEffect; set => _economyEffect = value; }
    public GameObject tile { get => _tile; set => _tile = value; }
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
        DrawHeader("Effects");

        script.environmentEffect = (int)EditorGUILayout.Slider("Environment", script.environmentEffect, -3, 3);
        script.happinessEffect = (int)EditorGUILayout.Slider("Happiness", script.happinessEffect, -3, 3);
        script.economyEffect = (int)EditorGUILayout.Slider("Economy", script.economyEffect, -3, 3);

        // REFERENCES SECTION
        DrawHorizontalRule();
        DrawHeader("References");

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

