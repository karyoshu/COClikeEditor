using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlacableBuilding : MonoBehaviour {

    public Material redMaterial;    
    public Material greenMaterial;
    public Material greyMaterial;
    public Material whiteMaterial;
    public GameObject parentScrollPanelItem;    //scroll panel item from which this building instantiated

    public GameObject buildingBoundingBox;  //a gameobject having bounding box similar to this building
    public Vector3 lastPosition;    //last position of the building
    public bool alreadyOnScreen = false;    //if the building was already on screen before current operation started
        
    public void MarkRed()
    {
        GetColorPlate().GetComponent<MeshRenderer>().material = redMaterial;
    }

    public void MarkGreen()
    {
        GetColorPlate().GetComponent<MeshRenderer>().material = greenMaterial;
    }

    public void MarkGrey()
    {
        GetColorPlate().GetComponent<MeshRenderer>().material = greyMaterial;
    }

    public void MarkWhite()
    {
        GetColorPlate().GetComponent<MeshRenderer>().material = whiteMaterial;
    }

    Transform GetColorPlate()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "ColorPlate")
            {
                return child;
            }
        }
        return null;
    }

    void Update()
    {
        //if this building is not current building in building placer then turn this building to grey
        if (BuildingPlacer.GetInstance().getCurrentBuilding() != this.transform)
        {
            MarkGrey();
        }
    }
}
