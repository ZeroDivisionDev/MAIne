using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoOverlay : MonoBehaviour
{
    public static InfoOverlay instance;
    public TextMeshProUGUI infoText;
    public int nbChunk;
    public int nbEntity;

    private void Awake()
    {
        instance = this;
        InvokeRepeating("UpdateInfo", 0f, 0.1f);
    }

    /*
    // Update is called once per frame
    void Update()
    {
        if (gameObject.activeSelf)
        {
            infoText.text = "X : " + (int)PlayerController.instance.transform.position.x + "    Y : " + (int)PlayerController.instance.transform.position.y + "    Z : " + (int)PlayerController.instance.transform.position.z + "\n" +
                            "Chunks : " + nbChunk + "   Entities : " + nbEntity + "\n" +
                            "FPS : " + (int)(1/Time.deltaTime);
        }
    }*/

    void UpdateInfo()
    {
        if (gameObject.activeSelf)
        {
            infoText.text = "X : " + (int)PlayerController.instance.transform.position.x + "    Y : " + (int)PlayerController.instance.transform.position.y + "    Z : " + (int)PlayerController.instance.transform.position.z + "\n" +
                            "Chunks : " + nbChunk + "   Entities : " + nbEntity + "\n" +
                            "FPS : " + (int)(1 / Time.deltaTime);
        }
    }
}
