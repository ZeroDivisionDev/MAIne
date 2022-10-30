using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Audio;

public class MainGameManager : MonoBehaviour
{

    public static MainGameManager instance;

    public UniversalRenderPipelineAsset[] urp;

    public enum Gamemode { Sandbox, Immortal, Survival}
    public Gamemode gamemode;
    public int seed;
    public string worldName;
    public SettingsData settings;
    public GameObject clouds;
    public GameObject postProcessing;
    public Light mainLight;
    public AudioMixer audioMixer;

    [Header("Materials & Textures")]
    public Material blockMat;
    public Material leavesMat;
    public Material flowerMat;
    public Material foliageMat;
    public Material playerMat;
    public Material zombieMat;
    public Item[] allItem;
    public Material[] particlesMat;
    public TexturePack[] texturePacks;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(clouds);
            Destroy(postProcessing);
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(clouds);
            DontDestroyOnLoad(postProcessing);
        }
    }

    private void Start()
    {
        LoadSettings();
        gamemode = Gamemode.Sandbox;
        ApplySettings();
    }

    public void SetTexturePack()
    {
        blockMat.mainTexture = texturePacks[settings.currentTexturePack].fullTexture;
        leavesMat.mainTexture = texturePacks[settings.currentTexturePack].fullTexture;
        flowerMat.mainTexture = texturePacks[settings.currentTexturePack].fullTexture;
        foliageMat.mainTexture = texturePacks[settings.currentTexturePack].foliageTexture;
        if (TerrainGenerator.instance != null)
        {
            foreach (KeyValuePair<Vector2Int, Chunk> ch in TerrainGenerator.instance.chunks)
            {
                if(ch.Value.shaderMat != null)
                    ch.Value.shaderMat.mainTexture = texturePacks[settings.currentTexturePack].foliageTexture;
            }
        }
        playerMat.mainTexture = texturePacks[settings.currentTexturePack].playerSkin;
        zombieMat.mainTexture = texturePacks[settings.currentTexturePack].zombieSkin;
        for (int i = 0; i < particlesMat.Length; i++)
        {
            particlesMat[i].mainTexture = texturePacks[settings.currentTexturePack].particlesTexture[i];
        }
        for (int i = 0; i < allItem.Length; i++)
        {
            allItem[i].sprite = texturePacks[settings.currentTexturePack].itemsSprite[i];
        }
    }

    public void SetShadowsType()
    {
        mainLight.shadows = settings.shadowType;
    }

    public void SaveSettings()
    {
        ES3.Save("Settings", settings, "settings.save");
    }

    public void LoadSettings()
    {
        if (ES3.FileExists("settings.save"))
            settings = ES3.Load<SettingsData>("Settings", "settings.save");
        else
            SaveSettings();
    }

    public void ApplySettings()
    {
        //Set volume
        float masterVolume = MainGameManager.instance.settings.masterVolume;
        MainGameManager.instance.audioMixer.SetFloat("Master Volume", masterVolume);
        float musicVolume = MainGameManager.instance.settings.musicVolume;
        MainGameManager.instance.audioMixer.SetFloat("Music Volume", musicVolume);
        //Set graphics
        SetTexturePack();
        SetShadowsType();
        MainGameManager.instance.postProcessing.SetActive(MainGameManager.instance.settings.activePostProcessing);
        MainGameManager.instance.clouds.SetActive(MainGameManager.instance.settings.activeClouds);
        for (int i = 0; i < MainGameManager.instance.urp.Length; i++)
        {
            MainGameManager.instance.urp[i].shadowDistance = MainGameManager.instance.settings.shadowDistance;
        }
        QualitySettings.SetQualityLevel(MainGameManager.instance.settings.shadowQuality);
    }

}
