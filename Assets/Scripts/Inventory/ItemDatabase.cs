using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour {

    // the key is the id of the item
    private List<Item> itemDatabase = new List<Item>();

	// Use this for initialization
	private void Start () {
        // TEMP: adds hammer to the database
        itemDatabase.Add(new Item(0, "Hammer", "Build your way to heaven!", 1, "HammerIcon", true, 100, "hammer"));
	}
	
    // returns item matching id
	public Item GetItemById(int id)
    {
        // iterates through items
        foreach (Item item in itemDatabase)
        {
            // if the id matches
            if (id == item.itemId)
            {
                // returns the item
                return item;
            }
        }
        // if no item is found return null
        return null;
    }
}

// a class containing information about a specific item
public class Item
{
    public int itemId;
    public string itemName;
    public string itemDescription;
    public int itemAmount;
    public Sprite itemIcon;
    public GameObject itemPrefab;
    public bool canStack;
    public float itemHealth;

    public Item(int id, string name, string description, int amount, string iconSlug, bool stackable, float health, string prefabSlug)
    {
        itemId = id;
        itemName = name;
        itemDescription = description;
        itemAmount = amount;
        itemIcon = Resources.Load<Sprite>("Sprites/Items/" + iconSlug);
        itemPrefab = Resources.Load<GameObject>("Items/" + prefabSlug);
        canStack = stackable;
        itemHealth = health;
    }
}