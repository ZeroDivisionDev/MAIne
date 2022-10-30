using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    public LayerMask hitLayer;
    public GameObject player;
    public GameObject loadingScreen;
    public GameObject gameCanvas;
    public GameObject dieScreen;
    public GameObject pauseScreen;
    public GameObject pauseMenu;
    public GameObject optionMenu;
    public GameObject graphicsMenu;
    public GameObject inventoryScreen;
    public TextMeshProUGUI loadingText;
    public TextMeshProUGUI scoreText;
    public RectTransform bar;
    public GameObject gravePrefab;
    public int spawnerDestroyed;
    public int zombieKilled;

    public bool isLoading;
    public bool gamePaused = false;

    public Light gameLight;

    public Vector2 mousePos;

    Vector3 spawnPosition;

    int numChunk;
    int loadChunk;
    bool inventoryOpen = false;
    bool isRespawn;

    Inputs inputs;

    //float loadTimer = 0.0f;

    private void Awake()
    {
        instance = this;
        inputs = new Inputs();
        inputs.Player.Escape.performed += _ => Pause();
        inputs.Player.OpenInventory.performed += _ => ToggleInventory();
        inputs.Player.MousePosition.performed += ctx => mousePos = ctx.ReadValue<Vector2>();
    }

    private void OnEnable()
    {
        inputs.Enable();
    }

    private void OnDisable()
    {
        inputs.Disable();
    }

    private void Start()
    {
        MainGameManager.instance.mainLight = gameLight;
        MainGameManager.instance.SetShadowsType();
        ResetGame();
        spawnPosition = transform.position;
        isRespawn = false;
        LoadPlayerData();
    }

    public void ResetGame()
    {
        TerrainGenerator.instance.maxGen = (SystemInfo.processorCount-1)*3;
        TerrainGenerator.instance.waitTime = Time.fixedDeltaTime;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isLoading = true;
        numChunk = TerrainGenerator.instance.renderDistance * TerrainGenerator.instance.renderDistance * 4;
        loadChunk = 0;
        gameCanvas.SetActive(false);
        dieScreen.SetActive(false);
        loadingScreen.SetActive(true);
        bar.localScale = new Vector3(0, 1, 1);
        TerrainGenerator.instance.needRegenerate = true;
        ResetPlayer();
        StartCoroutine(Loading());
    }

    void ResetPlayer()
    {
        player.transform.position = spawnPosition;
        PlayerController.instance.mouseSensitivity = MainGameManager.instance.settings.mouseSensitivity;
        PlayerController.instance.enabled = true;
        PlayerController.instance.playerMovement.enabled = true; 
        PlayerController.instance.playerMovement.movement = Vector2.zero;
        PlayerController.instance.dieRotation.enabled = false;
        PlayerController.instance.ResetInventory();
        PlayerController.instance.ResetHealth();
        PlayerController.instance.playerMovement.animator.speed = 1f;
        PlayerController.instance.skin.materials = new Material[] { MainGameManager.instance.playerMat };
        if(MainGameManager.instance.gamemode == MainGameManager.Gamemode.Immortal)
            PlayerController.instance.isInvicible = true;
        else
            PlayerController.instance.isInvicible = false;
        player.SetActive(false);
    }

    void LoadPlayerData()
    {
        if (!ES3.FileExists(MainGameManager.instance.worldName + "/player.save") || !ES3.KeyExists("PlayerHealth", MainGameManager.instance.worldName + "/player.save"))
            return;
        int health = ES3.Load<int>("PlayerHealth", MainGameManager.instance.worldName + "/player.save");
        spawnPosition = ES3.Load<Vector3>("SpawnPosition", MainGameManager.instance.worldName + "/player.save");
        if (health > 0)
        {
            player.transform.position = ES3.Load<Vector3>("PlayerPosition", MainGameManager.instance.worldName + "/player.save");
            PlayerController.instance.ResetHealth(health);
            ES3.LoadInto("PlayerInventory", MainGameManager.instance.worldName + "/player.save", PlayerController.instance.inventory);
        }
        else
        {
            player.transform.position = spawnPosition;
        }
        if(ES3.KeyExists("GraveTransform", MainGameManager.instance.worldName + "/player.save"))
        {
            Transform graveTransform = ES3.Load<Transform>("GraveTransform", MainGameManager.instance.worldName + "/player.save");
            Instantiate(gravePrefab, graveTransform.position, graveTransform.rotation);
            Grave.instance.graveInventory = ES3.Load<ItemInventory[]>("GraveInventory", MainGameManager.instance.worldName + "/player.save");
        }
    }

    void StartGame()
    {
        //Debug.Log(loadTimer);
        gamePaused = false;
        TerrainGenerator.instance.maxGen = SystemInfo.processorCount - 1;
        TerrainGenerator.instance.waitTime = 0.1f;
        RaycastHit hitInfo;
        Vector3 checkPos;
        if (isRespawn)
        {
            checkPos = spawnPosition;
        }
        else
        {
            isRespawn = true;
            checkPos = player.transform.position;
        }
        //Don't have the head in a block and have the feet on one
        if(!Physics.Raycast(checkPos, Vector3.up, out hitInfo, 1.5f, hitLayer) && Physics.Raycast(checkPos + Vector3.up * 0.5f, Vector3.down, out hitInfo, 1f, hitLayer))
        {
            player.transform.position = checkPos;
        }
        else if (Physics.Raycast(checkPos + Vector3.up * 500, Vector3.down, out hitInfo, 1000f, hitLayer))
        {
            player.transform.position = hitInfo.point;
        }
        player.SetActive(true);
        gameCanvas.SetActive(true);
        loadingScreen.SetActive(false);
    }

    public void SetSpawnPoint()
    {
        Vector3 playerPos = PlayerController.instance.transform.position;
        spawnPosition = new Vector3(Mathf.Floor(playerPos.x) + .5f, playerPos.y + 0.05f, Mathf.Floor(playerPos.z) + .5f);
    }

    IEnumerator Loading()
    {
        while(isLoading)
        {
            yield return new WaitForSeconds(1f);
            loadingText.text = "Loading   ";
            yield return new WaitForSeconds(1f);
            loadingText.text = "Loading.  ";
            yield return new WaitForSeconds(1f);
            loadingText.text = "Loading.. ";
            yield return new WaitForSeconds(1f);
            loadingText.text = "Loading...";
        }
    }

    public void SetDieScreen()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        dieScreen.SetActive(true);
        gamePaused = true;
        scoreText.text = "Zombies killed : " + zombieKilled + "\n" +
                    "Spawners destroyed : " + spawnerDestroyed;
    }



    public void UpadteBar(int nChunk)
    {
        loadChunk += nChunk;
        float xscale = loadChunk / (float)numChunk;
        bar.localScale = new Vector3(xscale, 1, 1);
        if(loadChunk >= numChunk && isLoading)
        {
            isLoading = false;
            StartGame();
        }
    }

    public void Pause()
    {
        //Do nothing when died or loading
        if (loadingScreen.activeSelf || dieScreen.activeSelf)
            return;

        //Close inventory if open
        if (inventoryOpen)
        {
            ToggleInventory();
            return;
        }

        if (!gamePaused)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            PlayerController.instance.inputs.Disable();
            gamePaused = true;
            pauseScreen.SetActive(true);
            pauseMenu.SetActive(true);
            optionMenu.SetActive(false);
            graphicsMenu.SetActive(false);
            Time.timeScale = 0f;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            PlayerController.instance.inputs.Enable();
            gamePaused = false;
            pauseScreen.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    public void LoadMenu()
    {
        MainGameManager.instance.clouds.transform.localScale = new Vector3(150, 1, 150);
        gamePaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    void ToggleInventory()
    {
        if (loadingScreen.activeSelf || dieScreen.activeSelf || gamePaused)
            return;

        if (!inventoryOpen) //Opening
        {
            PlayerController.instance.inputs.Disable();
            PlayerController.instance.playerMovement.movement = Vector2.zero;
            PlayerController.instance.UpdateInventoryUI();
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            inventoryOpen = true;
            PlayerController.instance.UpdateInventoryUI();
            inventoryScreen.SetActive(true);
        }
        else //Closing
        {
            PlayerController.instance.inputs.Enable();
            FollowMouse.instance.ResetItem();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            inventoryOpen = false;
            inventoryScreen.SetActive(false);
        }
    }

    public bool IsInventoryOpen()
    {
        return inventoryOpen;
    }

    private void OnDestroy()
    {
        SaveWorld();
        SavePlayer();
    }

    private void OnApplicationQuit()
    {
        SaveWorld();
        SavePlayer();
    }

    void SaveWorld()
    {
        int currentPlayTime = ES3.Load<int>("PlayTime", MainGameManager.instance.worldName + "/player.save");
        currentPlayTime += (int)Time.timeSinceLevelLoad;
        ES3.Save("PlayTime", currentPlayTime, MainGameManager.instance.worldName + "/player.save");
        ES3.Save("WorldDate", System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"), MainGameManager.instance.worldName + "/player.save");
    }

    void SavePlayer()
    {
        ES3.Save("SpawnPosition", spawnPosition, MainGameManager.instance.worldName + "/player.save");
    }
}