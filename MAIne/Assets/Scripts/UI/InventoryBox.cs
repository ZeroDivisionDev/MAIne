using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryBox : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image background;
    public Color normalColor;
    public Color hoverColor;

    public int inventoryIndex;

    private void OnEnable()
    {
        background.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        FollowMouse.instance.ManageItem(inventoryIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(PlayerController.instance.inventory[inventoryIndex].item != null)
            FollowMouse.instance.infoText.text = PlayerController.instance.inventory[inventoryIndex].item.name;
        background.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        FollowMouse.instance.infoText.text = "";
        background.color = normalColor;
    }
}
