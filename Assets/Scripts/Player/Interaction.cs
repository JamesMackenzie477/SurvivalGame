using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Interaction : NetworkBehaviour {

    // globals containers
    private ItemDatabase database;
    private Text playerNotification;
    private Inventory inventory;
    private Camera m_Camera;
    private RaycastHit hit;

    // Use this for initialization
    private void Start () {
        // gets the notification text to show to player
        playerNotification = GameObject.Find("HUD").transform.Find("Notification").GetComponent<Text>();
        // gets the item database
        database = GameObject.Find("Scripts").GetComponent<ItemDatabase>();
        // gets inventory to add items
        inventory = GetComponent<Inventory>();
        // gets the camera for raycasts
        m_Camera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    private void Update()
    {
        // allows player to interact with items locally
        LocalItemInteraction();
    }

    // allows player to interact with items locally
    private void LocalItemInteraction()
    {
        // only is we are the local player
        if (!isLocalPlayer) return;
        // casts ray cast 2 meters infront of player
        if (Physics.Raycast(transform.position, m_Camera.transform.forward, out hit, 2))
        {
            // gets the item info of the item
            ItemId itemInfo = hit.transform.GetComponentInParent<ItemId>();
            // if there is item info that means we can pick it up
            if (itemInfo != null)
            {
                // shows notification on screen
                playerNotification.enabled = true;
                // edits text to corrospond to item
                playerNotification.text = string.Format("Press F to pick up {0}x {1}(s)", itemInfo.itemAmount, database.GetItemById(itemInfo.itemId).itemName);
                // if player press f (tries to pick up item)
                if (Input.GetKeyDown(KeyCode.F))
                {
                    // notify the server
                    CmdPickUpItem();
                }
            }
        }
        else
        {
            // else we disable the notification
            playerNotification.enabled = false;
        }
    }

    [Command] // called on server
    private void CmdPickUpItem()
    {
        // casts ray cast 2 meters infront of player
        if (Physics.Raycast(transform.position, m_Camera.transform.forward, out hit, 2))
        {
            // gets the item info of the item
            ItemId itemInfo = hit.transform.GetComponentInParent<ItemId>();
            // if there is item info that means we can pick it up
            if (itemInfo != null)
            {
                // adds the item to the server inventory
                // this will then be synced with the client
                inventory.AddItem(itemInfo.itemId);
                // deletes the item for all clients
                Destroy(hit.transform.gameObject);
            }
        }
    }
}
