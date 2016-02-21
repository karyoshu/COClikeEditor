using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ScrollPanel : MonoBehaviour,  IPointerExitHandler, IPointerEnterHandler{
    public RectTransform scrollPanel;   //hold the scroll panel that contains all the scroll panel items
    public RectTransform leftBoundary;  //left boundary of the scroll panel
    public RectTransform rightBoundary; //right bondary of the scroll panel

    public bool scrollable = true;  //if panel is scrollable

    private List<Button> scrollPanelItems;   //list of all the scroll panel items
    private int visibleButtonCount; //count of visible buttons
    private bool dragging = false; //will be true, while we drag the panel
    private float buttonDistance = 150f; //will hold the distance between the buttons

    void Start()
    {
        //get scroll panel items from the building manager
        scrollPanelItems = BuildingManager.GetInstance().GetButtonsList();

        //add 3 building count to each scroll panel item(ideally we would get this data from a datasource but for the sake of this project, i am hardcoding it)
        for (int i = 0; i < scrollPanelItems.Count; i++)
        {
            scrollPanelItems[i].GetComponent<ScrollPanelItem>().AddBuilding();
            scrollPanelItems[i].GetComponent<ScrollPanelItem>().AddBuilding();
            scrollPanelItems[i].GetComponent<ScrollPanelItem>().AddBuilding();
        }
    }

    void Update()
    {
        UpdateList();
        UpdateVisibleButtonCount();

        //if buttons exceed the screen or pointer is not inside scroll bar then stop scrolling
        if ((visibleButtonCount-1) * buttonDistance < (rightBoundary.anchoredPosition.x - leftBoundary.anchoredPosition.x)  
            || !BuildingPlacer.GetInstance().pointerInScrollBar)
            scrollable = false;
        else
            scrollable = true;


        if (scrollable)
        {
            scrollPanel.parent.GetComponent<ScrollRect>().horizontal = true;
            if (!dragging)
            {
                //if not dragging, lerp the scroll bar to appropriate position based on the boundaries
                //if scroll bar dragged such that first scroll bar item crossed left boundary then lerp it back
                if (scrollPanelItems[0].transform.position.x > leftBoundary.transform.position.x)
                {
                    scrollPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Lerp(scrollPanel.GetComponent<RectTransform>().anchoredPosition.x, leftBoundary.GetComponent<RectTransform>().anchoredPosition.x, Time.deltaTime * 10f), scrollPanel.GetComponent<RectTransform>().anchoredPosition.y);
                }
                //if scroll bar dragged such that last visible scroll bar item crossed right boundary then lerp it forward
                if (scrollPanelItems[visibleButtonCount - 1].transform.position.x < rightBoundary.transform.position.x)
                {
                    scrollPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Lerp(scrollPanel.GetComponent<RectTransform>().anchoredPosition.x, rightBoundary.GetComponent<RectTransform>().anchoredPosition.x - buttonDistance * (visibleButtonCount - 1), Time.deltaTime * 10f), scrollPanel.GetComponent<RectTransform>().anchoredPosition.y);
                }
            }
        }
        else
        {
            scrollPanel.parent.GetComponent<ScrollRect>().horizontal = false;
            if (scrollPanelItems.Count != 0)
            {
                //if not scrollable and still scroll panel items visible then lerp scroll bar such that first scroll panel item is at start of scroll panel
                if (scrollPanelItems[0].transform.position.x > leftBoundary.transform.position.x)
                {
                    scrollPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Lerp(scrollPanel.GetComponent<RectTransform>().anchoredPosition.x, leftBoundary.GetComponent<RectTransform>().anchoredPosition.x, Time.deltaTime * 7f), scrollPanel.GetComponent<RectTransform>().anchoredPosition.y);
                }
                if (scrollPanelItems[0].transform.position.x < leftBoundary.transform.position.x)
                {
                    scrollPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Lerp(scrollPanel.GetComponent<RectTransform>().anchoredPosition.x, leftBoundary.GetComponent<RectTransform>().anchoredPosition.x, Time.deltaTime * 7f), scrollPanel.GetComponent<RectTransform>().anchoredPosition.y);
                }
            }
        }
    }

    public void StartDrag()
    {
        dragging = true;
    }

    public void StopDrag()
    {
        dragging = false;
    }
    
    public void UpdateList()
    {
        //updates the scroll panel list such that all the visible scroll panel items are in front of scroll panel and invisible later
        for (int i = 0; i < scrollPanelItems.Count - 1; i++)
        {
            if (!scrollPanelItems[i].GetComponent<ScrollPanelItem>().isVisible)
            {
                for (int j = i + 1; j < scrollPanelItems.Count; j++)
                {
                    if (scrollPanelItems[j].GetComponent<ScrollPanelItem>().isVisible)
                    {
                        SwapListItem(scrollPanelItems, i, j);
                        break;
                    }
                }
                foreach (Button newButton in scrollPanelItems)
                {
                    newButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(buttonDistance * scrollPanelItems.IndexOf(newButton), 0);
                }
            }
        }
    }

    void UpdateVisibleButtonCount()
    {
        //updates the visible button count
        int count = 0;
        for (int i = 0; i < scrollPanelItems.Count; i++)
        {
            if (scrollPanelItems[i].GetComponent<ScrollPanelItem>().isVisible)
            {
                count++;
            }
        }
        visibleButtonCount = count;
    }

    public void SwapListItem(List<Button> list, int firstItemPosition, int secondItemPosition)
    {
        Button tempButton = list[firstItemPosition];
        list[firstItemPosition] = list[secondItemPosition];
        list[secondItemPosition] = tempButton;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //handle on pinter exit event
        if (BuildingPlacer.GetInstance().currentSelectedPanelItem != null && BuildingPlacer.GetInstance().hasPlaced)
        {
            BuildingPlacer.GetInstance().currentSelectedPanelItem.RemoveBuilding(); //remove a building from currently selected scroll panel item
            BuildingPlacer.GetInstance().currentSelectedPanelItem = null;
            BuildingPlacer.GetInstance().pointerInScrollBar = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        BuildingPlacer.GetInstance().pointerInScrollBar = true;
    }
}
