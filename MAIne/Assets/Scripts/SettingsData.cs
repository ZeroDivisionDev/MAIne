using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Settings", menuName = "Settings preset")]
public class SettingsData : ScriptableObject
{
    [Header("Generic settings")]
    public float masterVolume = 0;
    public float musicVolume = 0;
    [Range(0.01f,0.21f)]
    public float mouseSensitivity = 0.1f;

    [Header("Graphic settings")]
    public bool activePostProcessing = true;
    public bool activeClouds = true;
    public float shadowDistance = 75;
    public int shadowQuality = 3;
    public LightShadows shadowType = LightShadows.Hard;
    public int renderDistance = 8;
    public int currentTexturePack = 0;

}
