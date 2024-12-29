using UnityEngine;

[CreateAssetMenu(fileName = "NewDecision", menuName = "Decisions/Decision")]
public class Decision : ScriptableObject
{
    public Buildable buildable;
    public int amount;
}
