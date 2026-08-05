using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Status", menuName = "Status/Status Data")]
public class StatusData : ScriptableObject
{
    public string statusName;
    public Sprite icon;


    [Header("Build Up")]
    public float maxBuildUp = 100f;
    public float defaultBuildUpPerHit = 25f;
    public float buildUpDecay = 10f;


    [Header("Active Status")]
    public float duration = 5f;
    public float activeDecay = 1f;


    public GameObject effectPrefab;


    [Header("Effects")]
    public List<StatusEffect> effects = new List<StatusEffect>();
}