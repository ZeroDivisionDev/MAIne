using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grave : MonoBehaviour
{
    public static Grave instance;
    public GameObject gravePartciles;
    public ItemInventory[] graveInventory;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(instance.gameObject);
        }
        instance = this;
        graveInventory = new ItemInventory[PlayerController.instance.inventory.Length];
    }

    public void FillInventory()
    {
        for (int i = 0; i < graveInventory.Length; i++)
        {
            graveInventory[i] = new ItemInventory();
            graveInventory[i].item = PlayerController.instance.inventory[i].item;
            graveInventory[i].number = PlayerController.instance.inventory[i].number;
        }
        ES3.Save("GraveTransform", transform, MainGameManager.instance.worldName + "/player.save");
        ES3.Save("GraveInventory", graveInventory, MainGameManager.instance.worldName + "/player.save");
    }

    public void EmptyInventory()
    {
        for (int i = 0; i < graveInventory.Length; i++)
        {
            if(graveInventory[i].item != null)
                PlayerController.instance.AddItem(graveInventory[i].item.id, graveInventory[i].number);
        }
        int r = Random.Range(1, 3);
        AudioManager.instance.Play("Stone" + r);
        AudioManager.instance.Play("Spirit");
        Instantiate(gravePartciles, transform.position + Vector3.up * .5f, Quaternion.identity);
        Destroy(gameObject);
        ES3.DeleteKey("GraveTransform", MainGameManager.instance.worldName + "/player.save");
        ES3.DeleteKey("GraveInventory", MainGameManager.instance.worldName + "/player.save");
    }

}
