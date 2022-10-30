using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FollowMouse : MonoBehaviour
{

    public static FollowMouse instance;

    public Image itemImage;
    public TextMeshProUGUI itemNumber;
    public TextMeshProUGUI infoText;

    public ItemInventory item;

    int previousIndex;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        item = new ItemInventory();
        previousIndex = -1;
    }

    private void OnEnable()
    {
        infoText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = LevelManager.instance.mousePos;
    }

    public void UpdateUI()
    {
        if (item.item == null)
        {
            itemImage.sprite = null;
            itemImage.color = new Color(1,1,1,0);
            itemNumber.text = "";
        }
        else
        {
            itemImage.sprite = item.item.sprite;
            itemImage.color = new Color(1, 1, 1, 1);
            itemNumber.text = " " + item.number;
        }
    }

    public void ManageItem(int index)
    {
        ItemInventory itemInventory = PlayerController.instance.inventory[index];

        Item temp = itemInventory.item;
        int tempNum = itemInventory.number;
        itemInventory.item = item.item;
        itemInventory.number = item.number;
        item.item = temp;
        item.number = tempNum;
        previousIndex = index;

        UpdateUI();
        PlayerController.instance.UpdateInventoryUI();
    }

    public void ResetItem()
    {
        if (item.item == null)
            return;
        if(PlayerController.instance.inventory[previousIndex].item == null)
        {
            ManageItem(previousIndex);
        }
        else
        {
            PlayerController.instance.AddItem(item.item.id, item.number);
            item.item = null;
            item.number = 0;
        }
        UpdateUI();
    }

    public void RemoveItem()
    {
        if (item == null || item.item == null)
            return;

        if(item.item.id == PlayerController.ItemID.Sword)
        {
            infoText.text = "You can't remove your sword";
            return;
        }

        if(item.item != null)
        {
            item.item = null;
            item.number = 0;
            int r = Random.Range(1, 4);
            AudioManager.instance.Play("Trash" + r);

            UpdateUI();

        }
    }
}
