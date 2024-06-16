using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Building : NetworkBehaviour
{
    // max distance away to delete prefab
    [SerializeField] private float maxDistance;

    // an array of prefabs to spawn
    [SerializeField] private GameObject[] buildingPrefabs;

    // material displayed for available place
    [SerializeField] private Material availablePlace;
    // material displayed for blocked place
    [SerializeField] private Material blockedPlace;

    // list of the spawnables owned by the player
    List<GameObject> ownedPrefabs = new List<GameObject>();

    // referance to the preview prefab
    private GameObject previewPrefab;

    // used to tell if the player can place
    private bool canPlace;

    // used to tell if the player is building
    private bool isBuilding = false;

    // the camera used to determin where the player is looking
    Camera m_Camera;

    // the current prefab the server is spawning
    public int s_CurrentPrefab = 0;

    // output for raycast
    private RaycastHit hit;

    // Use this for initialization
    void Start()
    {
        // gets the camera used to tell where to spawn object
        m_Camera = GetComponentInChildren<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // if we are not the local player then return
        if (!isLocalPlayer)
            return;

        // nots building
        isBuilding = false;

        // if the player is building
        if (isBuilding)
        {
            // allows user to switch item
            ChangeItem();
            // checks if user wants to place an item
            GetInput();
            // display the preview for the local player
            LocalShowPreview();
        }
        else
        {
            // if there is a preview prefab
            if (previewPrefab != null)
            {
                // destroy it
                Destroy(previewPrefab);
            }
        }
    }

    // allows the user to choose which item to place
    private void ChangeItem()
    {
        // allows user to choose what to spawn
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // changes current object to new object
            s_CurrentPrefab = 0;
            // switch to the second object
            CmdChangeObject(s_CurrentPrefab);
            // if there is already a preview prefab
            if (previewPrefab != buildingPrefabs[s_CurrentPrefab])
            {
                // destroy it
                Destroy(previewPrefab);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // changes current object to new object
            s_CurrentPrefab = 1;
            // switch to the second object
            CmdChangeObject(s_CurrentPrefab);
            // if there is already a preview prefab
            if (previewPrefab != buildingPrefabs[s_CurrentPrefab])
            {
                // destroy it
                Destroy(previewPrefab);
            }

        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // changes current object to new object
            s_CurrentPrefab = 2;
            // switch to the second object
            CmdChangeObject(s_CurrentPrefab);
            // if there is already a preview prefab
            if (previewPrefab != buildingPrefabs[s_CurrentPrefab])
            {
                // destroy it
                Destroy(previewPrefab);
            }

        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            // changes current object to new object
            s_CurrentPrefab = 3;
            // switch to the second object
            CmdChangeObject(s_CurrentPrefab);
            // if there is already a preview prefab
            if (previewPrefab != buildingPrefabs[s_CurrentPrefab])
            {
                // destroy it
                Destroy(previewPrefab);
            }

        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            // changes current object to new object
            s_CurrentPrefab = 4;
            // switch to the second object
            CmdChangeObject(s_CurrentPrefab);
            // if there is already a preview prefab
            if (previewPrefab != buildingPrefabs[s_CurrentPrefab])
            {
                // destroy it
                Destroy(previewPrefab);
            }

        }
    }
    
    // gets the user input and contacts the server
    private void GetInput()
    {
        // if the player left clicks
        if (Input.GetKeyDown(KeyCode.Mouse0) && canPlace)
        {
            // tell the server to place a object
            CmdPlaceObject(previewPrefab.transform.position, previewPrefab.transform.rotation);
        }
        // if the player right clicks
        else if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            // tell the server to remove a object at a 10 meter raycast
            CmdRemoveObject();
        }
    }

    // shows a preview of where the item will be built client side
    private void LocalShowPreview()
    {
        // casts ray cast to place item where raycast hits
        if (Physics.Raycast(transform.position, m_Camera.transform.forward, out hit, maxDistance))
        {
            // if this is not us
            if (hit.transform != transform)
            {                    
                // gets player rotation
                Quaternion playerRotation = transform.rotation;
                // if there is not a current prefab
                if (previewPrefab == null)
                {
                    // spawn the new preview prefab
                    previewPrefab = Instantiate(buildingPrefabs[s_CurrentPrefab], hit.point, playerRotation) as GameObject;
                    // turns off collisions
                    previewPrefab.GetComponentInChildren<MeshCollider>().enabled = false;
                }
                else
                {
                    // spawns object
                    previewPrefab.transform.position = hit.point;
                    previewPrefab.transform.rotation = playerRotation;
                }

                // if the hit was further then 10 meters user cannot place
                if (hit.distance > 10)
                {
                    // changes material of prefab
                    previewPrefab.GetComponentInChildren<Renderer>().material = blockedPlace;
                    // tells the game place can't place block
                    canPlace = false;
                }
                else
                {
                    // changes material of prefab
                    previewPrefab.GetComponentInChildren<Renderer>().material = availablePlace;
                    // tells the game place can place block
                    canPlace = true;
                }
            }
        }
    }

    [Command] // called on server
    private void CmdChangeObject(int index)
    {
        // changes current object to new object
        s_CurrentPrefab = index;
    }

    [Command] // called on server
    private void CmdPlaceObject(Vector3 position, Quaternion rotation)
    {
        // spawns object
        GameObject platform = Instantiate(buildingPrefabs[s_CurrentPrefab], position, rotation) as GameObject;
        // adds it to the list of owned prefabs
        ownedPrefabs.Add(platform);
        // spawns object for clients
        NetworkServer.Spawn(platform);
    }

    [Command] // called on server
    private void CmdRemoveObject()
    {    
        // if the play hits an object in 10 meters
        if (Physics.Raycast(transform.position, m_Camera.transform.forward, out hit, maxDistance))
        {
            // if it is owned by the player
            if (ownedPrefabs.Contains(hit.transform.parent.gameObject))
            {
                // despawn the object
                Destroy(hit.transform.parent.gameObject);
                ownedPrefabs.Remove(hit.transform.parent.gameObject);
            }
        }
    }
}
