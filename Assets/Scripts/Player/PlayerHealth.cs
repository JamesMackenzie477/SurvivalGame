using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : NetworkBehaviour {

    // this is the speed in which the health will decrease
    [SerializeField] private float decSpeed;

    // player vitals which are synced with every client
    [SyncVar] public float health = 100;
    [SyncVar] public float hunger = 100;
    [SyncVar] public float thirst = 100;

    // sets if the player is dead
    public bool isDead = false;
    
    // will be used to tell if the player is sprinting
    private PlayerController playerScript;

    // used for initialization
    void Start()
    {
        // if we are not the server then return
        if (!isServer)
            return;

        // gets the player controller script
        playerScript = GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void FixedUpdate () {

        // if we are only the server
        if (!isServer)
            return;

        // if hunger and thirst is 0 then start to decrease health
        if (hunger <= 0 || thirst <= 0)
        {
            health -= decSpeed;
            hunger = 0;
            thirst = 0;
        }
        else
        {
            // decrease vitals over time
            // decrease faster if player is sprinting
            if (playerScript.isSprinting)
            {
                // decreases vitals two times faster
                hunger -= decSpeed * 2;
                thirst -= decSpeed * 2;
            }
            else
            {
                // decrease vitals normally
                hunger -= decSpeed;
                thirst -= decSpeed;
            }
        }

        // health regeneration
        if (hunger >= 60 && thirst >= 60 && health < 100 && health > 0)
        {
            // increase health over time
            health += decSpeed;
        }

        // fixes health because it's a float
        // also checks if player is dead
        // if so a bool is set to true
        if (health <= 0)
        {
            health = 0;
            isDead = true;
        }
        else if (health > 100)
        {
            health = 100;
        }
    }
}
