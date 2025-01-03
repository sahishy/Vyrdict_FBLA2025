using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewConnection", menuName = "Game/Connection")]
public class Connection : ScriptableObject
{
    [Header("Stats")]
    public int environmentEffect;
    public int happinessEffect;
    public int economyEffect;
    [Header("Connection")]
    //public bool stacks;
    public List<Buildable> rootBuildables = new List<Buildable>();
    public List<Buildable> branchBuildables = new List<Buildable>();
}
