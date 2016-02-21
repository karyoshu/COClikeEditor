using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class BuildingPlacer : MonoBehaviour {
    
    public List<GameObject> waterBodies;    //list of water bodies around the base
    public Button removeButton;     //to hold remove button

    public ScrollPanelItem currentSelectedPanelItem;    //variable to hold currently selected scroll panel item
    public bool hasPlaced;      //variable to hold if a building has been placed or not
    public bool pointerInScrollBar = false; //if pointer is in scroll bar

    private Transform currentBuilding;      //to hold currently selected building
    private Transform currentBuildingBoundingBox;       //to hold bounding box of current building
    private Camera mainCamera;      //to hold main camera
    private int floorMask;      //floormask where buildings are placed
    private int ceilingMask;    //ceiling mask where buildings are shown before placing, it is above floor mask
                                //(this is used so that buildings don't go into each other while we are moving them)

    private int buildingMask;   //building mask
    private float camRayLength = 100f;  //maximum cam ray length
    private Vector3 placePosition;  //position on floor mask where building is to be placed
    private Vector3 movePosition;   //position on ceiling mask where buildings are moved
    private static BuildingPlacer buildingPlacer = null;    
    
    public static BuildingPlacer GetInstance()
    {
        return buildingPlacer;
    }
    
    void Awake()
    {
        if (buildingPlacer == null)
        {
            buildingPlacer = GameObject.FindGameObjectWithTag("Manager").GetComponent<BuildingPlacer>();
        }
        else if(buildingPlacer != this)
        {
            Destroy(gameObject);
        }

        floorMask = LayerMask.GetMask("Floor");
        ceilingMask = LayerMask.GetMask("Ceiling");
        buildingMask = LayerMask.GetMask("Building");
        mainCamera = Camera.main;
        hasPlaced = true;
    }

    void Update()
    {
        if (currentBuilding != null && hasPlaced)
        {
            //if there is a selected building and it is placed then show it in white to mark it as selected
            currentBuilding.GetComponent<PlacableBuilding>().MarkWhite();
        }
        //show remov button if there is any building selected
        ShowRemoveButton();

        //if there is a building selected which is not yet placed
        if (currentBuilding != null && !hasPlaced)
        {
            //update placeposition and moveposition by casting a ray from camera to touchposition and hitting floormask and ceiling mask respectively
            placePosition = GetPointerPositionOnMask(floorMask);
            movePosition = GetPointerPositionOnMask(ceilingMask);

            //round of the positions, so as to give effect of incremental block movement (0.12 is added to adjust for ceiling mask height)
            placePosition = new Vector3(Mathf.Round(placePosition.x + 0.12f), Mathf.Round(placePosition.y), Mathf.Round(placePosition.z + 0.12f));
            movePosition = new Vector3(Mathf.Round(movePosition.x) + 0.12f, Mathf.Round(movePosition.y), Mathf.Round(movePosition.z) + 0.12f);

            //mark current building as red, if a legal position is found, then it will be marked as green and if placed then marked as grey
            currentBuilding.GetComponent<PlacableBuilding>().MarkRed();
            currentBuilding.position = movePosition;
            currentBuildingBoundingBox.position = placePosition;
            if (IsPlacablePosition())
            {
                currentBuilding.GetComponent<PlacableBuilding>().MarkGreen();

                //when mouse button up or touch up event is found
                if (Input.GetMouseButtonUp(0))
                {
                    PlaceBuilding();
                }
            }
            else
            {
                //if not a legal position then return the building to its old position or in the scroll panel
                if (Input.GetMouseButtonUp(0))
                {
                    if (currentBuilding.GetComponent<PlacableBuilding>().alreadyOnScreen)
                    {
                        placePosition = currentBuilding.GetComponent<PlacableBuilding>().lastPosition;
                        PlaceBuilding();
                    }
                    else
                    {
                        RemoveBuilding(currentBuilding.gameObject);
                    }
                }
            }
        }
        //if no building is selected or it is already placed
        else
        {
            //if not zooming in or out
            if (Input.touchCount < 2)
            {
                //if mouse clicked or screen touched
                if (Input.GetMouseButtonDown(0))
                {
                    //cast a ray from camera to mouseposition, if hits a building, select that building for replacement
                    Ray camRay = mainCamera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit maskHit;
                    if (Physics.Raycast(camRay, out maskHit, camRayLength, buildingMask))
                    {
                        //if ray hits building
                        if (maskHit.collider.transform.parent == null)
                            currentBuilding = maskHit.collider.transform;
                        else
                            //if ray hits a child object
                            currentBuilding = maskHit.collider.transform.parent;
                        hasPlaced = false;
                        //create a bounding box to check for legal positions
                        currentBuildingBoundingBox = Instantiate(currentBuilding.GetComponent<PlacableBuilding>().buildingBoundingBox).transform;
                        //remove this building from building managers list
                        BuildingManager.GetInstance().RemoveFromBuildingList(currentBuilding.gameObject);
                    }
                }
            }
        }

        //if touch ends or mouse button up is detected then there is no selected control panel item
        if (Input.GetMouseButtonUp(0))
        {
            currentSelectedPanelItem = null;
        }

        //if there are no current buildings or it has been placed and there is no scroll panel item selected then camera can be moved
        if ((currentBuilding == null || hasPlaced) && currentSelectedPanelItem == null)
             Camera.main.GetComponent<CameraControls>().moveCamera = true;
        else
            Camera.main.GetComponent<CameraControls>().moveCamera = false;
    }

    public void PlaceBuilding()
    {
        //places a building on the base, destroys its place holder bounding box as building has a bounding box on itself, mark it as grey and assign its lastposition to current position
        hasPlaced = true;
        currentBuilding.position = placePosition;
        BuildingManager.GetInstance().AddToBuildingList(currentBuilding.gameObject);
        currentBuilding.GetComponent<PlacableBuilding>().MarkGrey();
        currentBuilding.GetComponent<PlacableBuilding>().lastPosition = placePosition;
        currentBuilding.GetComponent<PlacableBuilding>().alreadyOnScreen = true;
        Destroy(currentBuildingBoundingBox.gameObject);
    }


    public void RemoveBuilding()
    {
        RemoveBuilding(currentBuilding.gameObject);
    }

    public void RemoveBuilding(GameObject building)
    {
        //removes a building from the base, and adds it to the scroll panel, destroys building, and its bounding box if it is not already destroyed
        foreach (Button button in BuildingManager.GetInstance().GetButtonsList())
        {
            if (building.GetComponent<PlacableBuilding>().parentScrollPanelItem == button.gameObject)
            {
                hasPlaced = true;
                button.GetComponent<ScrollPanelItem>().AddBuilding();
                BuildingManager.GetInstance().RemoveFromBuildingList(building);
                Destroy(building);
                if(currentBuildingBoundingBox != null)
                    Destroy(currentBuildingBoundingBox.gameObject);
                return;
            }
        }
    }

    private bool IsPlacablePosition()
    {
        //check if current postion is legal or not, this is where place holder bounding box comes in action
        //as building is moved above the actual base to prevent colliding, a bounding box was needed, which was actually invisible but collided with the building on the base
        foreach (GameObject building in BuildingManager.GetInstance().GetBuildingsList())
        {
            if (building.GetComponent<BoxCollider>().bounds.Intersects(currentBuildingBoundingBox.GetComponent<BoxCollider>().bounds))
            {
                return false;
            }
        }
        //and water bodies
        foreach (GameObject waterbody in waterBodies)
        {
            if (waterbody.GetComponent<BoxCollider>().bounds.Intersects(currentBuildingBoundingBox.GetComponent<BoxCollider>().bounds))
            {
                return false;
            }
        }
        return true;
    }

    public void SetCurrentBuilding(GameObject building, GameObject parentScrollPanelItem)
    {
        if (hasPlaced)
        {
            hasPlaced = false;
            //instantiate a building and assign its parent scroll bar item
            currentBuilding = Instantiate(building).transform;
            currentBuilding.GetComponent<PlacableBuilding>().parentScrollPanelItem = parentScrollPanelItem;
            //instantiate a bounding box that will be needed for finding a placable position
            currentBuildingBoundingBox = Instantiate(currentBuilding.GetComponent<PlacableBuilding>().buildingBoundingBox).transform;
        }
    }

    Vector3 GetPointerPositionOnMask(int layerMask)
    {
        Ray camRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit maskHit;
        Physics.Raycast(camRay, out maskHit, camRayLength, layerMask);
        return maskHit.point;
    }
    

    void ShowRemoveButton()
    {
        if (hasPlaced == true && currentBuilding != null)
            removeButton.GetComponent<Image>().enabled = true;
        else
            removeButton.GetComponent<Image>().enabled = false;
    }

    public Transform getCurrentBuilding()
    {
        return currentBuilding;
    }
}
