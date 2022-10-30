using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bed : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    public void OnPointerClick(PointerEventData eventData)
    {
        LevelManager.instance.SetSpawnPoint();
        AudioManager.instance.Play("BedUI");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        FollowMouse.instance.infoText.text = "Set spawn point here";
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        FollowMouse.instance.infoText.text = "";
    }
}
