using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    // stores the item details
    public Item itemDetails;

    public GameObject player;

    private Transform originalParent;
    private Inventory localInventory;

    public void Start()
    {
        localInventory = player.GetComponent<Inventory>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(transform.parent.parent);
        transform.position = eventData.position;
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    { 
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // transform.SetParent(originalParent);
        // transform.position = originalParent.position;
        GameObject newParent = eventData.pointerCurrentRaycast.gameObject;
        // if this is a slot
        if (localInventory.slots.Contains(newParent))
        {
            // add the item to the slot
            transform.SetParent(newParent.transform);
            transform.position = newParent.transform.position;
        }
        else
        {
            // drops the item
            localInventory.RemoveItem(transform.gameObject);
        }
        // unblock raycasts
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}
