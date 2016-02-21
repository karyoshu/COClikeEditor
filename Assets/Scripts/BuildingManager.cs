using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuildingManager : MonoBehaviour {

    public List<Button> scrollPanelItems = new List<Button>();    //list of scroll panel button

    private List<GameObject> buildingsOnScreen = new List<GameObject>();    //list of all the buildings placed on the base
    private static BuildingManager buildingManager = null;  //static instance of building manager so as to have only one instance at a time

    public static BuildingManager GetInstance()
    {
        return buildingManager;
    }

    void Awake()
    {
        if (buildingManager == null)
        {
            buildingManager = GameObject.FindGameObjectWithTag("Manager").GetComponent<BuildingManager>();
        }
        else if (buildingManager != this)
        {
            Destroy(gameObject);
        }
    }

    public void AddToBuildingList(GameObject building)
    {
        buildingsOnScreen.Add(building);
    }

    public List<GameObject> GetBuildingsList()
    {
        return buildingsOnScreen;
    }

    public void RemoveFromBuildingList(GameObject building)
    {
        buildingsOnScreen.Remove(building);
    }

    public List<Button> GetButtonsList()
    {
        return scrollPanelItems;
    }
}
