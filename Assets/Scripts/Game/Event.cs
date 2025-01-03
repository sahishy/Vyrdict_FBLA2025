using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEvent", menuName = "Game/Event")]
public class Event : ScriptableObject
{
    [Header("Event")]
    [TextArea] public string description;
    [Header("Stats")]
    public int environmentEffect;
    public int happinessEffect;
    public int economyEffect;
}
