using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ScrollPanelItem : MonoBehaviour, IPointerDownHandler {

    private int buildingCount = 0;  //no of buildings of this type in scroll panel
    public GameObject buildingPrefab;   //prefab of building that will instatiate once it is removed from the scroll panel
    public bool isVisible = true;   //if this scroll panel item is visible or not
    
	// Update is called once per frame
	void Update () {
        //set scroll panel item visibility on the basis of isVisible
        GetComponent<Image>().enabled = isVisible;
        GetComponentInChildren<Text>().enabled = isVisible;
        //update text on scroll panel item to show building count
        GetComponentInChildren<Text>().text = "x" + buildingCount;
        //update isVisible on the basis of building count
        if (buildingCount < 1)
        {
            isVisible = false;
        }
        else
        {
            isVisible = true;
        }
    }

    public void AddBuilding()
    {
        buildingCount++;
    }

    public void RemoveBuilding()
    {
        buildingCount--;
        //remove building from scroll panel item but add to BuildigPlacer
        BuildingPlacer.GetInstance().SetCurrentBuilding(buildingPrefab, gameObject);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        //handle pointer down event on scroll panel item
        BuildingPlacer.GetInstance().currentSelectedPanelItem = this;
    }
}
