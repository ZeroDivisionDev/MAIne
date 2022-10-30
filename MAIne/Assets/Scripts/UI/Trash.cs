using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Trash : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    public void OnPointerClick(PointerEventData eventData)
    {
        FollowMouse.instance.RemoveItem();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        FollowMouse.instance.infoText.text = "Remove item";
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        FollowMouse.instance.infoText.text = "";
    }
}
