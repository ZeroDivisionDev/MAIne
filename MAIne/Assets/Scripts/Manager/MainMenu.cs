using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public TMP_InputField worldInput;
    public TextMeshProUGUI gamemodeText;
    public TextMeshProUGUI gamemodeInfo;
    public TextMeshProUGUI titleText;
    public CanvasGroup titleCanvas;
    public Image playImage;
    public Button playButton;
    public int gamemode = 0;
    public Light menuLight;

    public void Start()
    {
        MainGameManager.instance.mainLight = menuLight;
        MainGameManager.instance.SetShadowsType();
        LeanTitle();
    }

    public void StartGame()
    {
        AudioManager.instance.StopMusic();
        MainGameManager.instance.seed = MainGameManager.instance.worldName.GetHashCode();
        MainGameManager.instance.clouds.transform.localScale = new Vector3(75, 1, 75);
        SceneManager.LoadScene(1);
    }

    public void StartNewGame()
    {
        AudioManager.instance.StopMusic();
        MainGameManager.instance.worldName = worldInput.text;
        MainGameManager.instance.seed = worldInput.text.GetHashCode();
        MainGameManager.instance.clouds.transform.localScale = new Vector3(75, 1, 75);

        //Save data
        ES3.Save("WorldName", worldInput.text, worldInput.text + "/world.save");
        ES3.Save("WorldDate", System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"), worldInput.text + "/player.save");
        ES3.Save("PlayTime", 0, worldInput.text + "/player.save");
        ES3.Save("GameMode", MainGameManager.instance.gamemode, worldInput.text + "/player.save");
       
        SceneManager.LoadScene(1);
    }

    public void ChangeGameMode()
    {
        gamemode = (gamemode + 1) % 3;
        if (gamemode == 0)
        {
            gamemodeText.text = "Game Mode : Sandbox";
            gamemodeInfo.text = "Details : Explore the world freely";
            MainGameManager.instance.gamemode = MainGameManager.Gamemode.Sandbox;
            ChangeButtonState(playImage, playButton, true);
        }
        else if (gamemode == 1)
        {
            gamemodeText.text = "Game Mode : Immortal";
            gamemodeInfo.text = "Details : Explore the world freely and ignore its dangers";
            MainGameManager.instance.gamemode = MainGameManager.Gamemode.Immortal;
            ChangeButtonState(playImage, playButton, true);
        }
        else if (gamemode == 2)
        {
            gamemodeText.text = "Game Mode : Survival";
            gamemodeInfo.text = "COMING LATER";
            MainGameManager.instance.gamemode = MainGameManager.Gamemode.Survival;
            ChangeButtonState(playImage, playButton, false);
        }
    }

    public static void ChangeButtonState(Image image, Button button, bool activate)
    {
        if (activate)
        {
            image.color = new Color(1, 1, 1, 1);
            button.interactable = true;
        }
        else
        {
            image.color = new Color(1, 1, 1, 0.8f);
            button.interactable = false;
        }
    }

    public static void ChangeSliderState(Image image, Slider button, bool activate)
    {
        if (activate)
        {
            image.color = new Color(1, 1, 1, 1);
            button.interactable = true;
        }
        else
        {
            image.color = new Color(1, 1, 1, 0.8f);
            button.interactable = false;
        }
    }

    public void QuitGame()
    {
        Debug.Log("Closing game");
        Application.Quit();
    }

    void LeanTitle()
    {
        LeanTween.scale(titleText.gameObject, Vector3.one, 4f).setEaseInOutSine();
        LeanTween.alphaCanvas(titleCanvas, 1f, 3f);
    }
}
