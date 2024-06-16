using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Inventory : NetworkBehaviour {

    [SerializeField] private GameObject inventorySlot;
    [SerializeField] private GameObject inventoryItem;
    private GameObject inventoryPanel;
    private GameObject inventorySlots;

    // a list of inventory slots to be assigned
    public List<GameObject> slots = new List<GameObject>();

    public List<GameObject> inventory = new List<GameObject>();

    // a dictonary synced with clients that contains the slot as a key and the item as a value
    private Dictionary<int, Item> playerInventory = new Dictionary<int, Item>();

    private Dictionary<int, int> playerHotbar = new Dictionary<int, int>();

    // a database of obtainable items
    private ItemDatabase database;

    private void Start()
    {
        // gets the player inventory
        inventoryPanel = GameObject.Find("Inventory");
        // gets the player inventory slots
        inventorySlots = inventoryPanel.transform.Find("Slots").gameObject;
        // gets the item database
        database = GameObject.Find("Scripts").GetComponent<ItemDatabase>();

        // --------------- DEBUG ---------------
        AddItem(0);
        AddItem(0);
        AddItem(0);
        // -------------------------------------

        // only is we are the local player
        if (!isLocalPlayer) return;
        // this will allocate the inventory slots locally
        AllocateInventorySlots(16);
        // hide the inventory
        inventoryPanel.SetActive(false);
    }

    private void Update()
    {
        // if the user presses tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // toggle inventory on/off
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
            if (inventoryPanel.activeSelf)
            {
                // hides mouse
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                // hides mouse
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    // this will dynamically allocate the slots to populate the inventory
    private void AllocateInventorySlots(int amount)
    {
        // iterates through slots
        for (int i = 0; i < amount; i++)
        {
            // spawns slot in inventory
            slots.Add(Instantiate(inventorySlot, inventorySlots.transform));
        }
    }

    // this will add an item to the inventory by the id of the item
    public void AddItem(int itemId)
    {
        // only is we are the server
        if (!isServer) return;
        // gets the item from the database
        Item newItem = database.GetItemById(itemId);
        // if the item is stackable
        if (newItem.canStack)
        {
            // iterate through inventory
            foreach (var item in playerInventory)
            {
                // if we have that item in our inventory
                if (item.Value.itemId == itemId)
                {
                    // if the item amount is less then 10
                    if (item.Value.itemAmount < 10)
                    {
                        // increment the amount
                        item.Value.itemAmount += 1;
                        // stacks the item on the client
                        RpcStackItem(item.Key);
                        // returns
                        return;
                    }
                }
            }
        }
        // iterate through slots
        for (int i = 0; i < 16; i++)
        {
            // if the slot is not taken
            if (!playerInventory.ContainsKey(i))
            {
                // add item to server inventory
                playerInventory.Add(i, newItem);
                // tells the client to add the item
                RpcAddItem(i, itemId);
                // breaks out of loop
                break;
            }
        }
    }

    public void RemoveItem(GameObject itemSlot)
    {
        // only is we are the client
        if (!isClient) return;
        // gets the item details
        Item newItem = itemSlot.GetComponent<ItemSlot>().itemDetails;
        // destroys the item
        Destroy(itemSlot);
        // tells the server to remove the item
        CmdRemoveItem(newItem.itemId, newItem.itemAmount);
    }

    private void RefreshLocalInventory()
    {
        // iterates through inventory items
        foreach (var item in playerInventory)
        {
            // spawns item image
            GameObject slot = Instantiate(inventoryItem, slots[item.Key].transform);
            //slot.GetComponent<Image>().sprite = item.Value.itemIcon;
        }
    }

    [Command] // called on server
    private void CmdRemoveItem(int itemId, int amount)
    {
        // only is we are the server
        if (!isServer) return;
        // gets the item from the database
        Item item = database.GetItemById(itemId);

        Vector3 playerPos = transform.position;
        Vector3 playerDirection = GetComponentInChildren<Camera>().transform.forward;
        Vector3 spawnPos = playerPos + playerDirection * 1;

        // spawns the item
        GameObject newItem = Instantiate(item.itemPrefab, spawnPos, transform.rotation);
        // alters the item details
        newItem.GetComponent<ItemId>().itemAmount = amount;
        // spawns for all players
        NetworkServer.Spawn(newItem);

        foreach (var inventoryItem in playerInventory)
        {
            if (inventoryItem.Value.itemId == itemId)
            {
                playerInventory.Remove(inventoryItem.Key);
            }
        }
    }

    [ClientRpc] // called on all clients
    private void RpcAddItem(int index, int itemId)
    {
        // only if we are the local player
        if (!isLocalPlayer) return;
        // extracts item from database
        Item item = database.GetItemById(itemId);
        // adds item to inventory dictonary
        playerInventory.Add(index, database.GetItemById(itemId));
        // spawns item image
        GameObject itemImage = Instantiate(inventoryItem, slots[index].transform);

        itemImage.GetComponent<ItemSlot>().itemDetails = database.GetItemById(itemId);
        itemImage.GetComponent<ItemSlot>().player = transform.gameObject;
        inventory.Add(itemImage);

        // sets the item icon
        itemImage.GetComponent<Image>().sprite = item.itemIcon;
        // sets the item name
        itemImage.name = item.itemName;
    }

    [ClientRpc] // called on all clients
    private void RpcStackItem(int index)
    {
        // only if we are the local player
        if (!isLocalPlayer) return;
        // increments the amount of the item
        playerInventory[index].itemAmount += 1;
        // updates player inventory gui
        slots[index].transform.Find(playerInventory[index].itemName).GetComponentInChildren<Text>().text = playerInventory[index].itemAmount.ToString();
    }
}