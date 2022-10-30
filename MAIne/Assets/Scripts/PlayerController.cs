using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MilkShake;
using Cinemachine;

public class PlayerController : MonoBehaviour
{

    public static PlayerController instance;

    public Shaker camShaker;
    public ShakePreset shakePreset;
    public CinemachineFreeLook thirdCamLook;
    public HealthMovement[] healthMovements;

    //[Range(0.0001f, 0.0006f)]
    public float mouseSensitivity = 100f;
    //public Rigidbody playerRb;
    public Camera cam;
    public Transform head;
    public Transform headFollow;
    public float maxDist = 6f;
    public LayerMask hitLayer;
    public LayerMask buildLayer;
    public SkinnedMeshRenderer skin;
    public GameObject thirdCam;
    public GameObject crossHair;

    public ItemInventory[] inventory;
    public GameObject[] itemUI;
    public GameObject[] inventoryUI;
    public List<GameObject> lifeUI;

    public GameObject hitParticles;
    public ParticleSystem eatParticles;

    public Transform armature;
    public Transform handHolding;
    public Animator handAnimator;
    public GameObject handMesh;
    public GameObject reloadBar;
    public Material hitMat;
    public GameObject infoOverlay;
    public DieRotation dieRotation;
    public GameObject overlayCube;
    public GameObject gravePrefab;
    GameObject itemInHand;
    
    public PlayerMovementV2 playerMovement;

    bool firstPerson = true;
    bool isPunch = false;
    public bool isInvicible = false;
    
    public Inputs inputs;
    Vector2 cameraInput;
    int selectedItem = 0;
    float xRotation = 0f;
    float yRotation = 0f;

    Rigidbody rb;
    int maxHealth;
    int currentHealth;

    public enum ItemID { Dirt, Grass, Stone, Wood, Sand, Leaves, Spawner, Sword, Steak, Rose, Dandelion, Cornflower, Cactus }

    void Awake()
    {
        instance = this;
        inputs = new Inputs();
        inputs.Player.Mouse.performed += context => cameraInput = context.ReadValue<Vector2>();
        inputs.Player.Mouse.canceled += context => cameraInput = context.ReadValue<Vector2>();
        inputs.Player.LeftClick.performed += ctx => Punch();
        inputs.Player.RightClick.performed += ctx => Build();
        inputs.Player.SwitchCam.performed += ctx => SwitchCamera();
        inputs.Player.Scroll.performed += ctx => ChangeItem(selectedItem + (int)Mathf.Clamp(ctx.ReadValue<Vector2>().y, -1, 1));
        //inputs.Player.RandomForce.performed += context => RandomForce();
        inputs.Player.Switch1.performed += ctx => ChangeItem(0);
        inputs.Player.Switch2.performed += ctx => ChangeItem(1);
        inputs.Player.Switch3.performed += ctx => ChangeItem(2);
        inputs.Player.Switch4.performed += ctx => ChangeItem(3);
        inputs.Player.Switch5.performed += ctx => ChangeItem(4);
        inputs.Player.Switch6.performed += ctx => ChangeItem(5);
        inputs.Player.Switch7.performed += ctx => ChangeItem(6);
        inputs.Player.Switch8.performed += ctx => ChangeItem(7);
        inputs.Player.Switch9.performed += ctx => ChangeItem(8);
        inputs.Player.Switch0.performed += ctx => ChangeItem(9);
        inputs.Player.DisplayInfo.performed += ctx => infoOverlay.SetActive(!infoOverlay.activeSelf);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovementV2>();
        maxHealth = lifeUI.Count;
        //currentHealth = maxHealth;
        cam.fieldOfView = 75;
        cam.transform.parent.localPosition = new Vector3(0, 1.8f, 0.15f);
        ChangeItem(0);
        InitInventoryBox();
        UpdateInventoryUI();
        ResetHealth(currentHealth);
        if(ES3.KeyExists("PlayerRotation", MainGameManager.instance.worldName + "/player.save"))
        {
            Vector2 rot = ES3.Load<Vector2>("PlayerRotation", MainGameManager.instance.worldName + "/player.save");
            xRotation = rot.x;
            yRotation = rot.y;
        }
    }

    private void OnEnable()
    {
        inputs.Enable();
    }

    private void OnDisable()
    {
        inputs.Disable();
    }

    private void Update()
    {

        float mouseX = cameraInput.x * mouseSensitivity;
        float mouseY = cameraInput.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        yRotation += mouseX;

        armature.localRotation = Quaternion.Euler(0, yRotation, 0);
        Quaternion eulerRotation = Quaternion.Euler(xRotation, yRotation, 0);
        cam.transform.parent.localRotation = eulerRotation;
        headFollow.localRotation = eulerRotation;

        if (!firstPerson)
        {
            head.localRotation = Quaternion.Euler(Mathf.Clamp(xRotation, -60f, 60f), 0, 0);
        }

        RaycastHit hitInfo;
        if (RaycastFromView(out hitInfo, buildLayer))
        {
            Vector3 pos = hitInfo.point + headFollow.forward * 0.005f;
            overlayCube.transform.position = new Vector3(Mathf.Floor(pos.x) + .5f, Mathf.Floor(pos.y) + .5f, Mathf.Floor(pos.z) + .5f);
            overlayCube.SetActive(true);
        }
        else
        {
            overlayCube.SetActive(false);
        }
    }

    public void ResetInventory()
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            inventory[i].item = null;
            inventory[i].number = 0;
        }
        AddItem(ItemID.Dirt, 32);
        inventory[1].item = MainGameManager.instance.allItem[(int)ItemID.Sword];
        inventory[1].number = 1;
        if(MainGameManager.instance.gamemode != MainGameManager.Gamemode.Immortal)
            AddItem(ItemID.Steak, 5);
        UpdateInventoryUI();
    }

    public void ResetHealth(int health = 20)
    {
        currentHealth = health;
        for (int i = 0; i < lifeUI.Count; i++)
        {
            lifeUI[i].SetActive(true);
        }
        for (int i = 0; i < maxHealth - currentHealth; i++)
        {
            lifeUI[i].SetActive(false);
        }
    }

    //What happen when you right click
    void Build()
    {
        if (inventory[selectedItem].item == null)
            return;

        if(inventory[selectedItem].item.isHeal && currentHealth < maxHealth) //If you have food in hand
        {
            handAnimator.SetTrigger("Hit");
            eatParticles.Play();
            AudioManager.instance.Play("Eat");
            Damage(-inventory[selectedItem].item.heal, Vector2.zero);
            inventory[selectedItem].number--;
            if (inventory[selectedItem].number == 0)
                inventory[selectedItem].item = null;
            UpdateInventoryUI();
        }
        else if(inventory[selectedItem].item.isBlock) // If you have a block in hand
        {
            RaycastHit hitInfo;
            //if (Physics.Raycast(headFollow.position, headFollow.forward, out hitInfo, maxDist, buildLayer))
            if (RaycastFromView(out hitInfo, buildLayer))
            {
                handAnimator.SetTrigger("Hit");
                BlockType targetBlock = TerrainGenerator.instance.AddBlock(hitInfo.point - headFollow.forward*0.01f, inventory[selectedItem].item.type); // A bit outside the block
                if (targetBlock == BlockType.Air || targetBlock == BlockType.Water || Chunk.IsFlowerOrWeed(targetBlock))
                {
                    inventory[selectedItem].number--;
                    if (inventory[selectedItem].number == 0)
                        inventory[selectedItem].item = null;
                    UpdateInventoryUI();
                }
            }
        }
    }

    void Punch()
    {
        if (isPunch)
            return;
        isPunch = true;
        handAnimator.SetTrigger("Hit");
        Item item = null;
        RaycastHit hitInfo;
        if (RaycastFromView(out hitInfo, hitLayer))
        {
            CreatureEntity creature = hitInfo.collider.GetComponent<CreatureEntity>();
            Grave grave = hitInfo.collider.GetComponent<Grave>();
            //If we hit a creature
            if (creature != null)
            {
                Instantiate(hitParticles, hitInfo.point, Quaternion.identity);
                if(inventory[selectedItem].item != null)
                    creature.Damage(inventory[selectedItem].item.damage, new Vector3(cam.transform.parent.forward.x, 0f, cam.transform.parent.forward.z) * playerMovement.knockBackForce);
                else
                    creature.Damage(1, new Vector3(cam.transform.parent.forward.x, 0f, cam.transform.parent.forward.z) * playerMovement.knockBackForce);
            }
            //If we hit the grave
            else if(grave != null)
            {
                grave.EmptyInventory();
            }
            else
            {
                BlockType targetBlock = TerrainGenerator.instance.AddBlock(hitInfo.point + headFollow.forward * 0.01f, BlockType.Air); // A bit inside the block
                if (targetBlock != BlockType.Spawner)
                    AddItem(BlockToItem(targetBlock), 1);
                else
                    LevelManager.instance.spawnerDestroyed++;
                item = MainGameManager.instance.allItem[(int)BlockToItem(targetBlock)];
            }
        }
        if (item == null || item.name == "Sword")
            StartCoroutine(ReloadHit(0.2f));
        else
            StartCoroutine(ReloadHit(item.reloadSpeed));

    }

    bool RaycastFromView(out RaycastHit hit, LayerMask layerMask)
    {
        if (firstPerson)
            return Physics.Raycast(cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)), out hit, maxDist, layerMask);
        else
            return Physics.Raycast(headFollow.position, headFollow.forward, out hit, maxDist, layerMask);


    }

    IEnumerator ReloadHit(float reloadTime)
    {
        reloadBar.SetActive(true);
        RectTransform bar = reloadBar.transform.GetChild(0).GetComponent<RectTransform>();
        bar.localScale = new Vector3(0, 1, 1);
        while(bar.localScale != Vector3.one)
        {
            float xscale = Mathf.MoveTowards(bar.localScale.x, 1, Time.deltaTime / reloadTime);
            yield return new WaitForSeconds(Time.deltaTime);
            bar.localScale = new Vector3(xscale, 1, 1);
        }
        reloadBar.SetActive(false);
        isPunch = false;
    }

    public void AddItem(ItemID itemID, int number)
    {
        for (int j = 0; j < number; j++)
        {
            AddOneItem(itemID);
        }
        UpdateInventoryUI();
    }

     void AddOneItem(ItemID itemID)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i].item != null && inventory[i].item.name == MainGameManager.instance.allItem[(int)itemID].name && inventory[i].item.maxStack > inventory[i].number)
            {
                inventory[i].number++;
                return;
            }
        }
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i].item == null)
            {
                if (itemID == ItemID.Sword)
                    return;
                inventory[i].item = MainGameManager.instance.allItem[(int)itemID];
                inventory[i].number++;
                return;
            }
        }
    }

    ItemID BlockToItem(BlockType blockType)
    {
        if (blockType == BlockType.Dirt)
            return ItemID.Dirt;
        else if (blockType == BlockType.Grass)
            return ItemID.Grass;
        else if (blockType == BlockType.Stone)
            return ItemID.Stone;
        else if (blockType == BlockType.Wood)
            return ItemID.Wood;
        else if (blockType == BlockType.Sand)
            return ItemID.Sand;
        else if (blockType == BlockType.Spawner)
            return ItemID.Spawner;
        else if (blockType == BlockType.Leaves)
            return ItemID.Leaves;
        else if (blockType == BlockType.Rose)
            return ItemID.Rose;
        else if (blockType == BlockType.Dandelion)
            return ItemID.Dandelion;
        else if (blockType == BlockType.Cornflower)
            return ItemID.Cornflower;
        else if (blockType == BlockType.Cactus)
            return ItemID.Cactus;
        else
            return ItemID.Sword;
    }

    void SwitchCamera()
    {
        if (firstPerson) //Switch to Third Person Camera
        {
            thirdCam.SetActive(true);
            skin.gameObject.SetActive(true);
            crossHair.SetActive(false);
            handHolding.gameObject.SetActive(false);
            firstPerson = false;
            headFollow.rotation = Quaternion.Euler(0, 0, 0);
        }
        else //Switch to First Person Camera
        {
            thirdCam.SetActive(false);
            skin.gameObject.SetActive(false);
            crossHair.SetActive(true);
            handHolding.gameObject.SetActive(true);
            firstPerson = true;
            cam.fieldOfView = 75;
            cam.transform.localPosition = Vector3.zero;
            headFollow.rotation = Quaternion.Euler(0, 0, 0);
        }
    }

    void ChangeItem(int nb)
    {
        Item i = inventory[selectedItem].item;
        GameObject item = itemUI[selectedItem];
        item.transform.GetChild(0).gameObject.SetActive(false);
        selectedItem = nb;
        if (selectedItem < 0)
            selectedItem = itemUI.Length - 1;
        if (selectedItem > itemUI.Length - 1)
            selectedItem = 0;
        item = itemUI[selectedItem];
        item.transform.GetChild(0).gameObject.SetActive(true);
        if (!(i == inventory[selectedItem].item))
            handAnimator.SetTrigger("Reset");
        UpdateItemInHand();
    }

    void UpdateItemInHand()
    {
        if (itemInHand != null)
            Destroy(itemInHand);
        if (inventory[selectedItem].item == null)
        {
            itemInHand = Instantiate(handMesh, handHolding);
        }
        else
        {
            itemInHand = Instantiate(inventory[selectedItem].item.mesh, handHolding);
            if (inventory[selectedItem].item.isBlock && !inventory[selectedItem].item.isNotCube)
            {
                ColorCube colorCube = itemInHand.GetComponent<ColorCube>();
                colorCube.ApplyUV(inventory[selectedItem].item.type);
            }
        }
    }

    public void UpdateInventoryUI()
    {
        //Update item bar
        for(int i = 0; i < itemUI.Length; i++)
        {
            UpdateOneInventoryUI(itemUI[i], i);
        }

        UpdateItemInHand();

        if (!LevelManager.instance.IsInventoryOpen())
            return;

        //Update inventory tab
        for (int i = 0; i < inventoryUI.Length; i++)
        {
            UpdateOneInventoryUI(inventoryUI[i], i);
        }

    }

    void UpdateOneInventoryUI(GameObject item, int i)
    {
        Transform icon = item.transform.GetChild(1);
        Transform number = item.transform.GetChild(2);
        if (inventory[i].item == null)
        {
            icon.gameObject.SetActive(false);
            number.gameObject.SetActive(false);
        }
        else
        {
            icon.gameObject.SetActive(true);
            icon.GetComponent<Image>().sprite = inventory[i].item.sprite;
            number.gameObject.SetActive(true);
            number.GetComponent<TextMeshProUGUI>().text = " " + inventory[i].number;
        }
    }

    public void InitInventoryBox()
    {
        for (int i = 0; i < inventoryUI.Length; i++)
        {
            inventoryUI[i].GetComponent<InventoryBox>().inventoryIndex = i;
        }
    }

    public void Damage(int d, Vector2 dir)
    {
        if (d<0)
        {
            StartCoroutine(WaveHealth());
            currentHealth = Mathf.Clamp(currentHealth - d, 0, maxHealth);
            for (int i = 0; i < lifeUI.Count; i++)
            {
                lifeUI[i].SetActive(true);
            }
        }
        else
        {
            if (isInvicible)
                return;
            skin.materials = new Material[] { MainGameManager.instance.playerMat, hitMat };
            isInvicible = true;
            currentHealth = Mathf.Clamp(currentHealth - d, 0, maxHealth);
            AudioManager.instance.Play("Hit");
            if (firstPerson)
                camShaker.Shake(shakePreset);
            else
                StartCoroutine(ShakeThirdCam(4f));
            rb.velocity = Vector3.zero;
            if (playerMovement.isGrounded && Mathf.Abs(rb.velocity.y) < 0.02f )
                rb.AddForce(dir.x * 5f, 3f, dir.y * 5f, ForceMode.Impulse);
            else
                rb.AddForce(dir.x * 5f, 1f, dir.y * 5f, ForceMode.Impulse);
            for (int i = 0; i < healthMovements.Length; i++)
            {
                healthMovements[i].HealthShake();
            }
            if (currentHealth > 0)
                StartCoroutine(ResetInvicible());
        }

        for (int i = 0; i < maxHealth-currentHealth; i++)
        {
            lifeUI[i].SetActive(false);
        }

        if (currentHealth == 0)
        {
            Die();
        }

    }

    IEnumerator WaveHealth()
    {
        for (int i = 0; i < healthMovements.Length; i++)
        {
            healthMovements[i].HealthWave();
            yield return new WaitForSeconds(0.03f);
        }
    }

    IEnumerator ShakeThirdCam(float intensity)
    {
        CinemachineBasicMultiChannelPerlin noise = thirdCamLook.GetRig(1).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_AmplitudeGain = intensity;
        yield return new WaitForSeconds(0.2f);
        noise.m_AmplitudeGain = 0;
    }

    IEnumerator ResetInvicible()
    {
        yield return new WaitForSeconds(0.22f);
        skin.materials = new Material[] { MainGameManager.instance.playerMat };
        yield return new WaitForSeconds(0.2f);
        isInvicible = false;
    }

    
    void RandomForce()
    {
        Damage(10, Vector2.zero);
    }

    void Die()
    {
        LevelManager.instance.SetDieScreen();
        playerMovement.animator.speed = 0f;
        //dieRotation.firstPerson = firstPerson;
        dieRotation.enabled = true;
        //playerMovement.enabled = false;
        //pushCollider.SetActive(false);
        this.enabled = false;

        //Instantiate Grave
        Vector3 pos = transform.position;
        int r = Random.Range(0, 2);
        Instantiate(gravePrefab, new Vector3(Mathf.Floor(pos.x) + .5f, Mathf.Floor(pos.y), Mathf.Floor(pos.z) + .5f), Quaternion.Euler(0, 90 * r, 0));
        Grave.instance.FillInventory();
    }

    private void OnDestroy()
    {
        SavePlayer();
    }

    private void OnApplicationQuit()
    {
        SavePlayer();
    }

    void SavePlayer()
    {
        ES3.Save("PlayerPosition", transform.position, MainGameManager.instance.worldName + "/player.save");
        ES3.Save("PlayerHealth", currentHealth, MainGameManager.instance.worldName + "/player.save");
        ES3.Save("PlayerInventory", inventory, MainGameManager.instance.worldName + "/player.save");
        ES3.Save("PlayerRotation", new Vector2(xRotation,yRotation), MainGameManager.instance.worldName + "/player.save");
    }
}
