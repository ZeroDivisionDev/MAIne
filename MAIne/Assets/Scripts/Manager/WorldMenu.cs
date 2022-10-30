using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldMenu : MonoBehaviour
{
    public GameObject playButton;
    public GameObject deleteButton;
    public GameObject scrollViewContent;

    public GameObject worldInfoPrefab;
    public GameObject noWorldText;

    List<WorldInfo> worldInfos;
    Image currentSelect;

    private void OnEnable()
    {
        ToggleButtons(false);
        //Debug.Log(System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
        LoadWorldsInfo();
    }

    private void OnDisable()
    {
        DeleteInfos();
    }

    void ToggleButtons(bool activate)
    {
        MainMenu.ChangeButtonState(playButton.GetComponent<Image>(), playButton.GetComponent<Button>(), activate);
        MainMenu.ChangeButtonState(deleteButton.GetComponent<Image>(), deleteButton.GetComponent<Button>(), activate);
    }

    void LoadWorldsInfo()
    {
        string[] worlds = ES3.GetDirectories(Application.persistentDataPath);
        if (worlds.Length == 0)
            return;
        worldInfos = new List<WorldInfo>();
        for (int i = 0; i < worlds.Length; i++)
        {
            if (ES3.FileExists(worlds[i] + "/player.save") && ES3.FileExists(worlds[i] + "/world.save"))
            {
                noWorldText.SetActive(false);
                WorldInfo worldInfo = Instantiate(worldInfoPrefab, scrollViewContent.transform).GetComponent<WorldInfo>();
                string name = ES3.Load<string>("WorldName", worlds[i] + "/world.save");
                string info = ES3.Load<string>("WorldDate", worlds[i] + "/player.save")
                    +  "\n"
                    + ES3.Load<MainGameManager.Gamemode>("GameMode", worlds[i] + "/player.save").ToString() + ",  "
                    + ConvertTime(ES3.Load<int>("PlayTime", worlds[i] + "/player.save"));
                worldInfo.SetText(name, info);
                worldInfo.worldMenu = this;
                worldInfos.Add(worldInfo);
            }
        }
    }

    string ConvertTime(int t)
    {
        TimeSpan result = TimeSpan.FromSeconds(t);
        string[] values = result.ToString().Split(':');
        return values[0] + "h" + values[1] + "m" + values[2] + "s";
    }

    void DeleteInfos()
    {
        for (int i = 0; i < worldInfos.Count; i++)
        {
            Destroy(worldInfos[i].gameObject);
        }
    }

    public void SelectWorld(Image select, string name)
    {
        if (currentSelect != null)
            currentSelect.color = new Color(1, 1, 1, 0);
        currentSelect = select;
        currentSelect.color = new Color(1, 1, 1, 1);
        MainGameManager.instance.worldName = name;
        MainGameManager.instance.gamemode = ES3.Load<MainGameManager.Gamemode>("GameMode", name + "/player.save");
        ToggleButtons(true);
    }

    public void DeleteWorld()
    {
        ES3.DeleteDirectory(MainGameManager.instance.worldName);
        DeleteInfos();
        LoadWorldsInfo();
        ToggleButtons(false);
    }
}
