using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public class CameraControls : MonoBehaviour{


    public float minCameraZoom = 4f;    //minimum camera zoom
    public float maxCameraZoom = 15f;   //maximum camera zomm
    public Vector2 minCameraPos;    //minimum camera position
    public Vector2 maxCameraPos;    //maximum camera position
    public Vector2 moveSensitivity = new Vector2(1f, 1f);   //camera move sensitivity
    public bool updateZoomSensitivity = true;   //update movesensitivity on the basis of camera zoom
    public float zoomSpeed = 0.5f;  //camera zoom speed
    public bool moveCamera = true;  //if camera can be moved, that is no building is being placed, its valur is set by BuildingPlacer script

    private Camera mainCamera;  //to store main camera

    
    void Start()
    {
        //get main camera
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (moveCamera)
        {
            if (updateZoomSensitivity)
            {
                //updating move sensitivity on the basis of camera zoom
                moveSensitivity = new Vector2(mainCamera.orthographicSize / 4, mainCamera.orthographicSize / 4);
            }
            
            //get all the touch inputs
            Touch[] touches = Input.touches;

            if (touches.Length > 0)
            {
                //get camera move on the basis of 1 touch
                if (touches.Length == 1)
                {
                    if (touches[0].phase == TouchPhase.Moved)
                    {
                        Vector2 delta = touches[0].deltaPosition;
                        //get new position delta from touch delta and move sensitivity
                        //(also inverse that as camera moves in opposite direction of touch to give the effect of world movement)
                        Vector2 newPositionDelta = new Vector2(delta.x * moveSensitivity.x * Time.deltaTime * -1, delta.y * moveSensitivity.y * Time.deltaTime * -1);
                        //add delta to camera's local position(so as to ignore the  camera rotations)
                        transform.localPosition += new Vector3(newPositionDelta.x, newPositionDelta.y, 0);

                        transform.localPosition = new Vector3(Mathf.Clamp(transform.localPosition.x, minCameraPos.x, maxCameraPos.x), 
                            Mathf.Clamp(transform.localPosition.y, minCameraPos.y, maxCameraPos.y), transform.localPosition.z);
                    }
                }

                //get camera zoom on the basis of 2 touches
                if (touches.Length == 2)
                {
                    Touch touchZero = touches[0];
                    Touch touchOne = touches[1];

                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
                    //calculate how far/close touches have moved to update the zoom according to that
                    float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                    mainCamera.orthographicSize += deltaMagnitudeDiff * zoomSpeed;
                    //clamp the zoom between min zoom and max zoom
                    mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize, minCameraZoom, maxCameraZoom);
                }
            }
        }
    }
}
