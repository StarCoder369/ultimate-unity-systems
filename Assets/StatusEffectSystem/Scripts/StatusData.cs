using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Status", menuName = "Status/Status Data")]
public class StatusData : ScriptableObject
{
    public string statusName;
    public Sprite icon;
    public Color fillColor;


    [Header("Build Up")]
    public float maxBuildUp = 100f;
    public float defaultBuildUpPerHit = 25f;
    public float buildUpDecay = 10f;


    [Header("Active Status")]
    // This is how long the status effect lasts after it activates
    public float duration = 5f;

    [Header("Effects")]
    public List<StatusEffect> effects = new List<StatusEffect>();

}