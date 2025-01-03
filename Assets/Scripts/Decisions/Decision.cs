using UnityEngine;

[CreateAssetMenu(fileName = "NewDecision", menuName = "Game/Decision")]
public class Decision : ScriptableObject
{
    public Buildable buildable;
    public int amount;
}
