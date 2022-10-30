using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class WorldInfo : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector]
    public WorldMenu worldMenu;
    [HideInInspector]
    public Image select;
    public TextMeshProUGUI worldName;
    public TextMeshProUGUI infoText;

    private void Start()
    {
        select = GetComponent<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        worldMenu.SelectWorld(select, worldName.text);
    }

    public void SetText(string name, string info)
    {
        worldName.text = name;
        infoText.text = info;
    }
}
