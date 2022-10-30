using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GraphicsMenu : MonoBehaviour
{
    public TextMeshProUGUI cloudText;
    public TextMeshProUGUI postText;
    public TextMeshProUGUI screenText;
    public TextMeshProUGUI shadowText;
    public Image shadowQualityImage;
    public Button shadowQualityButton;
    public TextMeshProUGUI shadowQualityText;
    public Image shadowDistanceImage;
    public Slider shadowDistanceSlider;
    public TextMeshProUGUI shadowDistanceText;
    public TextMeshProUGUI textureText;
    public Slider renderSlider;
    public TextMeshProUGUI renderText;

    private void OnEnable()
    {
        if (MainGameManager.instance.settings.activePostProcessing)
            postText.text = "Post-Processing : ON";
        else
            postText.text = "Post-Processing : OFF";

        if (MainGameManager.instance.settings.activeClouds)
            cloudText.text = "Clouds : ON";
        else
            cloudText.text = "Clouds : OFF";

        if (Screen.fullScreen)
            screenText.text = "Fullscreen : ON";
        else
            screenText.text = "Fullscreen : OFF";

        ChangeShadowState();
        ChangeShadowState();
        shadowDistanceSlider.value = MainGameManager.instance.settings.shadowDistance;
        shadowDistanceText.text = "Shadows Distance : " + (int)MainGameManager.instance.settings.shadowDistance;

        SetQualityText(MainGameManager.instance.settings.shadowQuality);

        renderText.text =  "Render Distance : " + MainGameManager.instance.settings.renderDistance;
        renderSlider.value = MainGameManager.instance.settings.renderDistance;

        textureText.text = "Texture Pack : " + MainGameManager.instance.texturePacks[MainGameManager.instance.settings.currentTexturePack].name;
    }

    public void ChangePostProcessing()
    {
        if (MainGameManager.instance.settings.activePostProcessing)
        {
            MainGameManager.instance.settings.activePostProcessing = false;
            MainGameManager.instance.postProcessing.SetActive(false);
            postText.text = "Post-Processing : OFF";
        }
        else
        {
            MainGameManager.instance.settings.activePostProcessing = true;
            MainGameManager.instance.postProcessing.SetActive(true);
            postText.text = "Post-Processing : ON";
        }
    }

    public void ChangeClouds()
    {
        if (MainGameManager.instance.settings.activeClouds)
        {
            MainGameManager.instance.settings.activeClouds = false;
            MainGameManager.instance.clouds.SetActive(false);
            cloudText.text = "Clouds : OFF";
        }
        else
        {
            MainGameManager.instance.settings.activeClouds = true;
            MainGameManager.instance.clouds.SetActive(true);
            cloudText.text = "Clouds : ON";
        }
    }

    public void ChangeScreen()
    {
        if(Screen.fullScreen)
        {
            screenText.text = "Fullscreen : OFF";
            Screen.fullScreen = false;
        }
        else
        {
            screenText.text = "Fullscreen : ON";
            Screen.fullScreen = true;
        }
    }

    public void SetRenderDistance(float renderDistance)
    {
        renderText.text = "Render Distance : " + (int)renderDistance;
        MainGameManager.instance.settings.renderDistance = (int)renderDistance;
        if(TerrainGenerator.instance != null)
        {
            TerrainGenerator.instance.SetRenderDistance((int)renderDistance);
        }
    }

    public void ChangeShadowState()
    {
        if(MainGameManager.instance.settings.shadowType == LightShadows.None)
        {
            MainGameManager.instance.settings.shadowType = LightShadows.Hard;
            shadowText.text = "Shadows : ON";
            MainMenu.ChangeButtonState(shadowQualityImage, shadowQualityButton, true);
            MainMenu.ChangeSliderState(shadowDistanceImage, shadowDistanceSlider, true);
        }
        else
        {
            MainGameManager.instance.settings.shadowType = LightShadows.None;
            shadowText.text = "Shadows : OFF";
            MainMenu.ChangeButtonState(shadowQualityImage, shadowQualityButton, false);
            MainMenu.ChangeSliderState(shadowDistanceImage, shadowDistanceSlider, false);
        }
        MainGameManager.instance.SetShadowsType();
    }

    public void SetShadowDistance(float distance)
    {
        for (int i = 0; i < MainGameManager.instance.urp.Length; i++)
        {
            MainGameManager.instance.urp[i].shadowDistance = distance;
        }
        MainGameManager.instance.settings.shadowDistance = distance;
        shadowDistanceText.text = "Shadows Distance : " + (int)distance;
    }

    public void ChangeQuality()
    {
        MainGameManager.instance.settings.shadowQuality = (MainGameManager.instance.settings.shadowQuality + 1) % 5;
        QualitySettings.SetQualityLevel(MainGameManager.instance.settings.shadowQuality);
        SetQualityText(MainGameManager.instance.settings.shadowQuality);
    }

    public void SetQualityText(int q)
    {
        if (q == 0)
            shadowQualityText.text = "Shadows Quality : Very Low";
        else if(q == 1)
            shadowQualityText.text = "Shadows Quality : Low";
        else if (q == 2)
            shadowQualityText.text = "Shadows Quality : Medium";
        else if (q == 3)
            shadowQualityText.text = "Shadows Quality : High";
        else if (q == 4)
            shadowQualityText.text = "Shadows Quality : Very High";
    }

    public void ChangeTexturePack()
    {
        MainGameManager.instance.settings.currentTexturePack = (MainGameManager.instance.settings.currentTexturePack + 1) % MainGameManager.instance.texturePacks.Length;
        textureText.text = "Texture Pack : " + MainGameManager.instance.texturePacks[MainGameManager.instance.settings.currentTexturePack].name;
        MainGameManager.instance.SetTexturePack();
        if(PlayerController.instance != null)
        {
            PlayerController.instance.UpdateInventoryUI();
        }
    }
}
